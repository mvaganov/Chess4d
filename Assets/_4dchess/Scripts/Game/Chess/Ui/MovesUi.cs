using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovesUi : MonoBehaviour {
	[ContextMenuItem(nameof(Clear), nameof(Clear)),
	 ContextMenuItem(nameof(RebuildUi), nameof(RebuildUi))]
	public ChessGame game;
	public MoveHistory chessMoves;
	public GameObject branchUiPrefab;
	public GameObject moveUiPrefab;
	public GameObject Notes;
	public TMPro.TMP_InputField notesInput;
	[ContextMenuItem(nameof(ShowTextTrue),nameof(ShowTextTrue)),
	ContextMenuItem(nameof(HideText), nameof(HideText)),]
	[SerializeField] private bool _showText = true;
	[SerializeField] private MemoryPool<MoveUi> _moveUiPool = new MemoryPool<MoveUi>();
	[SerializeField] private MemoryPool<BranchUi> _branchUiPool = new MemoryPool<BranchUi>();
	//private List<GameObject> branches = new List<GameObject>();

	public bool ShowText {
		get => _showText;
		set {
			bool rebuild = _showText != value;
			_showText = value;
			if (rebuild) {
				RebuildUi();
			}
		}
	}
	public string CurrentNotes {
		get => chessMoves.CurrentMove.notes;
		set {
			chessMoves.CurrentMove.notes = value;
			CurrentMoveUi.RefreshElement();
		}
	}
	public MoveUi CurrentMoveUi {
		get {
			MoveNode current = chessMoves.CurrentMove;
			return Find(m => m.move == current);
		}
	}

	private void Start() {
		_branchUiPool.onReclaim = b => {
			Transform branchParent = b.transform;
			for(int i = branchParent.childCount - 1; i >= 0; --i) {
				Transform child = branchParent.GetChild(i);
				MoveUi m = child.GetComponent<MoveUi>();
				if (m != null) {
					_moveUiPool.Reclaim(m);
					continue;
				}
				throw new Exception("move branch has a non-Move in it. what is going on here? " +
					child.name + " in " + branchParent.name);
			}
		};
		_moveUiPool.onReclaim = m => {
			m.activeMarker.SetActive(false);
		};
	}

	public MoveUi Find(Func<MoveUi, bool> predicate) {
		for (int i = transform.childCount - 1; i >= 0; --i) {
			Transform child = transform.GetChild(i);
			MoveUi moveUi = child.GetComponent<MoveUi>();
			if (moveUi != null && predicate(moveUi)) {
				return moveUi;
			}
			MoveUi[] moves = child.GetComponentsInChildren<MoveUi>();
			for (int j = 0; j < moves.Length; ++j) {
				if (predicate(moves[j])) {
					return moves[j];
				}
			}
		}
		return null;
	}

	public void OnMove(MoveNode move) {
		RebuildUi();
	}

	public void OnUndoMove(MoveNode move) {
		RebuildUi();
	}

	public void HideText() { ShowText = false; }
	public void ShowTextTrue() { ShowText = true; }

	public void RebuildUi() {
		Clear();
		Transform _transform = transform;
		List<List<MoveNode>> moves = chessMoves.GetMoveList();

		MoveUi moveUi;
		// insert start game move
		moveUi = _moveUiPool.Get();//Instantiate(moveUiPrefab).GetComponent<MoveUi>();
		moveUi.Move = moves[moves.Count - 1][0].prev;
		ApplyToLayoutTransform(moveUi.transform, _transform);
		//Debug.Log($"moves {moves.Count}: [{string.Join(", ",moves.ConvertAll(l=>l.Count.ToString()))}]");

		for (int i = moves.Count-1; i >= 0; --i) {
			List<MoveNode> moveOptions = moves[i];
			//Debug.Log($"{moveOptions.Count}: [{string.Join(", ", moveOptions.ConvertAll(m => m.ToString()))}]");
			if (moveOptions.Count == 0) { continue; }
			BranchUi branch = _branchUiPool.Get();//Instantiate(branchUiPrefab);
			for (int m = 0; m < moveOptions.Count; ++m) {
				moveUi = _moveUiPool.Get(); //Instantiate(moveUiPrefab).GetComponent<MoveUi>();
				moveUi.Move = moveOptions[m];
				if (!_showText) {
					moveUi.label.gameObject.SetActive(false);
				}
				ApplyToLayoutTransform(moveUi.transform, branch.transform);
			}
			ApplyToLayoutTransform(branch.transform, _transform);
		}
		RefreshLayouts();
		notesInput?.SetTextWithoutNotify(chessMoves.CurrentMove.notes);
		//Debug.Log(CurrentMoveUi);
	}

	public void RefreshLayouts() {
		LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
	}


	private void ApplyToLayoutTransform(Transform item, Transform layoutTransform) {
		item.SetParent(layoutTransform, false);
		item.gameObject.SetActive(true);
		LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform.GetComponent<RectTransform>());
	}

	public void Clear() {
		for(int i = transform.childCount-1; i >= 0; --i) {
			GameObject go = transform.GetChild(i).gameObject;
			if (go == branchUiPrefab || go == moveUiPrefab) { continue; }
			BranchUi branchui;
			MoveUi moveui;
			if ((branchui = go.GetComponent<BranchUi>()) != null) {
				_branchUiPool.Reclaim(branchui);
			} else if ((moveui = go.GetComponent<MoveUi>()) != null) {
				_moveUiPool.Reclaim(moveui);
			} else {
				ChessGame.DestroyChessObject(go);
			}
		}
	}
}

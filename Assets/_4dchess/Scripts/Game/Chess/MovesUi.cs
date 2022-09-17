using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovesUi : MonoBehaviour {
	[ContextMenuItem(nameof(Clear), nameof(Clear)),
	 ContextMenuItem(nameof(RebuildUi), nameof(RebuildUi))]
	public ChessGame game;
	public Moves chessMoves;
	public GameObject branchUiPrefab;
	public GameObject moveUiPrefab;
	public void OnMove(Move move) {
		RebuildUi();
	}

	public void OnUndoMove(Move move) {
		RebuildUi();
	}

	public List<GameObject> branches = new List<GameObject>();

	public void RebuildUi() {
		Clear();
		Transform _transform = transform;
		List<Move> moves = chessMoves.GetMoveList();
		for (int i = moves.Count-1; i >= 0; --i) {
			GameObject branch = Instantiate(branchUiPrefab);
			MoveUi moveUi = Instantiate(moveUiPrefab).GetComponent<MoveUi>();
			Move move = moves[i];
			moveUi.move = move;
			int teamIndex = game.teams.IndexOf(move.pieceMoved.team);
			Piece piece = move.pieceMoved;
			moveUi.icon.sprite = System.Array.Find(game.pieceCodes, code => code.code == piece.code).icons[teamIndex];
			moveUi.label.text = move.ToString();
			if (move == chessMoves.CurrentMove) {
				moveUi.activeMarker.SetActive(true);
			}
			moveUi.transform.SetParent(branch.transform, false);
			branch.transform.SetParent(_transform, false);
			moveUi.gameObject.SetActive(true);
			branch.gameObject.SetActive(true);
		}
	}

	public void Clear() {
		for(int i = transform.childCount-1; i >= 0; --i) {
			GameObject go = transform.GetChild(i).gameObject;
			if (go == branchUiPrefab || go == moveUiPrefab) { continue; }
			ChessGame.DestroyObject(go);
		}
		//for(int i = branches.Count-1; i >= 0; --i) {
		//	ChessGame.DestroyObject(branches[i]);
		//}
		//branches.Clear();
	}
}

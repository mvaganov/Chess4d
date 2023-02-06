using System;
using UnityEngine;

public class MoveUi : MonoBehaviour {
	public GameObject activeMarker;
	public UnityEngine.UI.Image icon;
	public TMPro.TMP_Text label;
	[SerializeField] public MoveNode move;

	public MoveNode Move {
		get => move;
		set {
			move = value;
			RefreshElement();
		}
	}

	public void RefreshElement() {
		BasicMove bmove = move.move as BasicMove;
		Piece piece = bmove != null ? bmove.pieceMoved : null;
		ChessGame game;
		if (piece != null) {
			int teamIndex = piece.team.TeamIndex;
			game = piece.team.game;
			icon.sprite = System.Array.Find(game.pieceCodes, code => code.code == piece.code).icons[teamIndex];
		} else {
			game = FindObjectOfType<ChessGame>();
		}
		if (move == game.chessMoves.CurrentMoveNode) {
			activeMarker.SetActive(true);
		}
		label.text = move.ToString();
	}

	public void GoToMove() {
		MovesUi movesUi = GetComponentInParent<MovesUi>();
		if (movesUi == null) {
			throw new System.Exception($"{nameof(MoveUi)} should be a child of {nameof(MovesUi)}");
		}
		if (move == movesUi.chessMoves.CurrentMoveNode) {
			ActivateNotes();
			return;
		}
		//Debug.Log($"going to {move.index} {move}");
		movesUi.chessMoves.GoToMove(move);
		movesUi.RebuildUi();
	}

	public void ActivateNotes() {
		MovesUi movesUi = GetComponentInParent<MovesUi>();
		movesUi.Notes.SetActive(true);
		movesUi.Notes.transform.position = movesUi.CurrentMoveUi.transform.position;
		movesUi.notesInput.SetTextWithoutNotify(move.Notes);
		movesUi.notesInput.Select();
	}
}

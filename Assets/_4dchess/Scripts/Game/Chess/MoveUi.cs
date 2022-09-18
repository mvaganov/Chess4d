using System;
using UnityEngine;

public class MoveUi : MonoBehaviour {
	public GameObject activeMarker;
	public UnityEngine.UI.Image icon;
	public TMPro.TMP_Text label;
	[SerializeField] public Move move;

	public Move Move {
		get => move;
		set {
			move = value;
			RefreshElement();
		}
	}

	public void RefreshElement() {
		Piece piece = move.pieceMoved;
		if (piece != null) {
			int teamIndex = piece.team.TeamIndex;
			ChessGame game = piece.team.game;
			icon.sprite = System.Array.Find(game.pieceCodes, code => code.code == piece.code).icons[teamIndex];
			if (move == game.chessMoves.CurrentMove) {
				activeMarker.SetActive(true);
			}
		}
		label.text = move.ToString();
	}

	public void GoToMove() {
		MovesUi movesUi = GetComponentInParent<MovesUi>();
		if (movesUi == null) {
			throw new System.Exception($"{nameof(MoveUi)} should be a child of {nameof(MovesUi)}");
		}
		if (move == movesUi.chessMoves.CurrentMove) {
			movesUi.Notes.SetActive(true);
			movesUi.Notes.transform.position = movesUi.CurrentMoveUi.transform.position;
			movesUi.notesInput.SetTextWithoutNotify(move.notes);
			movesUi.notesInput.Select();
			return;
		}
		//Debug.Log($"going to {move.index} {move}");
		movesUi.chessMoves.GoToMove(move);
		movesUi.RebuildUi();
	}
}

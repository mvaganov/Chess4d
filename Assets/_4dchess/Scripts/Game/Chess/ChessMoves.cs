using System.Collections.Generic;
using UnityEngine;

public class ChessMoves : MonoBehaviour {
	public class Move {
		public Coord from, to, fromCaptured;
		public Piece pieceMoved;
		public Piece pieceCaptured;
		public int timestamp;
		public List<Move> next;
		public Move prev;
		public string notes;

		public Move(Piece pieceMoved, Coord from, Coord to, Piece pieceCaptured, Coord fromCaptured, string notes) {
			this.from = from;
			this.to = to;
			this.fromCaptured = fromCaptured;
			this.pieceMoved = pieceMoved;
			this.pieceCaptured = pieceCaptured;
			this.notes = notes;
			timestamp = System.Environment.TickCount;
			next = new List<Move>();
			prev = null;
		}

		public bool IsRoot => prev == null;

		public void Do() {
			Debug.Log(this);
			pieceMoved.MoveTo(to);
			if (pieceCaptured != null) {
				Capture(pieceMoved, pieceCaptured, fromCaptured);
			}
		}

		public void Undo() {
			Debug.Log("undo " + this);
			pieceMoved.MoveTo(from);
			if (pieceCaptured != null) {
				Uncapture(pieceMoved, pieceCaptured, fromCaptured);
			}
		}

		private static void Capture(Piece attacker, Piece captured, Coord capturedLocation) {
			Transform holdingArea = attacker.team.transform;
			captured.transform.SetParent(holdingArea);
			Vector3 holdingLocation = Vector3.right * (holdingArea.childCount - 1) / 2f;
			captured.JumpToLocalCenter(holdingLocation, 3);
		}

		private static void Uncapture(Piece attacker, Piece captured, Coord capturedLocation) {
			Board b = attacker.board;
			Tile tile = b.GetTile(capturedLocation);
			captured.transform.SetParent(tile.transform);
			captured.JumpToLocalCenter(Vector3.zero, 3);
		}

		public override string ToString() {
			if (pieceMoved == null) {
				return notes;
			}
			// https://en.wikipedia.org/wiki/Algebraic_notation_(chess)
			// TODO look for other peices of the same type on the same board that are also able to move to the given coord.
			// if there is more than one, prepend the column id of from.
			// if there are multiple in the same column, provide the row id instead
			// if there are multiple in both row and column, provide the entire from coordinate.
			if (pieceCaptured != null) {
				string identifier = pieceMoved.code;
				if (identifier == "") {
					identifier = from.ColumnId;
				}
				return $"{identifier}x{to}" + NotesSuffix();
			} else {
				string identifier = pieceMoved.code;
				return $"{identifier}{to}" + NotesSuffix();
			}
		}

		private string NotesSuffix() => !string.IsNullOrEmpty(notes) ? $" {notes}" : "";
	}
	
	private Move currentMove = new Move(null, Coord.zero, Coord.zero, null, Coord.zero, "game begins");

	public Move CurrentMove => currentMove;

	public void MakeMove(Piece pieceMoved, Coord from, Coord to, Piece pieceCaptured, Coord fromCaptured, string notes) {
		Move move = new Move(pieceMoved, from, to, pieceCaptured, fromCaptured, notes);
		currentMove.next.Insert(0, move);
		move.prev = currentMove;
		currentMove = move;
		currentMove.Do();
	}

	public bool UndoMove() {
		if(currentMove.IsRoot) { return false; }
		currentMove.Undo();
		currentMove = currentMove.prev;
		return true;
	}

	public List<Move> GetNextMoves() {
		return currentMove.next;
	}

	public bool RedoMove(int index) {
		if (index < 0 || index >= currentMove.next.Count) { return false; }
		currentMove = currentMove.next[index];
		currentMove.Do();
		return true;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Moves : MonoBehaviour {
	private Move currentMove = new Move(null, Coord.zero, Coord.zero, null, Coord.zero, "game begins");
	public MoveEventHandler onMove;
	public MoveEventHandler onUndoMove;

	public Move CurrentMove => currentMove;

	[System.Serializable] public class MoveEventHandler : UnityEvent<Move> { }

	public List<Move> GetMoveList() {
		List<Move> list = new List<Move>();
		Move last = currentMove;
		while (last.next.Count > 0) {
			last = last.next[0];
		}
		Move cursor = last;
		do {
			list.Add(cursor);
			cursor = cursor.prev;
		} while (cursor != null && !cursor.IsRoot);
		return list;
	}

	public void MakeMove(Piece pieceMoved, Coord from, Coord to, Piece pieceCaptured, Coord fromCaptured, string notes) {
		Move move = new Move(pieceMoved, from, to, pieceCaptured, fromCaptured, notes);
		currentMove.next.Insert(0, move);
		move.prev = currentMove;
		move.Do();
		currentMove = move;
		onMove?.Invoke(currentMove);
	}

	public bool UndoMove() {
		if(currentMove.IsRoot) { return false; }
		currentMove.Undo();
		currentMove = currentMove.prev;
		onUndoMove?.Invoke(CurrentMove);
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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Moves : MonoBehaviour {
	private Move currentMove = new Move(0, null, Coord.zero, Coord.zero, null, Coord.zero, "game begins");
	public MoveEventHandler onMove;
	public MoveEventHandler onUndoMove;

	public Move CurrentMove => currentMove;

	[System.Serializable] public class MoveEventHandler : UnityEvent<Move> { }

	public List<List<Move>> GetMoveList() {
		List<List<Move>> list = new List<List<Move>>();
		Move last = currentMove;
		while (last.next.Count > 0) {
			last = last.next[0];
		}
		Move cursor = last;
		do {
			list.Add(cursor.next);
			cursor = cursor.prev;
		} while (cursor != null);
		return list;
	}

	public void MakeMove(Piece pieceMoved, Coord from, Coord to, Piece pieceCaptured, Coord fromCaptured,
	string notes) {
		int currentIndex = currentMove.index;
		Move move = new Move(currentIndex+1, pieceMoved, from, to, pieceCaptured, fromCaptured, notes);
		currentMove.next.Insert(0, move);
		move.prev = currentMove;
		move.Do();
		currentMove = move;
		onMove?.Invoke(currentMove);
	}

	public void GoToMove(Move targetMove) {
		int actualIndexToTravelTo = targetMove.index - 1;
		Move next = null;
		do {
			if (currentMove.index < actualIndexToTravelTo) {
				next = currentMove.next.Count > 0 ? currentMove.next[0] : null;
				if (next == null) {
					throw new Exception($"can't go to next[0] after {currentMove.index} {currentMove}");
				}
				next.Do();
				currentMove = next;
			} else if (currentMove.index > actualIndexToTravelTo) {
				currentMove.Undo();
				next = currentMove.prev;
				if (next == null && actualIndexToTravelTo > 0) {
					throw new Exception($"can't go to move before {currentMove.index} {currentMove}");
				}
				if (next != null) {
					currentMove = next;
				}
			}
		} while (currentMove.index != actualIndexToTravelTo && next != null);
		if (actualIndexToTravelTo != currentMove.index) {
			return;
		}
		next = currentMove.next.Count > targetMove.BranchIndex ? currentMove.next[targetMove.BranchIndex] : null;
		if (next == null) {
			throw new Exception($"can't go to next[{targetMove.BranchIndex}] after {currentMove.index} {currentMove}");
		}
		currentMove.next.RemoveAt(targetMove.BranchIndex);
		currentMove.next.Insert(0, next);
		next.Do();
		currentMove = next;
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

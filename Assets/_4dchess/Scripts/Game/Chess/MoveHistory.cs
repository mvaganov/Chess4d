using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MoveHistory : MonoBehaviour {
	private MoveNode currentMove = new MoveNode(0, null, "game begins");
	public MoveEventHandler onMove;
	public MoveEventHandler onUndoMove;

	public MoveNode CurrentMove => currentMove;

	[System.Serializable] public class MoveEventHandler : UnityEvent<MoveNode> { }

	public void SetCurrentMove(MoveNode moveNode) {
		currentMove = moveNode;
	}

	public MoveNode FindMoveNode(Move move) {
		MoveNode n = currentMove;
		if (n.move == move) { return n; }
		// TODO search backwards along the current timeline first... if the node isn't found there,
		// then do the exhaustive resursive search
		n = GetRoot();
		return n.FindMoveRecursive(move);
	}

	public MoveNode GetRoot() {
		MoveNode cursor = currentMove;
		while (cursor.prev != null) {
			cursor = cursor.prev;
		}
		return cursor;
	}

	public List<List<MoveNode>> GetMoveList() {
		List<List<MoveNode>> list = new List<List<MoveNode>>();
		MoveNode last = currentMove;
		while (last.next.Count > 0) {
			last = last.next[0];
		}
		MoveNode cursor = last;
		do {
			list.Add(cursor.next);
			cursor = cursor.prev;
		} while (cursor != null);
		return list;
	}

	public void MakeMove(Move move, string notes) {
		DoThis(new MoveNode(currentMove.index + 1, move, notes));
	}
	//public void MakeMove(Piece pieceMoved, Coord from, Coord to, string notes) {
	//	DoThis(new MoveNode(currentMove.index + 1, new Move(pieceMoved, from, to), notes));
	//}
	//public void MakeCapture(Piece pieceMoved, Coord from, Coord to, Piece pieceCaptured, Coord fromCaptured,
	//string notes) {
	//	DoThis(new MoveNode(currentMove.index + 1, new Capture(pieceMoved, from, to, pieceCaptured, fromCaptured), notes));
	//}

	public void DoThis(MoveNode move) {
		int doneAlready = currentMove.next.IndexOf(move);
		if (doneAlready >= 0) {
			move = currentMove.next[doneAlready];
			Debug.Log("repeat!");
			currentMove.next.RemoveAt(doneAlready);
		}
		Debug.Log("doing "+move+" "+move.GetType().Name);
		currentMove.next.Insert(0, move);
		move.prev = currentMove;
		move.Do();
		currentMove = move;
		onMove?.Invoke(currentMove);
	}

	public void GoToMove(MoveNode targetMove) {
		int actualIndexToTravelTo = targetMove.index - 1;
		MoveNode next = null;
		HashSet<Board> boards = new HashSet<Board>();
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
			if (currentMove.move != null && currentMove.move.pieceMoved != null) {
				boards.Add(currentMove.move.pieceMoved.board);
			}
		} while (currentMove.index != actualIndexToTravelTo && next != null);
		if (actualIndexToTravelTo == currentMove.index) {
			next = currentMove.next.Count > targetMove.BranchIndex ? currentMove.next[targetMove.BranchIndex] : null;
			if (next == null) {
				throw new Exception($"can't go to next[{targetMove.BranchIndex}] after {currentMove.index} {currentMove}");
			}
			currentMove.next.RemoveAt(targetMove.BranchIndex);
			currentMove.next.Insert(0, next);
			next.Do();
			currentMove = next;
		}
		// TODO refresh UI, like chessVisuals.ResetPieceSelectionVisuals();
		foreach (Board board in boards) {
			board.RecalculatePieceMoves();
		}
	}

	public bool UndoMove() {
		if(currentMove.IsRoot) { return false; }
		currentMove.Undo();
		currentMove = currentMove.prev;
		onUndoMove?.Invoke(CurrentMove);
		return true;
	}

	public List<MoveNode> GetNextMoves() {
		return currentMove.next;
	}

	public bool RedoMove(int index) {
		if (index < 0 || index >= currentMove.next.Count) { return false; }
		currentMove = currentMove.next[index];
		currentMove.Do();
		onMove?.Invoke(currentMove);
		return true;
	}
}

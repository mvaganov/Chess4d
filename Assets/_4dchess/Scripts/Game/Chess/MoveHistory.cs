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

	public int CountMovesSinceCaptureOrPawnAdvance(Board board) {
		int count = 0;
		MoveNode node = CurrentMove;
		while (node != null && node.move != null) {
			if (node.move.board == board && (node.move is Capture || (node.move is Move m && m.pieceMoved.code == "P"))) {
				break;
			}
			node = node.prev;
			++count;
		}
		count += board.halfMovesSinceCaptureOrPawnMove;
		return count;
	}


	public void SetCurrentMove(MoveNode moveNode) {
		currentMove = moveNode;
	}

	/// <summary>
	/// find the point in history (or alternate timelines) that a specific piece moved from 'from' to 'to'.
	/// </summary>
	public MoveNode FindMoveNode(Move move) {
		MoveNode n = currentMove;
		// if we're at the node we're looking for, return it now. that was easy.
		if (n.move == move) { return n; }
		// look for nodes along this node's direct history, until beginning, or a branch in the timeline is found
		while (n.prev != null && n.prev.FutureTimelineCount > 1) {
			n = n.prev;
			if (n == null) { return null; }
			if (n.move == move) { return n; }
		}
		HashSet<MoveNode> branchesToIgnore = new HashSet<MoveNode>();
		// count this branch as fully ignored
		branchesToIgnore.Add(n);
		// and do a full check of the future (if it exists). if the node is in the future, get it
		n = CurrentMove.FindMoveRecursive(move, null);
		if (n != null) { return n; }
		// then do the exhaustive resursive search starting from the very beginning, ignoring the searched branch
		n = GetRoot();
		return n.FindMoveRecursive(move, branchesToIgnore);
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
		while (last.FutureTimelineCount > 0) {
			last = last.KnownNextMove;
		}
		MoveNode cursor = last;
		do {
			list.Add(cursor.GetAllTimelineBranches());
			cursor = cursor.prev;
		} while (cursor != null);
		return list;
	}

	public void MakeMove(Move move, string notes) {
		DoThis(new MoveNode(currentMove.turnIndex + 1, move, notes));
	}

	public void DoThis(MoveNode move) {
		int doneAlready = currentMove.IndexOfBranch(move);
		if (doneAlready >= 0) {
			//currentMove.GetTimelineBranch(doneAlready);
			move = currentMove.PopTimeline(doneAlready);
		}
		Debug.Log("doing " + move + " " + move.move.GetType().Name);
		currentMove.SetAsNextTimelineBranch(move);
		move.prev = currentMove;
		//Debug.Log("added timeline " + currentMove.IndexOfBranch(move));
		move.Do();
		currentMove = move;
		onMove?.Invoke(currentMove);
	}

	public void GoToMove(MoveNode targetMove) {
		int actualIndexToTravelTo = targetMove.turnIndex - 1;
		MoveNode next = null;
		HashSet<Board> boards = new HashSet<Board>();
		do {
			if (currentMove.turnIndex < actualIndexToTravelTo) {
				next = currentMove.FutureTimelineCount > 0 ? currentMove.KnownNextMove : null;
				if (next == null) {
					throw new Exception($"can't go to next[0] after {currentMove.turnIndex} {currentMove}");
				}
				next.Do();
				currentMove = next;
			} else if (currentMove.turnIndex > actualIndexToTravelTo) {
				currentMove.Undo();
				next = currentMove.prev;
				if (next == null && actualIndexToTravelTo > 0) {
					throw new Exception($"can't go to move before {currentMove.turnIndex} {currentMove}");
				}
				if (next != null) {
					currentMove = next;
				}
			}
			if (currentMove.move != null && currentMove.move.pieceMoved != null) {
				boards.Add(currentMove.move.pieceMoved.board);
			}
		} while (currentMove.turnIndex != actualIndexToTravelTo && next != null);
		if (actualIndexToTravelTo == currentMove.turnIndex) {
			next = currentMove.FutureTimelineCount > targetMove.BranchIndex
				? currentMove.GetTimelineBranch(targetMove.BranchIndex) : null;
			if (next == null) {
				throw new Exception($"can't go to next[{targetMove.BranchIndex}] after {currentMove.turnIndex} {currentMove}");
			}
			currentMove.PopTimeline(targetMove.BranchIndex);
			currentMove.SetAsNextTimelineBranch(next);
			next.Do();
			currentMove = next;
		}
		// TODO refresh UI, like chessVisuals.ResetPieceSelectionVisuals();
		foreach (Board board in boards) {
			board.RecalculatePieceMoves();
		}
	}

	public bool UndoMove() {
		if (currentMove.IsRoot) { return false; }
		currentMove.Undo();
		currentMove = currentMove.prev;
		onUndoMove?.Invoke(CurrentMove);
		return true;
	}

	public bool RedoMove() {
		if (currentMove.FutureTimelineCount > 0) { return false; }
		currentMove = currentMove.KnownNextMove;
		currentMove.Do();
		onMove?.Invoke(currentMove);
		return true;
	}
}

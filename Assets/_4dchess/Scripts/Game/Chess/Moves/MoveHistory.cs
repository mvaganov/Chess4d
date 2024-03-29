using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MoveHistory : MonoBehaviour {
	private MoveNode currentMoveNode;
	public MoveEventHandler onMove;
	public MoveEventHandler onUndoMove;
	public ChessGame game;
	private ChessVisuals chessVis;
	public MoveCalculator moveCalculator;

	public int NextMoveIndex => currentMoveNode.turnIndex + 1;
	public MoveNode CurrentMoveNode => currentMoveNode;
	public IGameMoveBase CurrentMove => CurrentMoveNode != null ? CurrentMoveNode.move : null;
	private ChessVisuals ChessVisuals => chessVis != null ? chessVis : chessVis = FindObjectOfType<ChessVisuals>();

	[System.Serializable] public class MoveEventHandler : UnityEvent<MoveNode> { }

	private void Awake() {
		if (game == null) { game = FindObjectOfType<ChessGame>(); }
		currentMoveNode = moveCalculator.GetMoveNode(0, new StartGame(game.GameBoard));
	}

	public int CountMovesSinceCaptureOrPawnAdvance(Board board) {
		int count = 0;
		MoveNode node = CurrentMoveNode;
		while (node != null && node.move != null) {
			if (node.move.Board == board && (node.move is PieceMoveAttack || (node.move is PieceMove m
			&& m.pieceMoved != null && m.pieceMoved.code == "P"))) {
				break;
			}
			node = node.Prev;
			++count;
		}
		count += board.halfMovesSinceCaptureOrPawnMove;
		return count;
	}


	public void SetCurrentMove(MoveNode moveNode) {
		currentMoveNode = moveNode;
	}

	/// <summary>
	/// find the point in history (or alternate timelines) that a specific piece moved from 'from' to 'to'.
	/// </summary>
	public MoveNode FindMoveNode(IGameMoveBase move) {
		MoveNode n = currentMoveNode;
		//Debug.Log("start "+n);
		// if we're at the node we're looking for, return it now. that was easy.
		if (n.move == move) { return n; }
		// look for nodes along this node's direct history, until beginning, or a branch in the timeline is found
		while (n.Prev != null && n.Prev.FutureTimelineCount > 1) {
			n = n.Prev;
			//Debug.Log("traverse " + n);
			if (n == null) { return null; }
			if (n.move == move) { return n; }
		}
		HashSet<MoveNode> branchesToIgnore = new HashSet<MoveNode>();
		// count this branch as fully ignored
		branchesToIgnore.Add(n);
		// and do a full check of the future (if it exists). if the node is in the future, get it
		n = CurrentMoveNode.FindMoveRecursive(move, null);
		//Debug.Log("found recursive future? " + n);
		if (n != null) { return n; }
		// then do the exhaustive resursive search starting from the very beginning, ignoring the searched branch
		n = GetRoot();
		n = n.FindMoveRecursive(move, branchesToIgnore);
		//Debug.Log("found recursive past? " + n);
		return n;
	}

	public MoveNode GetRoot() {
		MoveNode cursor = currentMoveNode;
		while (cursor.Prev != null) {
			cursor = cursor.Prev;
		}
		return cursor;
	}

	public List<List<MoveNode>> GetMoveList() {
		List<List<MoveNode>> list = new List<List<MoveNode>>();
		MoveNode last = currentMoveNode;
		while (last.FutureTimelineCount > 0) {
			last = last.KnownNextMove;
		}
		MoveNode cursor = last;
		do {
			list.Add(cursor.GetAllTimelineBranches());
			cursor = cursor.Prev;
		} while (cursor != null);
		return list;
	}

	public void MakeMove(IGameMoveBase move) {
		MoveNode moveNode = moveCalculator.GetMoveNode(NextMoveIndex, move);
		DoThis(moveNode);
		ChessVisuals.GenerateHints(currentMoveNode);
		// TODO check if one of the latest moves is check for either king
	}

	public void DoThis(MoveNode move) {
		int doneAlready = currentMoveNode.IndexOfBranch(move);
		if (doneAlready >= 0) {
			//currentMove.GetTimelineBranch(doneAlready);
			move = currentMoveNode.PopTimeline(doneAlready);
		}
		//BasicMove pmove = move.move as BasicMove;
		//Piece piece = pmove.pieceMoved;
		//Debug.Log(piece.name + " " + move.move.GetType().Name + " " + move);
		AnnounceTurnOrder(move);
		currentMoveNode.SetAsNextTimelineBranch(move);
		move.Prev = currentMoveNode;
		//Debug.Log("added timeline " + currentMove.IndexOfBranch(move));
		move.Do();
		currentMoveNode = move;
		onMove?.Invoke(currentMoveNode);
	}

	// TODO make a new class for user output and put this in there
	public void AnnounceCurrentTurn() {
		AnnounceTurnOrder(currentMoveNode);
	}

	// TODO make a new class for user output and put this in there
	private void AnnounceTurnOrder(MoveNode move) {
		int whoShouldBeGoing = game.GetWhosTurnItIs();
		if (move != null && move.move != null && move.move is PieceMove pmove
		&& pmove.pieceMoved.team.TeamIndex != whoShouldBeGoing) {
			Piece piece = pmove.pieceMoved;
			string message = piece.team.name + " went out of turn, it should be " +
				game.teams[whoShouldBeGoing].name + "'s turn";
			if (game.message != null) {
				game.message.Text = message;
			}
			Debug.Log(message);
		} else {
			int whosNext;
			if (move == null || move.move == null) {
				whosNext = game.WhoStartsTheGame;
			} else {
				whosNext = game.GetWhosTurnItIsNext();
			}
			if (game.message != null) {
				game.message.Text = game.teams[whosNext].name + "'s turn";
			}
		}

	}

	public void GoToMove(MoveNode targetMove) {
		int actualIndexToTravelTo = targetMove.turnIndex - 1;
		MoveNode next = null;
		HashSet<Board> boards = new HashSet<Board>();
		do {
			if (currentMoveNode.turnIndex < actualIndexToTravelTo) {
				next = currentMoveNode.FutureTimelineCount > 0 ? currentMoveNode.KnownNextMove : null;
				if (next == null) {
					throw new Exception($"can't go to next[0] after {currentMoveNode.turnIndex} {currentMoveNode}");
				}
				next.Do();
				currentMoveNode = next;
			} else if (currentMoveNode.turnIndex > actualIndexToTravelTo) {
				currentMoveNode.Undo();
				next = currentMoveNode.Prev;
				if (next == null && actualIndexToTravelTo > 0) {
					throw new Exception($"can't go to move before {currentMoveNode.turnIndex} {currentMoveNode}");
				}
				if (next != null) {
					currentMoveNode = next;
				}
			}
			if (currentMoveNode.move != null && currentMoveNode.move is PieceMove pmove && pmove.pieceMoved != null) {
				boards.Add(pmove.pieceMoved.board);
			}
		} while (currentMoveNode.turnIndex != actualIndexToTravelTo && next != null);
		if (actualIndexToTravelTo == currentMoveNode.turnIndex) {
			next = currentMoveNode.FutureTimelineCount > targetMove.BranchIndex
				? currentMoveNode.GetTimelineBranch(targetMove.BranchIndex) : null;
			if (next == null) {
				throw new Exception($"can't go to next[{targetMove.BranchIndex}] after {currentMoveNode.turnIndex} {currentMoveNode}");
			}
			currentMoveNode.PopTimeline(targetMove.BranchIndex);
			currentMoveNode.SetAsNextTimelineBranch(next);
			next.Do();
			currentMoveNode = next;
		}
		// TODO refresh UI, like chessVisuals.ResetPieceSelectionVisuals();
		foreach (Board board in boards) {
			board.RecalculatePieceMoves();
		}
	}

	public bool UndoMove() {
		if (currentMoveNode.IsRoot) { return false; }
		currentMoveNode.Undo();
		currentMoveNode = currentMoveNode.Prev;
		onUndoMove?.Invoke(CurrentMoveNode);
		return true;
	}

	public bool RedoMove() {
		if (currentMoveNode.FutureTimelineCount == 0) {
			//Debug.Log("unknown future for "+currentMove.move);
			return false;
		}
		currentMoveNode = currentMoveNode.KnownNextMove;
		currentMoveNode.Do();
		onMove?.Invoke(currentMoveNode);
		return true;
	}
}

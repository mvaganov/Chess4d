using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MoveNode {
	public IGameMoveBase move;
	public int turnIndex;
	public int timestamp;
	private List<MoveNode> next;
	private MoveNode prev;
	public GameState boardState;
	public Task calculationTask;

	public string Notes {
		get => boardState != null ? boardState.notes : null;
		set => boardState.notes = value;
	}
	public MoveNode Prev {
		get => prev;
		set {
			prev = value;
			boardState.prev = prev != null ? prev.boardState : null;
		}
	}
	public bool IsRoot => prev == null;

	public int BranchIndex => prev == null ? -1 : prev.next.IndexOf(this);

	public int FutureTimelineCount => next.Count;

	public MoveNode KnownNextMove => next[0];

	//public List<MoveNode> PossibleFutures => next;

	public int IndexOfBranch(MoveNode decision) { return next.IndexOf(decision); }

	public MoveNode GetTimelineBranch(int index) { return next[index]; }

	public void SetAsNextTimelineBranch(MoveNode next) {
		//Debug.Log(move+" -> "+next.move);
		this.next.Insert(0, next);
	}

	public MoveNode PopTimeline(int index) {
		MoveNode branch = GetTimelineBranch(index);
		next.RemoveAt(index);
		//Debug.Log("popping "+index+" "+move);
		return branch;
	}

	public List<MoveNode> GetAllTimelineBranches() { return next; }

	public MoveNode(int index, IGameMoveBase move, string notes) {
		Debug.Log("new node");
		this.move = move;
		this.turnIndex = index;
		timestamp = System.Environment.TickCount;
		next = new List<MoveNode>();
		prev = null;
		Calculate();
		this.Notes = notes;
	}

	// TODO make this properly asynchronous
	private GameState Calculate() {
		if (move == null) { return null; }
		List<IGameMoveBase> newMoves = new List<IGameMoveBase>();
		move.Board.game.moveNodeBeingProcessed = this;
		boardState = move.Board.Analysis.NewAnalysisAfter(move, newMoves);
		//yield return null;
		boardState.prev = prev != null ? prev.boardState : null;
		newMoves.RemoveAll(move => !move.IsValid);
		//yield return null;
		boardState.notableMoves = newMoves;
		//Debug.Log($"{move} new moves: {string.Join(", ", newMoves.ConvertAll(m => m.ToString()))}");
		return boardState;
	}

	protected string NotesSuffix() => !string.IsNullOrEmpty(Notes) ? $" {Notes}" : "";

	public virtual void Do() {
		move?.Do();
	}

	public virtual void Undo() {
		move?.Undo();
	}

	public override string ToString() {
		if (move == null) {
			return Notes;
		}
		string stats = boardState != null ? (" " + boardState.CalculateMoveStats().ToString()) : null;
		return $"{move}{NotesSuffix()}{stats}";
	}

	public override bool Equals(object obj) {
		return obj is MoveNode mn && mn.turnIndex == turnIndex && mn.move.Equals(move);
	}
	public override int GetHashCode() {
		return ((move != null) ? move.GetHashCode() : 0) ^ turnIndex;
	}

	public MoveNode FindMoveRecursive(IGameMoveBase m, HashSet<MoveNode> ignoreBranches) {
		if (move == m) { return this; }
		MoveNode found = null;
		for (int i = 0; i < next.Count; ++i) {
			MoveNode possibleNext = next[i];
			if (ignoreBranches != null && ignoreBranches.Contains(possibleNext)) { continue; }
			found = possibleNext.FindMoveRecursive(m, ignoreBranches);
			if (found != null) { break; }
		}
		return found;
	}
}

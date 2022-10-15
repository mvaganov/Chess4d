using System.Collections.Generic;
using UnityEngine;

public class MoveNode {
	public Move move;
	public int turnIndex;
	public int timestamp;
	private List<MoveNode> next;
	public MoveNode prev;
	public string notes;

	public bool IsRoot => prev == null;

	public int BranchIndex => prev == null ? -1 : prev.next.IndexOf(this);

	public int FutureTimelineCount => next.Count;

	public MoveNode KnownNextMove => next[0];

	public List<MoveNode> PossibleFutures => next;

	public int IndexOfBranch(MoveNode decision) { return next.IndexOf(decision); }

	public MoveNode GetTimelineBranch(int index) { return next[index]; }

	public void SetAsNextTimelineBranch(MoveNode next) { this.next.Insert(0, next); }

	public MoveNode PopTimeline(int index) {
		MoveNode branch = GetTimelineBranch(index);
		next.RemoveAt(index);
		return branch;
	}

	public List<MoveNode> GetAllTimelineBranches() { return next; }

	public MoveNode(int index, Move move, string notes) {
		this.move = move;
		this.notes = notes;
		this.turnIndex = index;
		timestamp = System.Environment.TickCount;
		next = new List<MoveNode>();
		prev = null;
	}

	protected string NotesSuffix() => !string.IsNullOrEmpty(notes) ? $" {notes}" : "";
	
	public virtual void Do() {
		move?.Do();
	}

	public virtual void Undo() {
		move?.Undo();
	}

	public override string ToString() {
		if (move == null) {
			return notes;
		}
		return $"{move}{NotesSuffix()}";
	}

	public override bool Equals(object obj) {
		return obj is MoveNode mn && mn.turnIndex == turnIndex && mn.move.Equals(move);
	}
	public override int GetHashCode() {
		return ((move != null) ? move.GetHashCode() : 0) ^ turnIndex;
	}

	public MoveNode FindMoveRecursive(Move m, HashSet<MoveNode> ignoreBranches) {
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

public class Move {
	public Coord from, to;
	public Piece pieceMoved;

	public Move(Piece pieceMoved, Coord from, Coord to) {
		this.from = from;
		this.to = to;
		this.pieceMoved = pieceMoved;
	}

	public Move(Move other) {
		from = other.from;
		to = other.to;
		pieceMoved = other.pieceMoved;
	}

	public virtual void Do() { pieceMoved?.DoMove(this); }

	public virtual void Undo() { pieceMoved?.UndoMove(this); }

	public override string ToString() {
		string identifier = pieceMoved.code;
		// https://en.wikipedia.org/wiki/Algebraic_notation_(chess)
		// TODO look for other peices of the same type on the same board that are also able to move to the given coord.
		// if there is more than one, prepend the column id of from.
		// if there are multiple in the same column, provide the row id instead
		// if there are multiple in both row and column, provide the entire from coordinate.
		return $"{identifier}{from}{to}";
	}

	public override bool Equals(object obj) {
		return obj.GetType() == typeof(Move) && DuckTypeEquals(obj as Move);
	}
	public virtual bool DuckTypeEquals(Move m) {
		return m.from == from && m.to == to && m.pieceMoved == pieceMoved;
	}
	public override int GetHashCode() {
		return from.GetHashCode() ^ to.GetHashCode() ^ pieceMoved.GetHashCode();
	}

	public virtual TiledGameObject MakeMark(MemoryPool<TiledGameObject> markPool, bool reverse) {
		TiledGameObject marker = markPool.Get();
		Coord coord = reverse ? from : to;
		Tile tile = pieceMoved.board.GetTile(coord);
		Transform markerTransform = marker.transform;
		markerTransform.SetParent(tile.transform);
		markerTransform.localPosition = Vector3.zero;
		if (marker is TiledWire tw) {
			tw.Destination = reverse ? to : from;
		}
		return marker;
	}
}

public class Capture : Move {
	public Coord captureCoord;
	public Piece pieceCaptured;

	public bool isDefend {
		get {
			if (pieceCaptured == null) { return true; }
			Team myTeam = pieceMoved.team;
			return myTeam.IsAlliedWith(pieceCaptured.team);
		}
	}
	public Capture(Piece pieceMoved, Coord from, Coord to, Piece pieceCaptured, Coord fromCaptured) :
	base(pieceMoved, from, to) {
		this.captureCoord = fromCaptured;
		this.pieceCaptured = pieceCaptured;
	}

	public Capture(Capture other) :
	this(other.pieceMoved, other.from, other.to, other.pieceCaptured, other.captureCoord) { }

	public override void Do() {
		base.Do();
		if (pieceCaptured != null) {
			DoCapture(pieceMoved, pieceCaptured, captureCoord);
		}
	}

	public override void Undo() {
		base.Undo();
		if (pieceCaptured != null) {
			Uncapture(pieceMoved, pieceCaptured, captureCoord);
		}
	}

	private static void DoCapture(Piece attacker, Piece captured, Coord capturedLocation) {
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
			string otherIdentifier = pieceCaptured.code;
			return $"{identifier}{from}x{otherIdentifier}{to}";
		} else {
			return "_" + base.ToString();
		}
	}

	public override bool Equals(object obj) {
		return obj.GetType() == typeof(Capture) && DuckTypeEquals(obj as Capture);
	}
	public virtual bool DuckTypeEquals(Capture c) {
		return base.DuckTypeEquals(c) && c.pieceCaptured == pieceCaptured && c.captureCoord == captureCoord;
	}
	public override int GetHashCode() {
		return base.GetHashCode() ^ captureCoord.GetHashCode() ^ pieceCaptured.GetHashCode();
	}
	public override TiledGameObject MakeMark(MemoryPool<TiledGameObject> markPool, bool reverse) {
		TiledGameObject marker = markPool.Get();
		Coord coord = reverse ? from : captureCoord;
		Tile tile = pieceMoved.board.GetTile(coord);
		Transform markerTransform = marker.transform;
		markerTransform.SetParent(tile.transform);
		markerTransform.localPosition = Vector3.zero;
		if (marker.Label != null && pieceCaptured != null) {
			marker.Label.text = $"capture";//\n[{capturable.code}]";
		}
		if (marker is TiledWire tw) {
			tw.Destination = reverse ? to : from;
		}
		return marker;
	}
}

public class Defend : Capture {
	public Defend(Piece pieceMoved, Coord from, Coord to, Piece pieceCaptured, Coord fromCaptured)
		: base(pieceMoved, from, to, pieceCaptured, fromCaptured) {
	}
	public override TiledGameObject MakeMark(MemoryPool<TiledGameObject> markPool, bool reverse) {
		TiledGameObject tgo = base.MakeMark(markPool, reverse);
		if (tgo.Label != null) {
			tgo.Label.text = "defend";
		}
		return tgo;
	}
	public override bool Equals(object obj) {
		return obj.GetType() == typeof(Defend) && base.DuckTypeEquals(obj as Capture);
	}
	public override int GetHashCode() {
		return base.GetHashCode();
	}
	public override string ToString() {
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
			string otherIdentifier = pieceCaptured.code;
			return $"{identifier}{from}^{otherIdentifier}{to}";
		} else {
			return "_" + base.ToString();
		}
	}
}

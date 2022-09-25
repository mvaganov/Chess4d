using System.Collections.Generic;
using UnityEngine;

public class MoveNode {
	public Move move;
	public int index;
	public int timestamp;
	public List<MoveNode> next;
	public MoveNode prev;
	public string notes;

	public bool IsRoot => prev == null;

	public int BranchIndex => prev == null ? -1 : prev.next.IndexOf(this);

	public MoveNode(int index, Move move, string notes) {
		this.move = move;
		this.notes = notes;
		this.index = index;
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
		return obj is MoveNode mn && mn.index == index && mn.move.Equals(move);
	}
	public override int GetHashCode() {
		return ((move != null) ? move.GetHashCode() : 0) ^ index;
	}

	public MoveNode FindMoveRecursive(Move m) {
		if (move == m) { return this; }
		MoveNode found = null;
		for (int i = 0; i < next.Count; ++i) {
			found = next[i].FindMoveRecursive(m);
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

	public virtual void Do() { pieceMoved?.DoMove(this); }

	public virtual void Undo() { pieceMoved?.UndoMove(this); }

	public override string ToString() {
		string identifier = pieceMoved.code;
		// https://en.wikipedia.org/wiki/Algebraic_notation_(chess)
		// TODO look for other peices of the same type on the same board that are also able to move to the given coord.
		// if there is more than one, prepend the column id of from.
		// if there are multiple in the same column, provide the row id instead
		// if there are multiple in both row and column, provide the entire from coordinate.
		return $"{identifier}{to}";
	}

	public override bool Equals(object obj) {
		return obj is Move m && m.GetType() == GetType() && m.from == from && m.to == to && m.pieceMoved == pieceMoved;
	}
	public override int GetHashCode() {
		return from.GetHashCode() ^ to.GetHashCode() ^ pieceMoved.GetHashCode();
	}
}

public class Capture : Move {
	// TODO rename captureCoord?
	public Coord fromCaptured;
	public Piece pieceCaptured;

	public bool IsDefend {
		get {
			if (pieceCaptured == null) { return true; }
			Team myTeam = pieceMoved.team;
			return myTeam.IsAlliedWith(pieceCaptured.team);
		}
	}
	public Capture(Piece pieceMoved, Coord from, Coord to, Piece pieceCaptured, Coord fromCaptured) :
	base(pieceMoved, from, to) {
		this.fromCaptured = fromCaptured;
		this.pieceCaptured = pieceCaptured;
	}

	public override void Do() {
		base.Do();
		if (pieceCaptured != null) {
			DoCapture(pieceMoved, pieceCaptured, fromCaptured);
		}
	}

	public override void Undo() {
		base.Undo();
		if (pieceCaptured != null) {
			Uncapture(pieceMoved, pieceCaptured, fromCaptured);
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
			return $"{identifier}x{to}";
		} else {
			return "_" + base.ToString();
		}
	}

	public override bool Equals(object obj) {
		return obj is Capture c && base.Equals(c) && c.fromCaptured == fromCaptured && c.pieceCaptured == pieceCaptured;
	}

	public override int GetHashCode() {
		return base.GetHashCode() ^ fromCaptured.GetHashCode() ^ pieceCaptured.GetHashCode();
	}
}
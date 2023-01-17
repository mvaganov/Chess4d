using System.Collections.Generic;
using UnityEngine;

public class MoveNode {
	public IMove move;
	public int turnIndex;
	public int timestamp;
	private List<MoveNode> next;
	public MoveNode prev;
	public BoardState boardState;
	public string Notes {
		get => boardState != null ? boardState.notes : null;
		set => boardState.notes = value;
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

	public MoveNode(int index, IMove move, string notes) {
		this.move = move;
		this.turnIndex = index;
		timestamp = System.Environment.TickCount;
		next = new List<MoveNode>();
		prev = null;
		if (move != null) {
			List<IMove> newMoves = new List<IMove>();
			move.Board.game.moveNodeBeingProcessed = this;
			boardState = move.Board.Analysis.NewAnalysisAfter(move, newMoves);
			newMoves.RemoveAll(move => move.GetType() == typeof(Defend));
			boardState.notableMoves = newMoves;
			Debug.Log($"{move} new moves: {string.Join(", ", newMoves.ConvertAll(m=>m.ToString()))}");
		}
		this.Notes = notes;
		//Debug.Log("MoveNode "+move);
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
		return $"{move}{NotesSuffix()}";
	}

	public override bool Equals(object obj) {
		return obj is MoveNode mn && mn.turnIndex == turnIndex && mn.move.Equals(move);
	}
	public override int GetHashCode() {
		return ((move != null) ? move.GetHashCode() : 0) ^ turnIndex;
	}

	public MoveNode FindMoveRecursive(IMove m, HashSet<MoveNode> ignoreBranches) {
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

/// <summary>
/// TODO implement this.
/// make the current Move a SingleMove class.
/// make a multi-move, which allows board resets.
/// </summary>
public interface IMove {
	public Board Board { get;}
	public Piece Piece { get;}
	public Piece GetPiece(int index);
	public int GetPieceCount();
	public Coord GetRelevantCoordinate();
	public bool Involves(Piece piece);
	public void GetMovingPieces(HashSet<Piece> out_movingPieces);
	public void Do();
	public void Undo();
	public void DoWithoutAnimation();
	public void UndoWithoutAnimation();
}

public class PieceMove : IMove {
	/// <summary>
	/// the board must be known because pieces could conceivably move between boards and do similar moves on different boards
	/// </summary>
	public Board board;
	public Coord from, to;
	public Piece pieceMoved;

	public Board Board => board;
	public Piece Piece => pieceMoved;

	public PieceMove(Board board, Piece pieceMoved, Coord from, Coord to) {
		this.board = board;
		this.from = from;
		this.to = to;
		this.pieceMoved = pieceMoved;
	}

	public PieceMove(PieceMove other) {
		from = other.from;
		to = other.to;
		pieceMoved = other.pieceMoved;
	}
	public Piece GetPiece(int index) => Piece;
	public int GetPieceCount() => 1;
	public virtual Coord GetRelevantCoordinate() => to;

	public virtual bool Involves(Piece piece) => pieceMoved == piece;

	public virtual void GetMovingPieces(HashSet<Piece> out_movingPieces) {
		if (pieceMoved != null) { out_movingPieces.Add(pieceMoved); }
	}

	public void DoWithoutAnimation() {
		if (pieceMoved != null) { pieceMoved.animating = false; }
		Do();
		if (pieceMoved != null) { pieceMoved.animating = true; }
	}

	public void UndoWithoutAnimation() {
		if (pieceMoved != null) { pieceMoved.animating = false; }
		Undo();
		if (pieceMoved != null) { pieceMoved.animating = true; }
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
		return obj.GetType() == typeof(PieceMove) && DuckTypeEquals(obj as PieceMove);
	}
	public virtual bool DuckTypeEquals(PieceMove m) {
		return m.from == from && m.to == to && m.pieceMoved == pieceMoved;
	}
	public override int GetHashCode() {
		return from.GetHashCode() ^ to.GetHashCode() ^ (pieceMoved != null ? pieceMoved.GetHashCode() : 0);
	}

	public virtual TiledGameObject MakeMark(MemoryPool<TiledGameObject> markPool, bool reverse, Color color) {
		TiledGameObject marker = markPool.Get();
		Coord coord = reverse ? from : to;
		Tile tile = pieceMoved.board.GetTile(coord);
		Transform markerTransform = marker.transform;
		markerTransform.SetParent(tile.transform);
		markerTransform.localPosition = Vector3.zero;
		if (marker is TiledWire tw) {
			//tw.Destination = reverse ? to : from;
			tw.DrawLine(reverse ? from : to, reverse ? to : from, color);
		}
		return marker;
	}
}

public class Capture : PieceMove {
	public Coord captureCoord;
	public Piece pieceCaptured;

	public bool isDefend {
		get {
			if (pieceCaptured == null) { return true; }
			Team myTeam = pieceMoved.team;
			return myTeam.IsAlliedWith(pieceCaptured.team);
		}
	}
	public Capture(Board board, Piece pieceMoved, Coord from, Coord to, Piece pieceCaptured, Coord fromCaptured) :
	base(board, pieceMoved, from, to) {
		this.captureCoord = fromCaptured;
		this.pieceCaptured = pieceCaptured;
	}

	public Capture(Capture other) :
	this(other.board, other.pieceMoved, other.from, other.to, other.pieceCaptured, other.captureCoord) { }

	public override Coord GetRelevantCoordinate() => captureCoord;

	public override bool Involves(Piece piece) => pieceCaptured == piece || base.Involves(piece);

	public override void GetMovingPieces(HashSet<Piece> out_movingPieces) {
		base.GetMovingPieces(out_movingPieces);
		if (pieceCaptured != null) { out_movingPieces.Add(pieceCaptured); }
	}

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
		attacker.team.Capture(captured);
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
	public override TiledGameObject MakeMark(MemoryPool<TiledGameObject> markPool, bool reverse, Color color) {
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
			//tw.Destination = reverse ? to : from;
			tw.DrawLine(reverse ? from : to, reverse ? to : from, color);
		}
		return marker;
	}
}

public class Defend : Capture {
	public Defend(Board board, Piece pieceMoved, Coord from, Coord to, Piece pieceCaptured, Coord fromCaptured)
		: base(board, pieceMoved, from, to, pieceCaptured, fromCaptured) {
	}
	public override TiledGameObject MakeMark(MemoryPool<TiledGameObject> markPool, bool reverse, Color color) {
		TiledGameObject tgo = base.MakeMark(markPool, reverse, color);
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

public class StartGame : IMove {
	Board board;
	public Board Board => board;
	public Piece Piece => null;
	public StartGame(Board board) { this.board = board; }
	public Coord GetRelevantCoordinate() => Coord.zero;
	public bool Involves(Piece piece) => false;
	public void GetMovingPieces(HashSet<Piece> out_movingPieces) { }
	public void Do() { }
	public void Undo() { }
	public override string ToString() { return "start game"; }
	public Piece GetPiece(int index) => null; // TODO should get all pieces?
	public int GetPieceCount() => 0; // TODO should be the entire board?
	public void DoWithoutAnimation() { }
	public void UndoWithoutAnimation() { }
}

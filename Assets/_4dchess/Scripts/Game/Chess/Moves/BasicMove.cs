using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMove : IGameMoveBase {
	/// <summary>
	/// the board must be known because pieces could conceivably move between boards and do similar moves on different boards
	/// </summary>
	public Board board;
	public Coord from, to;
	public Piece pieceMoved;

	public Board Board => board;
	public Piece Piece => pieceMoved;
	public virtual bool IsValid => true;
	public BasicMove(Board board, Piece pieceMoved, Coord from, Coord to) {
		this.board = board;
		this.from = from;
		this.to = to;
		this.pieceMoved = pieceMoved;
	}

	public BasicMove(BasicMove other) {
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
		return obj.GetType() == typeof(BasicMove) && DuckTypeEquals(obj as BasicMove);
	}
	public virtual bool DuckTypeEquals(BasicMove m) {
		return m != null && m.from == from && m.to == to && m.pieceMoved == pieceMoved;
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

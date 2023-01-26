using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceMoveAttack : BasicMove, ICapture {
	public Coord captureCoord;
	public Piece pieceCaptured;

	public bool isDefend {
		get {
			if (pieceCaptured == null) { return true; }
			Team myTeam = pieceMoved.team;
			return myTeam.IsAlliedWith(pieceCaptured.team);
		}
	}
	public PieceMoveAttack(Board board, Piece pieceMoved, Coord from, Coord to, Piece pieceCaptured, Coord fromCaptured) :
	base(board, pieceMoved, from, to) {
		this.captureCoord = fromCaptured;
		this.pieceCaptured = pieceCaptured;
	}

	public PieceMoveAttack(PieceMoveAttack other) :
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
		return obj.GetType() == typeof(PieceMoveAttack) && DuckTypeEquals(obj as PieceMoveAttack);
	}
	public virtual bool DuckTypeEquals(PieceMoveAttack c) {
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

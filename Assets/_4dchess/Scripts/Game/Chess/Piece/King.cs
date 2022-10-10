using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : MoveLogic {
	// TODO king-side and queen-side castle, optionally don't even give move-into-check as an option
	public static readonly Coord[] movePattern = new Coord[] {
			new Coord(+1, 0),
			new Coord(-1, 0),
			new Coord( 0,+1),
			new Coord( 0,-1),
			new Coord(+1,+1),
			new Coord(-1,+1),
			new Coord(+1,-1),
			new Coord(-1,-1),
	};

	public override void GetMoves(List<Move> out_moves, MoveKind moveKind) {
		StandardMoves(movePattern, 1, out_moves, moveKind);
		if (piece.moveCount == 0) {
			List<Move> castleMoves = Castle.FindMoves(this, Rook.movePattern, "R");
			out_moves.AddRange(castleMoves);
		}
	}

	public class Check : Capture {
		public Move triggeringMove;

		public Check(Piece pieceMoved, Coord from, Coord to, Piece king)
			: base(pieceMoved, from, to, king, king.GetCoord()) {
		}

		public override void Do() { base.Do(); triggeringMove.Do(); }
		public override void Undo() { triggeringMove.Undo(); base.Undo(); }
	}

	public class Castle : Move {
		public Move partnerMove;

		public Castle(Piece pieceMoved, Coord from, Coord to, Piece partner, Coord partnerFrom, Coord partnerTo)
		: base(pieceMoved, from, to) {
			partnerMove = new Move(partner, partnerFrom, partnerTo);
		}

		public static List<Move> FindMoves(MoveLogic king, Coord[] movePattern, string pieceCode) {
			List<Move> moves = new List<Move>();
			// look for rooks that have a line to the king using rook movement
			king.StandardMoves(movePattern, 8, moves, MoveKind.Defend);
			//Debug.Log("looking for castles " + string.Join(", ", moves));
			if (moves.Count > 0) {
				Piece self = king.piece;
				Coord here = self.GetCoord();
				// get the ones sort of far away that haven't moved
				for (int i = moves.Count - 1; i >= 0; --i) {
					Defend def = moves[i] as Defend;
					if (def == null || def.pieceCaptured == null || def.pieceCaptured.moveCount != 0
					|| def.pieceCaptured.code != pieceCode || (def.captureCoord - here).MagnitudeManhattan < 2) {
						moves.RemoveAt(i);
					} else {
						Debug.Log("found a castle? " + def.pieceCaptured);
					}
				}
				// mark the castle, which is the rook moving next to the king, and the king jumping over the rook
				for (int i = 0; i < moves.Count; ++i) {
					// replace the move with a castle to that target.
					Capture cap = moves[i] as Capture;
					Piece other = cap.pieceCaptured;
					Coord there = other.GetCoord();
					Coord delta = there - here;
					Coord normal = delta.normalized;
					Castle castle = new Castle(king.piece, here, here + normal * 2, other, other.GetCoord(), here + normal);
					moves[i] = castle;
				}
			}
			return moves;
		}

		public override void Do() { base.Do(); partnerMove.Do(); }

		public override void Undo() { partnerMove.Undo(); base.Undo(); }

		public override string ToString() { return base.ToString() + partnerMove.ToString(); }

		public override bool Equals(object obj) {
			return base.Equals(obj) && ((Castle)obj).partnerMove.Equals(partnerMove);
		}

		public override int GetHashCode() {
			return from.GetHashCode() ^ to.GetHashCode() ^ pieceMoved.GetHashCode() ^ partnerMove.GetHashCode();
		}

		public override TiledGameObject MakeMark(MemoryPool<TiledGameObject> markPool, bool reverse) {
			TiledGameObject marker = markPool.Get();
			Coord coord = reverse ? from : to;
			Board board = pieceMoved.board;
			Tile tile = board.GetTile(coord);
			Transform markerTransform = marker.transform;
			markerTransform.SetParent(tile.transform);
			markerTransform.localPosition = Vector3.zero;
			if (marker.Label != null) {
				Piece partner = partnerMove.pieceMoved;
				if (partner != null) {
					marker.Label.text = $"castle";
				} else {
					marker.Label.text = "IMposSI 'BLE";
				}
			}
			if (marker is TiledWire tw) {
				tw.DrawLine(partnerMove.from, partnerMove.to);
			}
			return marker;
		}
	}
}

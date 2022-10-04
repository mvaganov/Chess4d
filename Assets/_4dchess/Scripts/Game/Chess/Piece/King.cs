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
			List<Move> castleMoves = GetCastleMoves();
			out_moves.AddRange(castleMoves);
		}
	}

	private List<Move> GetCastleMoves() {
		List<Move> moves = new List<Move>();
		// look for rooks that have a line to the king using rook movement
		StandardMoves(Rook.movePattern, 8, moves, MoveKind.Defend);
		if (moves.Count > 0) {
			Piece self = piece;
			Coord here = self.GetCoord();
			// get the ones sort of far away that haven't moved
			for (int i = moves.Count - 1; i >= 0; --i) {
				Move move = moves[i];
				if (move.pieceMoved.moveCount != 0 || (move.from - here).MagnitudeManhattan < 2) {

				}
			}
			// mark the castle, which is the rook moving next to the king, and the king jumping over the rook
			for(int i = 0; i < moves.Count; ++i) {
				// replace the move with a castle to that target.
			}
			moves.Clear();// TODO remove this once the castle move is implemented
		}

		return moves;
	}
}

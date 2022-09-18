using System.Collections.Generic;

public class Pawn : MoveLogic {
	// TODO en passant, pawn promotion
	public override void GetMoves(List<Coord> out_moves, List<Coord> out_captures, List<Coord> out_defends) {
		Coord dir = team.pawnDirection;
		Moves(new Coord[] { dir }, piece.moveCount > 0 ? 1 : 2, out_moves, null, null);
		Moves(new Coord[] { dir + new Coord(-1, 0), dir + new Coord(1, 0) }, 1, null, out_captures, out_defends);
	}
}

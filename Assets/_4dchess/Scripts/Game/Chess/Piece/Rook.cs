using System.Collections.Generic;

public class Rook : MoveLogic {
	private static readonly Coord[] movePattern = new Coord[] {
		new Coord(+1, 0),
		new Coord(-1, 0),
		new Coord( 0,+1),
		new Coord( 0,-1),
	};
	public override void GetMoves(List<Move> out_moves, MoveKind moveKind) {
		StandardMoves(movePattern, 8, out_moves, moveKind);
	}
}

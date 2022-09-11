using System.Collections.Generic;

public class Rook : MoveLogic {
	public override List<Coord> GetMoves() {
		return Moves(new Coord[] {
			new Coord(+1, 0),
			new Coord(-1, 0),
			new Coord( 0,+1),
			new Coord( 0,-1),
		}, 8, MoveCalculation.MovesAndCaptures);
	}
}

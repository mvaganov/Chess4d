using System.Collections.Generic;

public class Queen : MoveLogic {
	public override List<Coord> GetMoves(MoveCalculation moveType) {
		return Moves(new Coord[] {
			new Coord(+1, 0),
			new Coord(-1, 0),
			new Coord( 0,+1),
			new Coord( 0,-1),
			new Coord(+1,+1),
			new Coord(-1,+1),
			new Coord(+1,-1),
			new Coord(-1,-1),   
		}, 8, moveType);
	}
}

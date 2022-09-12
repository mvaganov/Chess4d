using System.Collections.Generic;

public class Knight : MoveLogic {
	public override List<Coord> GetMoves(MoveCalculation moveType) {
		return Moves(new Coord[] {
			new Coord(+1,+2),
			new Coord(+2,+1),
			new Coord(-1,+2),
			new Coord(-2,+1),
			new Coord(+1,-2),
			new Coord(+2,-1),
			new Coord(-1,-2),
			new Coord(-2,-1),
		}, 1, moveType);
	}
}

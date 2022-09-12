using System.Collections.Generic;

public class Bishop : MoveLogic {
	public override List<Coord> GetMoves(MoveCalculation moveType) {
		return Moves(new Coord[] {
			new Coord(+1,+1),
			new Coord(-1,+1),
			new Coord(+1,-1),
			new Coord(-1,-1),
		}, 8, moveType);
	}
}

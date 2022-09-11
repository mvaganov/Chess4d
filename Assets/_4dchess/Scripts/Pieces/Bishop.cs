using System.Collections.Generic;

public class Bishop : MoveLogic {
	public override List<Coord> GetMoves() {
		return Moves(new Coord[] {
			new Coord(+1,+1),
			new Coord(-1,+1),
			new Coord(+1,-1),
			new Coord(-1,-1),
		}, 8, MoveCalculation.MovesAndCaptures);
	}
}

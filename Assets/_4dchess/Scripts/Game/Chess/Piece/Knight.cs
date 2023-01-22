using System.Collections.Generic;

public class Knight : MoveLogic {
	public static readonly Coord[] movePattern = new Coord[] {
		new Coord(+1,+2),
		new Coord(+2,+1),
		new Coord(-1,+2),
		new Coord(-2,+1),
		new Coord(+1,-2),
		new Coord(+2,-1),
		new Coord(-1,-2),
		new Coord(-2,-1),
	};
	public override void GetMoves(List<IGameMoveBase> out_moves, MoveKind moveKind) {
		StandardMoves(movePattern, 1, out_moves, moveKind);
	}
}

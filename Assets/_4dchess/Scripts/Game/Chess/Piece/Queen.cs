using System.Collections.Generic;

public class Queen : MoveLogic {
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
	public override void GetMoves(GameState state, List<IGameMoveBase> out_moves, MoveKind moveKind) {
		StandardMoves(state, movePattern, 8, out_moves, moveKind);
	}
}

using System.Collections.Generic;

public class Bishop : MoveLogic {
	private static readonly Coord[] movePattern = new Coord[] {
			new Coord(+1,+1),
			new Coord(-1,+1),
			new Coord(+1,-1),
			new Coord(-1,-1),
	};
	public override void GetMoves(GameState state, List<IGameMoveBase> out_moves, MoveKind moveKind) {
		StandardMoves(state, movePattern, 8, out_moves, moveKind);
	}
}

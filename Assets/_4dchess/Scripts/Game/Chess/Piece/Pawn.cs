using System.Collections.Generic;

public class Pawn : MoveLogic {
	public bool hasMoved = false;
	public override void DoMove(Coord coord) {
		base.DoMove(coord);
		hasMoved = true;
	}
	// TODO en passant, piece upgrade
	public override List<Coord> GetMoves(MoveCalculation moveType) {
		List<Coord> moves = new List<Coord>();
		Coord dir = team.pawnDirection;
		if (moveType.HasFlag(MoveCalculation.Moves)) {
			List<Coord> forwardMove = Moves(new Coord[] { dir }, 1, moveType);
			if (forwardMove.Count > 0) {
				moves.AddRange(forwardMove);
				if (!hasMoved) {
					moves.AddRange(Moves(new Coord[] { dir * 2 }, 1, moveType));
				}
			}
		}
		if (moveType.HasFlag(MoveCalculation.Captures) || moveType.HasFlag(MoveCalculation.Defense)) {
			moves.AddRange(Moves(new Coord[] { dir + new Coord(-1, 0), dir + new Coord(1, 0) }, 1, moveType));
		}
		return moves;
	}
}

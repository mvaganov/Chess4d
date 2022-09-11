using System.Collections.Generic;

public class Pawn : MoveLogic {
	public bool hasMoved = false;
	public void DoMove(Coord coord) {
		piece.MoveTo(coord);
		hasMoved = true;
	}
	public override IEnumerable<Coord> GetMoves() {
		List<Coord> moves = new List<Coord>();
		Coord dir = team.pawnDirection;
		List<Coord> forwardMove = Moves(new Coord[] { dir }, 1, MoveCalculation.OnlyMoves);
		if (forwardMove.Count > 0) {
			moves.AddRange(forwardMove);
			if (!hasMoved) {
				moves.AddRange(Moves(new Coord[] { dir * 2 }, 1, MoveCalculation.OnlyMoves));
			}
		}
		moves.AddRange(Moves(new Coord[] { dir + new Coord(0, -1) }, 1, MoveCalculation.OnlyCaptures));
		moves.AddRange(Moves(new Coord[] { dir + new Coord(0, 1) }, 1, MoveCalculation.OnlyCaptures));
		return moves;
	}
}

using System.Collections.Generic;

public class Pawn : MoveLogic {
	// TODO mark a flag to identify the turn that this pawn did a double move, required for en passant logic
	// TODO en passant, pawn promotion
	public override void GetMoves(List<Coord> out_moves, List<Coord> out_captures, List<Coord> out_defends) {
		Coord dir = team.pawnDirection;
		Moves(new Coord[] { dir }, piece.moveCount > 0 ? 1 : 2, out_moves, null, null);
		Moves(new Coord[] { dir + Coord.left, dir + Coord.right }, 1, null, out_captures, out_defends);
		Piece p = piece;
		// TODO en passant
		Coord coord = p.GetCoord();
		Board board = p.board;
		Move leftEP = HasPossibleEnPassant(board, p, coord + Coord.left);
		Move rightEP = HasPossibleEnPassant(board, p, coord + Coord.left);
		// TODO add leftEP and rightEP to out_moves if they exist
		// TODO pawn promotion
	}
	private Move HasPossibleEnPassant(Board board, Piece p, Coord coord) {
		Tile leftTile = board.GetTile(coord);
		if (leftTile == null) { return null; }
		Piece possibleTarget = leftTile.GetPiece();
		if (possibleTarget == null || possibleTarget.code != p.code || possibleTarget.moveCount != 1) {
			return null;
		}
		UnityEngine.Debug.Log("TODO posible en passant on " + leftTile.GetCoord() + " if the pawn just double moved");
		// TODO create en passant move
		return null;
	}
}

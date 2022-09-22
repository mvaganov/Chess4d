using System.Collections.Generic;

public class Pawn : MoveLogic {
	public class DoublePawnMove : Move {
		public DoublePawnMove(Piece pieceMoved, Coord from, Coord to) : base(pieceMoved, from, to) { }
		public override void Do() {
			Pawn p = pieceMoved.GetComponent<Pawn>();
			int doubleMoveTurn = pieceMoved.board.game.chessMoves.CurrentMove.index + 1;
			UnityEngine.Debug.Log($"double moving on turn {doubleMoveTurn}");
			p.didDoubleMoveOnTurn = doubleMoveTurn;
			pieceMoved?.DoMove(this);
		}
		public override void Undo() {
			Pawn p = pieceMoved.GetComponent<Pawn>();
			p.didDoubleMoveOnTurn = -1;
			pieceMoved?.UndoMove(this);
		}
	}
	public class EnPassant : Capture {
		public EnPassant(Piece pieceMoved, Coord from, Coord to, Piece pieceCaptured, Coord fromCaptured)
		: base(pieceMoved, from, to, pieceCaptured, fromCaptured) { }
	}
	private int didDoubleMoveOnTurn = -1;
	// TODO mark a flag to identify the turn that this pawn did a double move, required for en passant logic
	// TODO en passant, pawn promotion
	public override void GetMoves(List<Move> out_moves, MoveKind moveKind) {
		Piece p = piece;
		Coord coord = p.GetCoord();
		Board board = p.board;
		Coord dir = team.pawnDirection;
		if (moveKind.HasFlag(MoveKind.Move)) {
			StandardMoves(new Coord[] { dir }, 1, out_moves, MoveKind.Move);
			if (piece.moveCount == 0) {
				Coord rushedMove = coord + dir * 2;
				Tile tile = board.GetTile(rushedMove);
				if (tile != null && tile.GetPiece() == null) {
					out_moves.Add(new DoublePawnMove(p, coord, rushedMove));
				}
			}
		}
		if (moveKind.HasFlag(MoveKind.Defend) || moveKind.HasFlag(MoveKind.Attack)) {
			StandardMoves(new Coord[] { dir + Coord.left, dir + Coord.right }, 1, out_moves, MoveKind.AttackDefend);
		}
		// TODO en passant
		Move leftEP = HasPossibleEnPassant(board, p, coord, coord + dir + Coord.left, coord + Coord.left);
		Move rightEP = HasPossibleEnPassant(board, p, coord, coord + dir + Coord.right, coord + Coord.right);
		// TODO add leftEP and rightEP to out_moves if they exist
		// TODO pawn promotion
	}
	private Move HasPossibleEnPassant(Board board, Piece p, Coord thisPieceLocation, Coord nextPieceLocation, Coord otherPieceLocation) {
		Tile leftTile = board.GetTile(otherPieceLocation);
		if (leftTile == null) { return null; }
		Piece possibleTarget = leftTile.GetPiece();
		if (possibleTarget == null || possibleTarget.code != p.code) {
			return null;
		}
		//UnityEngine.Debug.Log("maybe en passant?");
		Pawn pawn = possibleTarget.GetComponent<Pawn>();
		if (possibleTarget.moveCount != 1 || pawn.didDoubleMoveOnTurn == board.game.chessMoves.CurrentMove.index) {
			return null;
		}
		UnityEngine.Debug.Log("TODO posible en passant on " + leftTile.GetCoord() + " if the pawn just double moved");
		// TODO create en passant move
		return new EnPassant(p, thisPieceLocation, nextPieceLocation, possibleTarget, otherPieceLocation);
	}
}

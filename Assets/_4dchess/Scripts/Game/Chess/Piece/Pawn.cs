using System.Collections.Generic;
using UnityEngine;
public partial class Pawn : MoveLogic {
	const int InvalidTurn = -1;
	private int didDoubleMoveOnTurn = InvalidTurn;
	private Coord direction;

	private void Start() {
		direction = team.pawnDirection;
	}

	internal static Pawn GetPawn(Piece piece, string forWhat) {
		Pawn p = piece.GetComponent<Pawn>();
		if (p == null) { throw new System.Exception($"only {nameof(Pawn)} should have access to {forWhat}"); }
		return p;
	}

	public override void GetMoves(List<Move> out_moves, MoveKind moveKind) {
		Piece p = piece;
		Coord coord = p.GetCoord();
		Board board = p.board;
		List<Move> pawnMoves = new List<Move>();
		if (moveKind.HasFlag(MoveKind.Move)) {
			StandardMoves(new Coord[] { direction }, 1, pawnMoves, MoveKind.Move);
			if (piece.moveCount == 0 && pawnMoves.Count > 0) {
				Coord rushedMove = coord + direction * 2;
				Tile tile = board.GetTile(rushedMove);
				if (tile != null && tile.GetPiece() == null) {
					pawnMoves.Add(new DoubleMove(p.board, p, coord, rushedMove));
				}
			}
		}
		if (moveKind.HasFlag(MoveKind.Defend) || moveKind.HasFlag(MoveKind.Attack)) {
			StandardMoves(new Coord[] { direction + Coord.left, direction + Coord.right }, 1, pawnMoves, MoveKind.AttackDefend);
		}
		Move leftEP = EnPassant.GetPossible(board, p, coord, coord + direction + Coord.left, coord + Coord.left);
		if (leftEP != null) { pawnMoves.Add(leftEP); }
		Move rightEP = EnPassant.GetPossible(board, p, coord, coord + direction + Coord.right, coord + Coord.right);
		if (rightEP != null) { pawnMoves.Add(rightEP); }
		ReplaceAnyMoveOntoFinalSquareWithPawnPromotion(pawnMoves, board);
		out_moves.AddRange(pawnMoves);
	}

	private void ReplaceAnyMoveOntoFinalSquareWithPawnPromotion(List<Move> out_pawnMoves, Board board) {
		for (int i = 0; i < out_pawnMoves.Count; ++i) {
			Move m = out_pawnMoves[i];
			if (IsLastRow(board, m.to) && !(m is Defend) && !(m is Promotion)) {
				out_pawnMoves[i] = new Promotion(out_pawnMoves[i]);
			}
		}
	}

	public bool IsLastRow(Board board, Coord coord) {
		bool isValidCoord = board.GetTile(coord) != null;
		bool nextWouldBeValid = board.GetTile(coord + direction) != null;
		return isValidCoord && !nextWouldBeValid;
	}

	public override void DoMove(Move move) {
		base.DoMove(move);
	}

	public override void UndoMove(Move move) {
		base.UndoMove(move);
	}
}

using System.Collections.Generic;
using UnityEngine;
public partial class Pawn : PieceLogic {
	const int InvalidTurn = -1;
	private int didDoubleMoveOnTurn = InvalidTurn;
	/// <summary>
	/// should never change. if any effect causes it to change, it should be added to PawnState
	/// </summary>
	private Coord direction;
	class PawnState : State {
		public int didDoubleMoveOnTurn = InvalidTurn;
		public PawnState(Coord coord) : base(coord, 0, true) { }
		public PawnState(Coord coord, short moveCount, int doubleMoveOnTurn) : base (coord, moveCount, true) {
			didDoubleMoveOnTurn = doubleMoveOnTurn;
		}
		public override State MovedTo(Coord coord, short newMoveCount = -1) =>
			new PawnState(coord, (short)(newMoveCount < 0 ? moveCount + 1 : newMoveCount), didDoubleMoveOnTurn);
		public override bool Equals(object obj) => obj is PawnState ps && base.Equals(ps)
			&& didDoubleMoveOnTurn == ps.didDoubleMoveOnTurn;
		public override int GetHashCode() => base.GetHashCode() ^ didDoubleMoveOnTurn;
	}
	public void SetState(State state) => base.SetState<PawnState>(state);

	public override void Initialize() {
		direction = team.pawnDirection;
	}

	internal static Pawn GetPawn(Piece piece, string forWhat) {
		Pawn p = piece.GetComponent<Pawn>();
		if (p == null) { throw new System.Exception($"only {nameof(Pawn)} should have access to {forWhat}"); }
		return p;
	}

	public override void GetMoves(GameState state, List<IGameMoveBase> out_moves, MoveKind moveKind) {
		Piece p = piece;
		Coord coord = p.GetCoord();
		Board board = p.board;
		List<IGameMoveBase> pawnMoves = new List<IGameMoveBase>();
		if (moveKind.HasFlag(MoveKind.Move)) {
			//StandardMoves(new Coord[] { direction }, 1, pawnMoves, MoveKind.Move);
			//if(board.GetPiece(coord + direction) == null) {
			if (state.GetPieceAt(coord + direction) == null) {
				pawnMoves.Add(new PieceMove(board, p, coord, coord+direction));
			}
			if (piece.moveCount == 0 && pawnMoves.Count > 0) {
				Coord rushedMove = coord + direction * 2;
				Tile tile = board.GetTile(rushedMove);
				if (tile != null && tile.GetPiece() == null) {
					pawnMoves.Add(new DoubleMove(p.board, p, coord, rushedMove));
				}
			}
		}
		if (moveKind.HasFlag(MoveKind.Defend) || moveKind.HasFlag(MoveKind.Attack)) {
			StandardMoves(state, new Coord[] { direction + Coord.left, direction + Coord.right }, 1, pawnMoves, MoveKind.AttackDefend);
		}
		BasicMove leftEP = EnPassant.GetPossible(board, p, coord, coord + direction + Coord.left, coord + Coord.left);
		if (leftEP != null) { pawnMoves.Add(leftEP); }
		BasicMove rightEP = EnPassant.GetPossible(board, p, coord, coord + direction + Coord.right, coord + Coord.right);
		if (rightEP != null) { pawnMoves.Add(rightEP); }
		ReplaceAnyMoveOntoFinalSquareWithPawnPromotion(pawnMoves, board);
		out_moves.AddRange(pawnMoves);
	}

	private void ReplaceAnyMoveOntoFinalSquareWithPawnPromotion(List<IGameMoveBase> out_pawnMoves, Board board) {
		for (int i = 0; i < out_pawnMoves.Count; ++i) {
			IGameMoveBase im = out_pawnMoves[i];
			BasicMove m = im as BasicMove;
			if (IsLastRow(board, m.to) && m.IsValid && !(m is Promotion)) {
				out_pawnMoves[i] = new Promotion(out_pawnMoves[i]);
			}
		}
	}

	public bool IsLastRow(Board board, Coord coord) {
		bool isValidCoord = board.GetTile(coord) != null;
		bool nextWouldBeValid = board.GetTile(coord + direction) != null;
		return isValidCoord && !nextWouldBeValid;
	}

	public override void DoMove(BasicMove move) {
		base.DoMove(move);
	}

	public override void UndoMove(BasicMove move) {
		base.UndoMove(move);
	}
}

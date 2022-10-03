using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Pawn
{
	public class DoubleMove : Move {
		public DoubleMove(Piece pieceMoved, Coord from, Coord to) : base(pieceMoved, from, to) { }
		public override void Do() {
			MoveNode thisMove = pieceMoved.Game.chessMoves.FindMoveNode(this);
			GetPawn(pieceMoved, nameof(DoubleMove)).didDoubleMoveOnTurn = thisMove.turnIndex;
			pieceMoved?.DoMove(this);
		}
		public override void Undo() {
			GetPawn(pieceMoved, nameof(DoubleMove)).didDoubleMoveOnTurn = InvalidTurn;
			pieceMoved?.UndoMove(this);
		}
	}

	public class EnPassant : Capture {
		public EnPassant(Piece pieceMoved, Coord from, Coord to, Piece pieceCaptured, Coord fromCaptured)
		: base(pieceMoved, from, to, pieceCaptured, fromCaptured) { }
		public override void Do() {
			GetPawn(pieceMoved, nameof(EnPassant));
			base.Do();
		}
		public override void Undo() {
			GetPawn(pieceMoved, nameof(EnPassant));
			base.Undo();
		}
		public static Move GetPossible(Board board, Piece p, Coord pieceLocation, Coord nextPieceLocation, Coord otherPieceLocation) {
			Tile sideTile = board.GetTile(otherPieceLocation);
			if (sideTile == null) { return null; }
			Piece possibleTarget = sideTile.GetPiece();
			if (possibleTarget == null || possibleTarget.code != p.code) {
				return null;
			}
			//UnityEngine.Debug.Log("maybe en passant?");
			Pawn pawn;
			if (possibleTarget.moveCount != 1 || (pawn = possibleTarget.GetComponent<Pawn>()) == null) {
				return null;
			}
			bool onlyJustNowDidDoubleMove = pawn.didDoubleMoveOnTurn == board.game.chessMoves.CurrentMove.turnIndex;
			UnityEngine.Debug.Log("en passant " + sideTile.GetCoord() + " just double moved? " + onlyJustNowDidDoubleMove +
				"  " + pawn.didDoubleMoveOnTurn + " " + board.game.chessMoves.CurrentMove.turnIndex);
			if (!onlyJustNowDidDoubleMove) return null;
			// TODO create en passant move
			return new EnPassant(p, pieceLocation, nextPieceLocation, possibleTarget, otherPieceLocation);
		}
	}

	public class Promotion : Move {
		private string selectedPieceCode = null;
		public Piece promotedPiece;
		public Move moreInterestingMove;
		const int InvalidUserSelection = -1;
		public int userSelection = InvalidUserSelection;

		public Promotion(Move move) : base(move) {
			if (move.GetType() != typeof(Move)) {
				moreInterestingMove = move;
			}
		}

		public override void Do() {
			MakeBasicMove();
			PromotePieceLogic();
		}

		private void MakeBasicMove() {
			if (moreInterestingMove != null) {
				moreInterestingMove.Do();
			} else {
				base.Do();
			}
		}

		private void PromotePieceLogic() {
			PieceSelection selectionUi = FindObjectOfType<PieceSelection>(true);
			string options = pieceMoved.board.game.PawnPromotionOptions;
			if (selectedPieceCode != null) {
				userSelection = options.IndexOf(selectedPieceCode);
			} else if (promotedPiece != null) {
				userSelection = options.IndexOf(promotedPiece.code);
			}
			int selectedOption = userSelection;
			if (selectedOption == InvalidUserSelection) { selectedOption = options.IndexOf("Q"); }
			if (promotedPiece != null && selectedPieceCode == promotedPiece.code) {
				//UnityEngine.Debug.Log("aleady did this before. " + selectedPieceCode + " " + options[selected]);
				DoReplacement(selectedPieceCode);
			} else {
				selectionUi.SelectPiece(options, pieceMoved.team, selectedOption, DoReplacement);
			}
		}

		public override void Undo() {
			//UnityEngine.Debug.Log("UNdid pawn promotion to " + selectedPieceCode);
			UndoReplacement();
			if (moreInterestingMove != null) {
				moreInterestingMove.Undo();
			} else {
				base.Undo();
			}
		}

		private void DoReplacement(int index) {
			if (userSelection == InvalidUserSelection) {
				userSelection = index;
			}
			string options = pieceMoved.board.game.PawnPromotionOptions;
			DoReplacement(options[index].ToString());
		}

		public void DoReplacement(string code) {
			//UnityEngine.Debug.Log("replacing " + pieceMoved.code + " with " + code);
			bool promotionHasHappenedBefore = promotedPiece != null && promotedPiece.code != code;
			if (!promotionHasHappenedBefore) {
				PromotePiece(code);
				return;
			}
			if (RedoPreviouslyDonePromotion(code)) {
				return;
			}
			BranchIntoDifferentPromotion(code);
		}

		private bool RedoPreviouslyDonePromotion(string code) {
			Board board = pieceMoved.board;
			ChessGame game = board.game;
			MoveNode thisNode = game.chessMoves.CurrentMove;//game.chessMoves.FindMoveNode(this);
			MoveNode parentMove = thisNode.prev;
			for (int i = 0; i < parentMove.next.Count; ++i) {
				MoveNode possibleMove = parentMove.next[i];
				if (possibleMove == thisNode) { continue; }
				Promotion promo = possibleMove.move as Promotion;
				if (promo == null) { continue; }
				if (promo.promotedPiece.code == code) {
					// set the move that we have done before as the next move, and then do it.
					//UnityEngine.Debug.Log("did " + code + " before, doing it again");
					promo.selectedPieceCode = promo.promotedPiece.code; // identify that we don't need to trigger UI again.
					game.chessMoves.SetCurrentMove(parentMove);
					parentMove.next.RemoveAt(i);
					parentMove.next.Insert(0, possibleMove);
					game.chessMoves.RedoMove(0);
					return true;
				}
			}
			return false;
		}

		private void BranchIntoDifferentPromotion(string code) {
			Board board = pieceMoved.board;
			Team team = pieceMoved.team;
			ChessGame game = board.game;
			MoveNode thisNode = game.chessMoves.CurrentMove;//game.chessMoves.FindMoveNode(this);
			MoveNode parentMove = thisNode.prev;
			Promotion otherPromo = new Promotion(moreInterestingMove != null ? moreInterestingMove : new Move(this));
			otherPromo.selectedPieceCode = code; // <-- this will force the new promotion event to skip the UI
			string options = pieceMoved.board.game.PawnPromotionOptions;
			//otherPromo.userSelection = options.IndexOf(code);
			otherPromo.promotedPiece = game.CreatePiece(team, code, to, board);
			game.chessMoves.SetCurrentMove(parentMove);
			MoveNode alternatePromotion = new MoveNode(thisNode.turnIndex, otherPromo, "");
			alternatePromotion.prev = game.chessMoves.CurrentMove;
			game.chessMoves.CurrentMove.next.Insert(0, alternatePromotion);
			game.chessMoves.RedoMove(0);
		}

		private void PromotePiece(string code) {
			selectedPieceCode = code;
			Board board = pieceMoved.board;
			ChessGame game = board.game;
			Team team = pieceMoved.team;
			if (promotedPiece == null) {
				//UnityEngine.Debug.Log("new promotion? " + code);
				promotedPiece = game.CreatePiece(team, code, to, board);
			} else {
				//UnityEngine.Debug.Log("reactivating old promotion " + code);
				promotedPiece.gameObject.SetActive(true);
				game.SetPiece(promotedPiece, board, to);
			}
			//UnityEngine.Debug.Log("did the old switcharoo");
			pieceMoved.transform.SetParent(null, false);
			pieceMoved.gameObject.SetActive(false);
			int index = team.Pieces.IndexOf(pieceMoved);
			team.Pieces[index] = promotedPiece;
			pieceMoved.board.RecalculatePieceMoves();
		}

		public void UndoReplacement() {
			Team team = pieceMoved.team;
			Board board = pieceMoved.board;
			Tile tile = board.GetTile(promotedPiece.GetCoord());
			promotedPiece.transform.SetParent(null);
			pieceMoved.transform.SetParent(tile.transform, false);
			pieceMoved.gameObject.SetActive(true);
			int index = team.Pieces.IndexOf(promotedPiece);
			if (index < 0) {
				index = team.Pieces.IndexOf(pieceMoved);
			}
			team.Pieces[index] = pieceMoved;
			promotedPiece.gameObject.SetActive(false);
			pieceMoved.board.RecalculatePieceMoves();
			selectedPieceCode = null; // marker that will force selection again
		}

		public override string ToString() {
			string str = moreInterestingMove != null ? moreInterestingMove.ToString() : base.ToString();
			return str + "=" + (promotedPiece != null ? promotedPiece.code : "?");
		}
	}
}

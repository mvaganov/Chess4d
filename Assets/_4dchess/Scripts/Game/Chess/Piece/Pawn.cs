using System.Collections.Generic;

public class Pawn : MoveLogic {
	private int didDoubleMoveOnTurn = -1;
	public class DoubleMove : Move {
		public DoubleMove(Piece pieceMoved, Coord from, Coord to) : base(pieceMoved, from, to) { }
		public override void Do() {
			Pawn p = pieceMoved.GetComponent<Pawn>();
			int indexOfThisMove = -1;
			MoveNode mn = pieceMoved.board.game.chessMoves.FindMoveNode(this);
			if (mn != null) { indexOfThisMove = mn.index; }
			p.didDoubleMoveOnTurn = indexOfThisMove;
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
		public override void Do() {
			base.Do();
			UnityEngine.Debug.Log("DID en passant");
		}
		public override void Undo() {
			base.Undo();
			UnityEngine.Debug.Log("UNdid en passant");
		}
	}

	public class Promotion : Move {
		private string selectedPieceCode = null;
		public Piece promotedPiece;
		public Move moreInterestingMove;
		public int selected = -1;

		public Promotion(Move move) : base(move) {
			if (move.GetType() != typeof(Move)) {
				moreInterestingMove = move;
			}
		}

		public override void Do() {
			//base.Do();
			if (moreInterestingMove != null) {
				moreInterestingMove.Do();
			} else {
				base.Do();
			}
			UnityEngine.Debug.Log("DID pawn promotion to \'" + selectedPieceCode + "\'");
			// open up the piece selection UI
			PieceSelection selectionUi = FindObjectOfType<PieceSelection>(true);
			string options = pieceMoved.board.game.PawnPromotionOptions;
			if (selected == -1) { selected = options.IndexOf("Q"); }
			if (promotedPiece != null && selectedPieceCode == promotedPiece.code) {
				UnityEngine.Debug.Log("aleady did this before.");
				DoReplacement(selected);
			} else {
				selectionUi.SelectPiece(options, pieceMoved.team, selected, DoReplacement);
			}
		}

		public override void Undo() {
			//base.Undo();
			UnityEngine.Debug.Log("UNdid pawn promotion to " + selectedPieceCode);
			UndoReplacement();
			if (moreInterestingMove != null) {
				moreInterestingMove.Undo();
			} else {
				base.Undo();
			}
		}

		private void DoReplacement(int index) {
			selected = index;
			string options = pieceMoved.board.game.PawnPromotionOptions;
			DoReplacement(options[index].ToString());
		}

		public void DoReplacement(string code) {
			UnityEngine.Debug.Log("replacing " + pieceMoved.code + " with " + code);
			Board board = pieceMoved.board;
			ChessGame game = board.game;
			Team team = pieceMoved.team;
			if (promotedPiece != null && promotedPiece.code != code) {
				UnityEngine.Debug.Log("redoing? "+code+" vs "+promotedPiece.code);
				MoveNode thisNode = game.chessMoves.FindMoveNode(this);
				MoveNode parentMove = thisNode.prev;
				for(int i = 0; i < parentMove.next.Count; ++i) {
					MoveNode possibleMove = parentMove.next[i];
					if (possibleMove == thisNode) { continue; }
					Promotion promo = possibleMove.move as Promotion;
					if (promo == null) { continue; }
					if (promo.promotedPiece.code == code) {
						UnityEngine.Debug.Log("did "+code+" before");
						game.chessMoves.GoToMove(possibleMove);
						return;
					}
				}
				UnityEngine.Debug.Log("never did " + code + " before, making branch");
				// TODO create a new MoveNode at the same branch (coming from the same source move) and finish there
				Promotion otherPromo = new Promotion(moreInterestingMove != null ? moreInterestingMove : new Move(this));
				otherPromo.selectedPieceCode = code;
				string options = pieceMoved.board.game.PawnPromotionOptions;
				otherPromo.selected = options.IndexOf(code);
				otherPromo.promotedPiece = game.CreatePiece(team, code, to, board);
				game.chessMoves.SetCurrentMove(parentMove);
				game.chessMoves.CurrentMove.next.Insert(0, new MoveNode(thisNode.index, otherPromo, ""));
				game.chessMoves.RedoMove(0);
				return;
			}
			selectedPieceCode = code;
			if (promotedPiece == null) {
				UnityEngine.Debug.Log("new promotion? " + code);
				promotedPiece = game.CreatePiece(team, code, to, board);
			} else {
				UnityEngine.Debug.Log("reactivating old promotion " + code);
				promotedPiece.gameObject.SetActive(true);
				game.SetPiece(promotedPiece, board, to);
			}
			UnityEngine.Debug.Log("did the old switcharoo");
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
				UnityEngine.Debug.Log("woah, promoted piece "+promotedPiece.code+" is not in list? is it in "+index+" ");
				if (index < 0) {
					UnityEngine.Debug.Log("we got a problem here, can't find "+pieceMoved+" or "+promotedPiece+" in list...");
				} else {
					UnityEngine.Debug.Log("...."+team.Pieces[index].code);
				}
			}
			team.Pieces[index] = pieceMoved;
			promotedPiece.gameObject.SetActive(false);
			//ChessGame.DestroyChessObject(promotedPiece.gameObject);
			//promotedPiece = null;
			pieceMoved.board.RecalculatePieceMoves();
			selectedPieceCode = null; // marker that will force selection again
		}

		public override string ToString() {
			return base.ToString() + "=" + (promotedPiece != null ? promotedPiece.code : "?");
		}
	}

	public override void GetMoves(List<Move> out_moves, MoveKind moveKind) {
		Piece p = piece;
		Coord coord = p.GetCoord();
		Board board = p.board;
		Coord dir = team.pawnDirection;
		List<Move> pawnMoves = new List<Move>();
		if (moveKind.HasFlag(MoveKind.Move)) {
			StandardMoves(new Coord[] { dir }, 1, pawnMoves, MoveKind.Move);
			if (piece.moveCount == 0) {
				Coord rushedMove = coord + dir * 2;
				Tile tile = board.GetTile(rushedMove);
				if (tile != null && tile.GetPiece() == null) {
					pawnMoves.Add(new DoubleMove(p, coord, rushedMove));
				}
			}
		}
		if (moveKind.HasFlag(MoveKind.Defend) || moveKind.HasFlag(MoveKind.Attack)) {
			StandardMoves(new Coord[] { dir + Coord.left, dir + Coord.right }, 1, pawnMoves, MoveKind.AttackDefend);
		}
		Move leftEP = HasPossibleEnPassant(board, p, coord, coord + dir + Coord.left, coord + Coord.left);
		if (leftEP != null) { pawnMoves.Add(leftEP); }
		Move rightEP = HasPossibleEnPassant(board, p, coord, coord + dir + Coord.right, coord + Coord.right);
		if (rightEP != null) { pawnMoves.Add(rightEP); }
		// TODO pawn promotion
		for(int i = 0; i < pawnMoves.Count; ++i) {
			Move m = pawnMoves[i];
			if (IsLastRow(board, m.to)) {
				pawnMoves[i] = new Promotion(pawnMoves[i]);
			}
		}
		out_moves.AddRange(pawnMoves);
	}

	public bool IsLastRow(Board board, Coord coord) {
		Team team = piece.team;
		Coord moveDirection = team.pawnDirection;
		bool isValidCoord = board.GetTile(coord) != null;
		bool nextWouldBeValid = board.GetTile(coord + moveDirection) != null;
		return isValidCoord && !nextWouldBeValid;
	}

	public override void DoMove(Move move) {
		base.DoMove(move);
		// TODO check if piece moved to last possible row.
		// if so, show promotion UI.
		// after promotion UI, create the selected piece in the capture area
		// do a PawnPromotion move, which is a capture of that piece as capturing the pawn as the next move
		// use the same move index as the move into the last row.
	}

	public override void UndoMove(Move move) {
		base.UndoMove(move);
		// if undoing a PawnPromotion, show the PawnPromotion UI again
	}

	private Move HasPossibleEnPassant(Board board, Piece p, Coord thisPieceLocation, Coord nextPieceLocation, Coord otherPieceLocation) {
		Tile leftTile = board.GetTile(otherPieceLocation);
		if (leftTile == null) { return null; }
		Piece possibleTarget = leftTile.GetPiece();
		if (possibleTarget == null || possibleTarget.code != p.code) {
			return null;
		}
		//UnityEngine.Debug.Log("maybe en passant?");
		Pawn pawn;
		if (possibleTarget.moveCount != 1 || (pawn = possibleTarget.GetComponent<Pawn>()) == null) {
			return null;
		}
		bool onlyJustNowDidDoubleMove = pawn.didDoubleMoveOnTurn == board.game.chessMoves.CurrentMove.index;
		UnityEngine.Debug.Log("en passant " + leftTile.GetCoord() + " just double moved? "+ onlyJustNowDidDoubleMove+
			"  "+ pawn.didDoubleMoveOnTurn+" "+ board.game.chessMoves.CurrentMove.index);
		if (!onlyJustNowDidDoubleMove) return null;
		// TODO create en passant move
		return new EnPassant(p, thisPieceLocation, nextPieceLocation, possibleTarget, otherPieceLocation);
	}
}

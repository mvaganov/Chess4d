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
		public string newPieceCode;
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
			UnityEngine.Debug.Log("DID pawn promotion to \'" + newPieceCode + "\'");
			if (promotedPiece == null) {
				// open up the piece selection UI
				PieceSelection selectionUi = FindObjectOfType<PieceSelection>(true);
				string options = pieceMoved.board.game.PawnPromotionOptions;
				if (selected == -1) { selected = options.IndexOf("Q"); }
				selectionUi.SelectPiece(options, pieceMoved.team, selected, DoReplacement);
			} else {
				UnityEngine.Debug.Log("already selected replacement: " + promotedPiece);
			}
		}

		public override void Undo() {
			//base.Undo();
			UnityEngine.Debug.Log("UNdid pawn promotion to " + newPieceCode);
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
			newPieceCode = options[index].ToString();
			DoReplacement(newPieceCode);
		}

		public class PreviousForm : UnityEngine.MonoBehaviour {
			public Piece previous;
		}

		public void DoReplacement(string code) {
			UnityEngine.Debug.Log("replacing " + pieceMoved.code + " with " + code);
			Board board = pieceMoved.board;
			ChessGame game = board.game;
			Team team = pieceMoved.team;
			promotedPiece = game.CreatePiece(team, code, to, board);
			pieceMoved.transform.SetParent(null, false);
			pieceMoved.gameObject.SetActive(false);
			int index = team.Pieces.IndexOf(pieceMoved);
			team.Pieces[index] = promotedPiece;
			//promotedPiece.gameObject.AddComponent<PreviousForm>().previous = pieceMoved;
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
			team.Pieces[index] = pieceMoved;
			ChessGame.DestroyChessObject(promotedPiece.gameObject);
			promotedPiece = null;
			pieceMoved.board.RecalculatePieceMoves();
		}

		public override string ToString() {
			return base.ToString() + "=" + newPieceCode;
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

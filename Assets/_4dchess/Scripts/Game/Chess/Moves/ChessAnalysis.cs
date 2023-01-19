using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessAnalysis : MonoBehaviour {
	private List<IMove> currentMoves;
	private List<IMove> validMoves;
	private Piece selectedPiece;
	[SerializeField] private MoveHistory moves;
	private List<King> kingsInCheck = new List<King>();
	private Dictionary<Board,BoardState> boardAnalysis = new Dictionary<Board,BoardState>();
	[SerializeField] private ChessGame game;

	public List<IMove> CurrentPieceCurrentMoves => currentMoves;
	public List<IMove> CurrentPieceValidMoves => validMoves;

	public Piece SelectedPiece {
		get => selectedPiece;
		set => selectedPiece = value;
	}

	public void Start() {
		moves.onMove.AddListener(MoveMade);
		moves.onUndoMove.AddListener(MoveMade);
		if (game == null) {
			game = FindObjectOfType<ChessGame>();
		}
	}

	public void MoveMade(MoveNode move) {
		RecalculatePieceMoves();
	}

	public void RecalculatePieceMoves() {
		if (selectedPiece == null) { return; }
		RecalculatePieceMoves(selectedPiece.board);
	}

	public BoardState GetAnalysis(Board board) {
		if (!boardAnalysis.TryGetValue(board, out BoardState analysis)) {
			boardAnalysis[board] = analysis = new BoardState(board);
		}
		return analysis;
	}

	public void RecalculateAllPieceMoves() {
		IEnumerable<Board> boards = game.boards;
		foreach(Board board in boards) {
			RecalculatePieceMoves(board);
		}
	}

	public void RecalculatePieceMoves(Board board) {
		//selectedPiece?.board.RecalculatePieceMoves();
		BoardState analysis = GetAnalysis(board);
		analysis.RecalculatePieceMoves(board);
		List<King.Check> checks = FindChecks(analysis);
		//string xfen = XFEN.ToString(board);
		//if (board.tiles.Count > 0) {
		//	Debug.Log(xfen);
		//}
		if (checks.Count > 0) {
			Debug.Log("CHECK! " + string.Join(", ", checks));
		}
	}

	public List<King.Check> FindChecks(BoardState analysis) {
		List<King.Check> checks = new List<King.Check>();
		List<Piece> allKings = GetAllKings();
		// check each king to see if there are unallied pieces that can capture him
		for (int i = 0; i < allKings.Count; ++i) {
			Piece king = allKings[i];
			Coord kingLocation = king.GetCoord();
			IMove[] moves = analysis.GetMovesTo(kingLocation);
			if (moves != null) {
				for (int m = 0; m < moves.Length; ++m) {
					IMove move = moves[m];
					if (move.GetType() == typeof(Defend)) { continue; }
					if (move is Pawn.Promotion pp) {
						move = pp.moreInterestingMove;
					}
					Capture cap = move as Capture;
					if (cap != null && cap.pieceCaptured == king && !cap.pieceMoved.team.IsAlliedWith(king.team)) {
						// the current move as enabling such a capture as a Check
						King.Check check = new King.Check(game.chessMoves.CurrentMove.move, cap);
						checks.Add(check);
					}
				}
			}
		}
		return checks;
	}

	public List<Piece> GetAllKings() {
		List<Piece> allKings = new List<Piece>();
		for (int t = 0; t < game.teams.Count; ++t) {
			for (int p = 0; p < game.teams[t].Pieces.Count; ++p) {
				Piece king = game.teams[t].Pieces[p];
				if (king.code != "K") { continue; }
				allKings.Add(king);
			}
		}
		return allKings;
	}

	public bool IsValidMove(Coord coord) {
		return validMoves != null && validMoves.FindIndex(im => im is PieceMove m && m.to == coord) >= 0;
	}

	public List<IMove> GetMovesAt(Coord coord, Func<IMove, bool> filter) {
		List<IMove> moves = new List<IMove>();
		for (int i = 0; i < currentMoves.Count; i++) {
			IMove imove = currentMoves[i];
			PieceMove move = imove as PieceMove;
			if (filter != null && !filter(move)) { continue; }
			if (move.to == coord) {
				moves.Add(move);
			}
		}
		return moves;
	}

	public void SetCurrentPiece(Piece piece) {
		if (currentMoves == null) { currentMoves = new List<IMove>(); } else { currentMoves.Clear(); }
		if (validMoves == null) { validMoves = new List<IMove>(); } else { validMoves.Clear(); }
		selectedPiece = piece;
		if (selectedPiece == null) { return; }
		List<IMove> pieceMoves = new List<IMove>();
		piece.GetMoves(pieceMoves);
		for (int i = pieceMoves.Count-1; i >= 0; --i) {
			if (pieceMoves[i] as Defend != null) {
				pieceMoves.RemoveAt(i);
			}
		}
		currentMoves.AddRange(pieceMoves);
		for (int i = 0; i < pieceMoves.Count; i++) {
			if (IsValidMove(piece, pieceMoves[i])) {
				if (validMoves.IndexOf(pieceMoves[i]) >= 0) {
					Debug.Log($"duplicate move? {string.Join(", ", pieceMoves)}");
				} else {
					validMoves.Add(pieceMoves[i]);
				}
			}
		}
	}

	private bool IsValidMove(Piece piece, IMove move) {
		if (piece == null) { return false; }
		Capture cap = move as Capture;
		if (cap != null && (cap.pieceCaptured == null || piece.team.IsAlliedWith(cap.pieceCaptured.team))) {
			return false;
		}
		return true;
	}
}

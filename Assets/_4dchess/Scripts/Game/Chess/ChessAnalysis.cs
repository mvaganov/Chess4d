using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessAnalysis : MonoBehaviour {
	private List<Move> currentMoves;
	private List<Move> validMoves;
	private Piece selectedPiece;
	[SerializeField] private MoveHistory moves;
	private List<King> kingsInCheck = new List<King>();
	private Dictionary<Board,BoardAnalysis> boardAnalysis = new Dictionary<Board,BoardAnalysis>();
	[SerializeField] private ChessGame game;

	public List<Move> CurrentPieceCurrentMoves => currentMoves;
	public List<Move> CurrentPieceValidMoves => validMoves;

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

	public BoardAnalysis GetAnalysis(Board board) {
		if (!boardAnalysis.TryGetValue(board, out BoardAnalysis analysis)) {
			boardAnalysis[board] = analysis = new BoardAnalysis(board);
		}
		return analysis;
	}

	public void RecalculatePieceMoves(Board board) {
		//selectedPiece?.board.RecalculatePieceMoves();
		BoardAnalysis analysis = GetAnalysis(board);
		analysis.RecalculatePieceMoves();
		List<King.Check> checks = FindChecks(analysis);
		if (checks.Count > 0) {
			Debug.Log("CHECK! " + string.Join(", ", checks));
		}
	}

	public List<King.Check> FindChecks(BoardAnalysis analysis) {
		List<King.Check> checks = new List<King.Check>();
		List<Piece> allKings = GetAllKings();
		// check each king to see if there are unallied pieces that can capture him
		for (int i = 0; i < allKings.Count; ++i) {
			Piece king = allKings[i];
			Coord kingLocation = king.GetCoord();
			List<Move> moves = analysis.GetMovesTo(kingLocation);
			for (int m = 0; m < moves.Count; ++m) {
				Move move = moves[m];
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
		return validMoves != null && validMoves.FindIndex(m => m.to == coord) >= 0;
	}

	public List<Move> GetMovesAt(Coord coord, Func<Move, bool> filter) {
		List<Move> moves = new List<Move>();
		for (int i = 0; i < currentMoves.Count; i++) {
			Move move = currentMoves[i];
			if (filter != null && !filter(move)) { continue; }
			if (move.to == coord) {
				moves.Add(move);
			}
		}
		return moves;
	}

	public void SetCurrentPiece(Piece piece) {
		if (currentMoves == null) { currentMoves = new List<Move>(); } else { currentMoves.Clear(); }
		if (validMoves == null) { validMoves = new List<Move>(); } else { validMoves.Clear(); }
		selectedPiece = piece;
		if (selectedPiece == null) { return; }
		List<Move> pieceMoves = new List<Move>();
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

	private bool IsValidMove(Piece piece, Move move) {
		if (piece == null) { return false; }
		Capture cap = move as Capture;
		if (cap != null && (cap.pieceCaptured == null || piece.team.IsAlliedWith(cap.pieceCaptured.team))) {
			return false;
		}
		return true;
	}
}

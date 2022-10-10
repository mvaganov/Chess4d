using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessAnalysis : MonoBehaviour {
	private List<Move> currentMoves;
	private List<Move> validMoves;
	private Piece selectedPiece;
	[SerializeField] private MoveHistory moves;
	private King[] kingInCheck;

	private Dictionary<Board,BoardAnalysis> boardAnalysis = new Dictionary<Board,BoardAnalysis>();

	public class BoardAnalysis {
		public Board board;
		private List<List<Move>> movesToLocation = new List<List<Move>>();

		public BoardAnalysis(Board board) {
			this.board = board;
		}

		public void RecalculatePieceMoves() {
			EnsureClearLedger(board, movesToLocation);
			List<Piece> allPieces = board.GetAllPieces();
			//allPieces.ForEach(p => p.MarkMovesAsInvalid());
			List<Move> moves = new List<Move>();
			for (int i = 0; i < allPieces.Count; ++i) {
				Piece p = allPieces[i];
				p.GetMovesForceCalculation(moves);
				AddToMapping(board, movesToLocation, moves);
				moves.Clear();
			}
		}
		private static void EnsureClearLedger<T>(Board board, List<List<T>> out_ledger) {
			for (int i = 0; i < out_ledger.Count; ++i) {
				out_ledger[i].Clear();
			}
			for (int i = out_ledger.Count; i < board.tiles.Count; ++i) {
				out_ledger.Add(new List<T>());
			}
		}
		private static void AddToMapping(Board board, List<List<Move>> out_ledger, List<Move> moves) {
			for (int m = 0; m < moves.Count; ++m) {
				Move mov = moves[m];
				Coord coord = mov.to;
				switch (mov) {
					case Pawn.EnPassant ep: coord = ep.captureCoord; break;
					case Capture cap: coord = cap.captureCoord; break;
						//case Pawn.DoublePawnMove dpm: coord = dpm.to;           break;
						//case Move move:               coord = move.to;          break;
				}
				int tileIndex = board.TileIndex(coord);
				out_ledger[tileIndex].Add(mov);
			}
		}

		private void AddToList(List<List<Piece>> out_ledger, Piece piece, List<Move> moves, Func<Move, Coord> location) {
			for (int m = 0; m < moves.Count; ++m) {
				Coord coord = location(moves[m]);
				int tileIndex = board.TileIndex(coord);
				out_ledger[tileIndex].Add(piece);
			}
		}

		public List<Move> GetMovesTo(Coord coord) {
			int index = board.TileIndex(coord);
			return movesToLocation[index];
		}

	}

	public List<Move> CurrentMoves => currentMoves;
	public List<Move> ValidMoves => validMoves;
	public Piece SelectedPiece {
		get => selectedPiece;
		set => selectedPiece = value;
	}

	public void Start() {
		moves.onMove.AddListener(MoveMade);
		moves.onUndoMove.AddListener(MoveMade);
		ChessGame game = FindObjectOfType<ChessGame>();
		kingInCheck = new King[game.teams.Count];
	}

	public void MoveMade(MoveNode move) {
		RecalculateSelectedPieceMoves();
	}

	public void RecalculateSelectedPieceMoves() {
		if (selectedPiece == null) { return; }
		RecalculateSelectedPieceMoves(selectedPiece.board);
	}

	public BoardAnalysis GetAnalysis(Board board) {
		if (!boardAnalysis.TryGetValue(board, out BoardAnalysis analysis)) {
			boardAnalysis[board] = analysis = new BoardAnalysis(board);
		}
		return analysis;
	}

	public void RecalculateSelectedPieceMoves(Board board) {
		//selectedPiece?.board.RecalculatePieceMoves();
		BoardAnalysis analysis = GetAnalysis(board);
		analysis.RecalculatePieceMoves();
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

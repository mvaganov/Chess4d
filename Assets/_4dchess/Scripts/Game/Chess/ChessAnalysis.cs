using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessAnalysis : MonoBehaviour {
	private List<Move> currentMoves;
	private List<Move> validMoves;
	private Piece selectedPiece;
	public List<Move> CurrentMoves => currentMoves;
	public List<Move> ValidMoves => validMoves;
	public Piece SelectedPiece {
		get => selectedPiece;
		set => selectedPiece = value;
	}

	public bool IsValidMove(Coord coord) {
		return validMoves != null && validMoves.FindIndex(m => m.to == coord) >= 0;
	}

	public List<Move> GetMovesAt(Coord coord, Func<Move, bool> filter) {
		List<Move> moves = new List<Move>();
		for (int i = 0; i < validMoves.Count; i++) {
			Move move = validMoves[i];
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

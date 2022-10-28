using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardAnalysis {
	public Board board;
	private List<List<Move>> movesToLocation = new List<List<Move>>();
	public string identity;

	public BoardAnalysis(Board board) {
		this.board = board;
	}

	public void RecalculatePieceMoves(Move whatJustHappened = null) {
		string currentXfen = board.ToXfen();
		if (identity == currentXfen) {
			//Debug.Log("already calculated, skipping.");
			//return;
		}
		// TODO mark units that need recalculation
		// - this moving unit
		// - units defending this moving unit
		// - 
		// remove that unit's moves from the analysis, then recalculate
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
		identity = currentXfen;
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

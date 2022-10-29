using System.Collections.Generic;
using UnityEngine;

public class BoardAnalysis {
	private Dictionary<Coord, List<Move>> movesToLocations = new Dictionary<Coord, List<Move>>();
	private BoardAnalysis prev;
	public string identity;
	[SerializeField] private Coord BoardSize;

	public BoardAnalysis(Board board) {
		RecalculatePieceMoves(board);
	}

	public BoardAnalysis(BoardAnalysis other) {
		prev = other;
		identity = other.identity;
	}

	private void Init(Board board) {
		BoardSize = board.BoardSize;
		identity = board.ToXfen();
		movesToLocations.Clear();
		//EnsureClearLedger(BoardSize, movesToLocation);
	}

	public void RecalculatePieceMoves(Board board) {
		Init(board);
		List<Piece> allPieces = board.GetAllPieces();
		List<Move> moves = new List<Move>();
		for (int i = 0; i < allPieces.Count; ++i) {
			Piece p = allPieces[i];
			p.GetMovesForceCalculation(p.GetCoord(), moves);
			AddToMapping(BoardSize, movesToLocations, moves);
			moves.Clear();
		}
	}

	void GetMovesInvolving(Piece piece, List<Move> sourceMoves, List<Move> out_moves) {
		for (int i = 0; i < sourceMoves.Count; ++i) {
			Move move = sourceMoves[i];
			if (!move.Involves(piece)) { continue; }
			out_moves.Add(move);
		}
	}

	public BoardAnalysis NewAnalysisAfter(Move move) {
		BoardAnalysis nextAnalysis = new BoardAnalysis(this);
		Dictionary<Coord, List<Move>> movesToRemove = new Dictionary<Coord, List<Move>>();
		HashSet<Piece> relevantPieces = new HashSet<Piece>();
		move.GetMovingPieces(relevantPieces);
		List<Move> foundSomeMoves = new List<Move>();
		foreach (KeyValuePair<Coord,List<Move>> moveToLoc in movesToLocations) {
			foreach (Piece piece in relevantPieces) {
				foundSomeMoves.Clear();
				GetMovesInvolving(piece, moveToLoc.Value, foundSomeMoves);
//				AddTo(movesToRemove, moveToLoc.Key, )
			}
		}

		// TODO mark units that need recalculation
		// - this moving unit
		// - units defending this moving unit
		// - 
		// remove that unit's moves from the analysis, then recalculate
		return nextAnalysis;
	}

	//private static void EnsureClearLedger<T>(Coord boardSize, List<List<T>> out_ledger) {
	//	for (int i = 0; i < out_ledger.Count; ++i) {
	//		out_ledger[i].Clear();
	//	}
	//	int tileCount = boardSize.Area();
	//	for (int i = out_ledger.Count; i < tileCount; ++i) {
	//		out_ledger.Add(new List<T>());
	//	}
	//}

	private static void AddToMapping(Coord boardSize, Dictionary<Coord, List<Move>> out_ledger, List<Move> moves) {
		for (int m = 0; m < moves.Count; ++m) {
			Move mov = moves[m];
			Coord coord = mov.GetRelevantCoordinate();
			AddTo(out_ledger, coord, mov);
		}
	}

	public static void AddTo(Dictionary<Coord, List<Move>> map, Coord coord, Move move) {
		if (!map.TryGetValue(coord, out List<Move> movesToThisTile)) {
			map[coord] = new List<Move>() { move };
		} else {
			movesToThisTile.Add(move);
		}
	}

	//private void AddToList(List<List<Piece>> out_ledger, Piece piece, List<Move> moves, Func<Move, Coord> location) {
	//	for (int m = 0; m < moves.Count; ++m) {
	//		Coord coord = location(moves[m]);
	//		int tileIndex = Board.TileIndex(coord, BoardSize);
	//		out_ledger[tileIndex].Add(piece);
	//	}
	//}

	public List<Move> GetMovesTo(Coord coord) {
		if (movesToLocations.TryGetValue(coord, out List<Move> moves)) {
			return moves;
		}
		if (prev != null) {
			return prev.GetMovesTo(coord);
		}
		return null;
		//if (coord.IsOutOfBounds(BoardSize)) { return null; }
		//int index = Board.TileIndex(coord, BoardSize);
		//return movesToLocation[index];
	}
}

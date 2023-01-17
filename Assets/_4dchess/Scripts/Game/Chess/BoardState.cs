using System.Collections.Generic;
using UnityEngine;

public class BoardState {
	/// <summary>
	/// using an Array of Move instead of a more mutable List because there will be many instances,
	/// (a boardstate per move in the tree) and the vast majority of instances should be immutable,
	/// which allows previously calculated boardstates to be referenced safely.
	/// </summary>
	private Dictionary<Coord, IMove[]> movesToLocations = new Dictionary<Coord, IMove[]>();
	private BoardState prev;
	public string identity;
	public string notes;
	[SerializeField] private Coord BoardSize;
	/// <summary>
	/// shows particularly notable moves. used to show what new moves are enabled by the new state
	/// </summary>
	public IList<IMove> notableMoves;

	public BoardState(Board board) {
		RecalculatePieceMoves(board);
	}

	//public BoardState(BoardState other) {
	//	prev = other;
	//	identity = other.identity;
	//}

	public BoardState() { }

	public static BoardState Next(BoardState other) {
		BoardState next = new BoardState();
		next.prev = other;
		next.identity = other.identity;
		return next;
	}

	public static BoardState Copy(BoardState other) {
		BoardState boardState = new BoardState();
		return boardState;
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
		List<IMove> moves = new List<IMove>();
		for (int i = 0; i < allPieces.Count; ++i) {
			Piece p = allPieces[i];
			p.GetMovesForceCalculation(p.GetCoord(), moves);
			AddToMapping(BoardSize, movesToLocations, moves);
			moves.Clear();
		}
	}

	void GetMovesInvolving(Piece piece, List<IMove> sourceMoves, List<IMove> out_moves) {
		for (int i = 0; i < sourceMoves.Count; ++i) {
			IMove move = sourceMoves[i];
			if (!move.Involves(piece)) { continue; }
			out_moves.Add(move);
		}
	}

	public BoardState NewAnalysisAfter(IMove move, List<IMove> totalNewMoves) {
		move.DoWithoutAnimation(); // make move
		BoardState nextAnalysis = new BoardState(move.Board); // do entire analysis from scratch
		// collapse common memory with previous. also note which moves are new
		UseMemoryFromOldStateWherePossible(nextAnalysis, this, totalNewMoves);
		move.UndoWithoutAnimation(); // unmake move, so the state stays as it should be

//		//Dictionary<Coord, List<Move>> movesToRemove = new Dictionary<Coord, List<Move>>();
//		HashSet<Piece> relevantPieces = new HashSet<Piece>();
//		move.GetMovingPieces(relevantPieces);
//		// for each relevant piece
//			// get location of the piece before the move
//				// if any other pieces were defending that square, mark that we need to recalculate those pieces moves
//			// get the move list of the piece before the move
//				// 
//		// 

//		List<Move> foundSomeMoves = new List<Move>();
//		foreach (KeyValuePair<Coord,List<Move>> moveToLoc in movesToLocations) {
//			foreach (Piece piece in relevantPieces) {
//				foundSomeMoves.Clear();
//				GetMovesInvolving(piece, moveToLoc.Value, foundSomeMoves);
////				AddTo(movesToRemove, moveToLoc.Key, )
//			}
//		}

		// TODO mark units that need recalculation
		// - this moving unit
		// - units defending this moving unit
		// - 
		// remove that unit's moves from the analysis, then recalculate
		return nextAnalysis;
	}

	private static void UseMemoryFromOldStateWherePossible(BoardState nextAnalysis, BoardState older,
	List<IMove> totalNewMoves) {
		// after fully calculating both board states, combine the new moves with the original in memory as much as possible
		foreach (var kvp in older.movesToLocations) {
			IMove[] original = kvp.Value;
			if (!nextAnalysis.movesToLocations.TryGetValue(kvp.Key, out IMove[] newMoves)) {
				newMoves = new IMove[0];
			}
			if (older.IsMoveListCollapsable(original, ref newMoves)) {
				nextAnalysis.movesToLocations[kvp.Key] = original;
			} else {
				if (totalNewMoves != null) {
					for (int i = 0; i < newMoves.Length; ++i) {
						bool moveIsActuallyNew = System.Array.IndexOf(original, newMoves[i]) < 0;
						if (moveIsActuallyNew) {
							totalNewMoves.Add(newMoves[i]);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// tries to obviate the memory of movesB by looking for existing movesA values.
	/// </summary>
	/// <param name="movesA">original move list, should be considered a source of truth</param>
	/// <param name="movesB">a new move list, should reference original if possible</param>
	/// <returns>true if both lists are identical</returns>
	private bool IsMoveListCollapsable(IMove[] movesA, ref IMove[] movesB) {
		if (movesA == movesB) { return true; }
		bool identical = true;
		if (movesA.Length != movesB.Length) { identical = false; }
		int sameIndex = 0;
		for(; sameIndex < movesB.Length; ++sameIndex) {
			if (sameIndex >= movesA.Length ||
			!movesA[sameIndex].Equals(movesB[sameIndex])) { identical = false; break; }
			movesB[sameIndex] = movesA[sameIndex];
		}
		if (!identical) {
			for(int i = sameIndex; i < movesB.Length; ++i) {
				int identicalInA = System.Array.IndexOf(movesA, movesB[i], sameIndex);// movesA.IndexOf(movesB[i], sameIndex);
				if (identicalInA != -1) {
					movesB[i] = movesA[identicalInA];
				}
			}
		}
		return identical;
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

	private static void AddToMapping(Coord boardSize, Dictionary<Coord, IMove[]> out_ledger, 
	List<IMove> moves) {
		for (int m = 0; m < moves.Count; ++m) {
			IMove mov = moves[m];
			Coord coord = mov.GetRelevantCoordinate();
			AddTo(out_ledger, coord, mov);
		}
	}

	public static void AddTo(Dictionary<Coord, IMove[]> map, Coord coord, IMove move) {
		if (!map.TryGetValue(coord, out IMove[] movesToThisTile)) {
			map[coord] = new IMove[] { move };
		} else {
			//movesToThisTile.Add(move);
			System.Array.Resize(ref movesToThisTile, movesToThisTile.Length + 1);
			movesToThisTile[movesToThisTile.Length - 1] = move;
			map[coord] = movesToThisTile;
		}
	}

	//private void AddToList(List<List<Piece>> out_ledger, Piece piece, List<Move> moves, Func<Move, Coord> location) {
	//	for (int m = 0; m < moves.Count; ++m) {
	//		Coord coord = location(moves[m]);
	//		int tileIndex = Board.TileIndex(coord, BoardSize);
	//		out_ledger[tileIndex].Add(piece);
	//	}
	//}

	public IMove[] GetMovesTo(Coord coord) {
		if (movesToLocations.TryGetValue(coord, out IMove[] moves)) {
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

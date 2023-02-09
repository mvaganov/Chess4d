using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameState {
	/// <summary>
	/// using an Array of Move instead of a more mutable List because there will be many instances,
	/// (a boardstate per move in the tree) and the vast majority of instances should be immutable,
	/// which allows previously calculated boardstates to be referenced safely.
	/// </summary>
	private Dictionary<Coord, IGameMoveBase[]> movesToLocations = new Dictionary<Coord, IGameMoveBase[]>();
	private List<King.Check> checks = null;
	public GameState prev;
	private string _identity;
	public string notes;
	public IGameMoveBase TriggeringMove;
	[SerializeField] public Coord BoardSize { get; private set; }
	/// <summary>
	/// shows particularly notable moves. used to show what new moves are enabled by the new state
	/// </summary>
	public IList<IGameMoveBase> notableMoves;
	public Dictionary<Coord, Piece[]> piecesOnBoard = new Dictionary<Coord, Piece[]>();

	public string Identity => _identity != null ? _identity : _identity = XFEN.ToString(this);

	public GameState(Board board, IGameMoveBase moveThatPromptedThisBoardState) {
		TriggeringMove = moveThatPromptedThisBoardState;
		RecalculatePieceMoves(board, moveThatPromptedThisBoardState);
	}

	//public BoardState(BoardState other) {
	//	prev = other;
	//	identity = other.identity;
	//}

	public GameState(IGameMoveBase moveThatPromptedThisBoardState) {
		TriggeringMove = moveThatPromptedThisBoardState;
	}

	public struct MoveStats {
		public int count;
		public int added;
		public int lost;
		public override string ToString() {
			//string newmoves = newMoves != null ? string.Join(", ", newMoves) : "";
			return $"{count}" + (added > 0 ? $" +{added}" : "") + (lost > 0 ? $" -{lost}" : "");
			//+ $" new : {newmoves}";
		}
		public List<IGameMoveBase> allMoves;
		public List<IGameMoveBase> newMoves;
	}

	public Piece GetPieceAt(Coord coord) {
		if (piecesOnBoard.TryGetValue(coord, out Piece[] pieces)) {
			if (pieces.Length != 1) { throw new System.Exception($"{pieces.Length} pieces at {coord}"); }
			return pieces[0];
		}
		return null;
	}

	private void GeneratePiecesOnBoardTable(Board board) {
		piecesOnBoard.Clear();
		List<Team> teams = board.game.teams;
		for (int t = 0; t < teams.Count; ++t) {
			List<Piece> pieces = teams[t].Pieces;
			for (int i = 0; i < pieces.Count; ++i) {
				AddPieceToBoardState(pieces[i]);
			}
		}
	}

	private bool AddPieceToBoardState(Piece piece) {
		if (!piece.TryGetCoord(out Coord coord)) { return false; }
		if (piecesOnBoard.TryGetValue(coord, out Piece[] pieces) && pieces.Length > 0) {
			if (pieces[0] == piece) {
				Debug.LogWarning($"{piece}@{coord} already");
			} else throw new System.Exception($"can't place {piece}@{coord}, {pieces[0].MoveLogic} is already there");
		}
		piecesOnBoard[coord] = new Piece[] { piece };
		return true;
	}

	public King.Check GetCheck(Team team) {
		if (checks == null) { return null; }
		for(int i = 0; i < checks.Count; ++i) {
			if (checks[i].pieceCaptured.team == team) { return checks[i]; }
		}
		return null;
	}

	public MoveStats CalculateMoveStats() {
		MoveStats moveStats = new MoveStats();
		moveStats.allMoves = new List<IGameMoveBase>();
		moveStats.newMoves = new List<IGameMoveBase>();
		foreach (var moveKvp in movesToLocations) {
			moveStats.count += CountValidMoves(moveKvp.Value, null);//moveKvp.Value.Length;
		}
		if (prev == null) {
			moveStats.added = moveStats.count;
			moveStats.lost = 0;
			foreach (var moveKvp in movesToLocations) {
				CountValidMoves(moveKvp.Value, moveStats.allMoves);
			}
		} else {
			Coord c = Coord.zero;
			while (c.Iterate(BoardSize)) {
				Dictionary<Coord, IGameMoveBase[]> prevMoveLocations = prev.movesToLocations;
				movesToLocations.TryGetValue(c, out IGameMoveBase[] these);
				prevMoveLocations.TryGetValue(c, out IGameMoveBase[] older);
				if (these == older) { continue; }
				if (these == null && older != null) {
					moveStats.lost += CountValidMoves(older, moveStats.allMoves);// older.Length;
				} else if (older == null && these != null) {
					moveStats.added += CountValidMoves(these, moveStats.allMoves);// these.Length;
					CountValidMoves(these, moveStats.newMoves);
					//Debug.Log(string.Join(", ", System.Array.ConvertAll(these, m => m.ToString())));
				} else {
					List<IGameMoveBase> newOnesThisSquare = new List<IGameMoveBase>();
					int newOnes = 0, theSame = 0;
					for(int i = 0; i < these.Length; ++i) {
						if (!these[i].IsValid) { continue; }
						int index = System.Array.IndexOf(older, these[i]);
						moveStats.allMoves.Add(these[i]);
						if (index != -1) {
							++theSame;
						} else {
							++newOnes;
							moveStats.newMoves.Add(these[i]);
							newOnesThisSquare.Add(these[i]);
						}
					}
					//Debug.Log("~~~SQ " + c +
					//	"\n" + string.Join(", ", System.Array.ConvertAll(these, m => m.ToString())) +
					//	"--" + string.Join(", ", System.Array.ConvertAll(older, m => m.ToString())) +
					//	"\n{" + string.Join(", ", newOnesThisSquare.ConvertAll(m => m.ToString())) + "}");
					int lostOnes = 0;// older.Length - theSame;
					for(int i = 0; i < older.Length; ++i) {
						if (!older[i].IsValid) { continue; }
						int index = System.Array.IndexOf(these, older[i]);
						if (index < 0) { ++lostOnes; }
					}
					moveStats.lost += lostOnes;
					moveStats.added += newOnes;
				}
			}
		}
		return moveStats;
	}

	public int CountValidMoves(IList<IGameMoveBase> moves, List<IGameMoveBase> allValidMoves) {
		int count = 0;
		for(int i = 0; i < moves.Count; ++i) {
			if (moves[i].IsValid) {
				if (allValidMoves != null) { allValidMoves.Add(moves[i]); }
				++count;
			}
		}
		return count;
	}
	//public static GameState Next(GameState other) {
	//	GameState next = new GameState();
	//	next.prev = other;
	//	//next.identity = other.identity;
	//	return next;
	//}

	//public static GameState Copy(GameState other) {
	//	GameState boardState = new GameState();
	//	return boardState;
	//}

	private void Init(Board board) {
		BoardSize = board.BoardSize;
		//identity = board.ToXfen();
		movesToLocations.Clear();
		GeneratePiecesOnBoardTable(board);
		//EnsureClearLedger(BoardSize, movesToLocation);
	}

	public void RecalculatePieceMoves(Board board, IGameMoveBase moveThatPromptedThisBoardState) {
		Init(board);
		List<Piece> allPieces = board.GetAllPieces();
		List<IGameMoveBase> moves = new List<IGameMoveBase>();
		if (checks != null) { checks.Clear(); }
		for (int i = 0; i < allPieces.Count; ++i) {
			Piece p = allPieces[i];
			p.GetMovesForceCalculation(this, p.GetCoord(), moves);
			UpdateCheckMoves(moveThatPromptedThisBoardState, moves);
			AddToMapping(BoardSize, movesToLocations, moves);
			moves.Clear();
		}
	}

	int UpdateCheckMoves(IGameMoveBase triggeringMove, List<IGameMoveBase> moves) {
		if (triggeringMove == null) { return 0; }
		int count = 0;
		for(int i = 0; i < moves.Count; ++i) {
			if (moves[i] is ICapture && moves[i] is PieceMoveAttack capture && capture.pieceCaptured != null
			&& capture.pieceCaptured.code.ToLower() == "k" && !capture.IsDefend) {
				King.Check check = new King.Check(triggeringMove, capture);
				moves[i] = check;
				++count;
				Debug.Log("CHECK! " + moves[i]);
				if (checks == null) { checks = new List<King.Check>(); }
				checks.Add(check);
			}
		}
		return count;
	}

	void GetMovesInvolving(Piece piece, List<IGameMoveBase> sourceMoves, List<IGameMoveBase> out_moves) {
		for (int i = 0; i < sourceMoves.Count; ++i) {
			IGameMoveBase move = sourceMoves[i];
			if (!move.Involves(piece)) { continue; }
			out_moves.Add(move);
		}
	}

	public GameState NewAnalysisAfter(IGameMoveBase move, List<IGameMoveBase> totalNewMoves) {
		move.DoWithoutAnimation(); // make move
		GameState nextAnalysis = new GameState(move.Board, move); // do entire analysis from scratch
		nextAnalysis.prev = this;
		// collapse common memory with previous. also note which moves are new
		UseMemoryFromOldStateWherePossible(nextAnalysis, this, totalNewMoves);
		move.UndoWithoutAnimation(); // unmake move, so the state stays as it should be
		return nextAnalysis;
	}

	// TODO make this a coroutine
	private static void UseMemoryFromOldStateWherePossible(GameState nextAnalysis, GameState older,
	List<IGameMoveBase> totalNewMoves) {
		// after fully calculating both board states, combine the new moves with the original in memory as much as possible
		foreach (var kvp in older.movesToLocations) {
			IGameMoveBase[] original = kvp.Value;
			if (!nextAnalysis.movesToLocations.TryGetValue(kvp.Key, out IGameMoveBase[] newMoves)) {
				newMoves = new IGameMoveBase[0];
			}
			if (CanFullyCollapseNewMovesIntoOriginal(original, ref newMoves)) {
				nextAnalysis.movesToLocations[kvp.Key] = original;
			} else {
				AddToTotalNewMoves(totalNewMoves, original, newMoves);
			}
		}
	}
	private static void AddToTotalNewMoves(List<IGameMoveBase> totalNewMoves, IGameMoveBase[] original, IList<IGameMoveBase> newMoves) {
		if (totalNewMoves == null) {
			return;
		}
		for (int i = 0; i < newMoves.Count; ++i) {
			bool moveIsActuallyNew = System.Array.IndexOf(original, newMoves[i]) < 0;
			if (moveIsActuallyNew) {
				// double check that it is new... we don't want false negatives.
				StrictNameCheck(newMoves[i], original);
				totalNewMoves.Add(newMoves[i]);
			}
		}
	}
	private static void StrictNameCheck(IGameMoveBase newMove, IList<IGameMoveBase> moves) {
		string newMoveName = newMove.ToString();
		for (int j = 0; j < moves.Count; ++j) {
			if (newMoveName.Equals(moves[j].ToString())) {
				Debug.LogError("failed to recognize duplicate: " +
					newMoveName + newMove.GetType() + " and " + moves[j] + moves[j].GetType() + " " +
					moves[j].Equals(newMove));
			}
		}
	}

	/// <summary>
	/// tries to obviate the memory of movesB by looking for existing movesA values.
	/// </summary>
	/// <param name="movesA">original move list, should be considered a source of truth</param>
	/// <param name="movesB">a new move list, should reference original if possible</param>
	/// <returns>true if both lists are identical</returns>
	private static bool CanFullyCollapseNewMovesIntoOriginal(IGameMoveBase[] movesA, ref IGameMoveBase[] movesB) {
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

	private static void AddToMapping(Coord boardSize, Dictionary<Coord, IGameMoveBase[]> out_ledger, 
	List<IGameMoveBase> moves) {
		for (int m = 0; m < moves.Count; ++m) {
			IGameMoveBase mov = moves[m];
			Coord coord = mov.GetRelevantCoordinate();
			AddTo(out_ledger, coord, mov);
		}
	}

	public static void AddTo(Dictionary<Coord, IGameMoveBase[]> map, Coord coord, IGameMoveBase move) {
		if (!map.TryGetValue(coord, out IGameMoveBase[] movesToThisTile)) {
			map[coord] = new IGameMoveBase[] { move };
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

	public IGameMoveBase[] GetMovesTo(Coord coord) {
		if (movesToLocations.TryGetValue(coord, out IGameMoveBase[] moves)) {
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

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameState {
	/// <summary>
	/// using an Array of Move instead of a more mutable List because there will be many instances,
	/// (a state per move in the tree) and all instances should be immutable,
	/// which allows previously calculated states to be referenced safely.
	/// TODO replace Coord with a more general Key? or create this specific lookup table with a more generalized algorithm?
	/// </summary>
	private Dictionary<Coord, IGameMoveBase[]> movesToLocations = new Dictionary<Coord, IGameMoveBase[]>();
	private List<King.Check> checks = null;
	public GameState prev;
	/// <summary>
	/// cached identity string, which is calculated by doing analysis on the state (XFEN string)
	/// </summary>
	private string _identity;
	public IGameMoveBase TriggeringMove;
	[SerializeField] public Coord BoardSize { get; private set; }
	/// <summary>
	/// shows particularly notable moves. used to show what new moves are enabled by the new state
	/// </summary>
	public IList<IGameMoveBase> notableMoves;
	// TODO optimize piece coordinate checking with lookup table
#if HAVE_COORD_LOOKUP_TABLE
	/// <summary>
	/// where pieces are on the board.
	/// uses an array because in the future, multiple pieces will be possible in the same coordinate. not yet.
	/// TODO generate this lookup table from the <see cref="pieceState"/> list
	/// </summary>
	public Dictionary<Coord, Piece[]> piecesOnBoard = new Dictionary<Coord, Piece[]>();
#endif
	/// <summary>
	/// keep track of piece state per turn. master list for each turn's state.
	/// </summary>
	public Dictionary<Piece, PieceLogic.State> pieceState = new Dictionary<Piece, PieceLogic.State>();

	public string Identity => _identity != null ? _identity : _identity = XFEN.ToString(this);
	public bool PutsSelfInCheck => TriggeringMove != null && TriggeringMove.Piece != null ?
		GetCheck(TriggeringMove.Piece.team) != null : false;

	public GameState(Board board, IGameMoveBase moveThatPromptedThisBoardState) {
		TriggeringMove = moveThatPromptedThisBoardState;
		RecalculatePieceMoves(board, moveThatPromptedThisBoardState);
	}

	public GameState(IGameMoveBase moveThatPromptedThisBoardState) {
		TriggeringMove = moveThatPromptedThisBoardState;
	}

	/// <summary>
	/// diff data structure identifying move delta between two states
	/// </summary>
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

	public Piece GetPieceAt(Coord coord) => AssertPieceAt(coord);

	public bool IsExpectedPiece(Piece piece, Coord from) {
		bool isExpectedPiece = AssertPieceAt(from) == piece;
		return isExpectedPiece;
	}

	public Piece AssertPieceAt(Coord coord) {
#if HAVE_COORD_LOOKUP_TABLE
		if (!piecesOnBoard.TryGetValue(coord, out Piece[] pieces)) {
			return null;
		}
		if (pieces.Length > 0) {
			throw new System.Exception($"more than one piece at {coord}?");
		}
		if (pieces.Length == 0) {
			throw new System.Exception($"should not have empty list in {nameof(piecesOnBoard)}");
		}
#else
		List<Piece> pieces = null;
		foreach (var kvp in pieceState) {
			if (kvp.Value.coord == coord) {
				if (pieces == null) { pieces = new List<Piece>(); }
				pieces.Add(kvp.Key);
			}
		}
#endif
		Piece piece = pieces[0];
		if (!pieceState.TryGetValue(piece, out PieceLogic.State pState)) {
			throw new System.Exception($"{piece} does not have a known state this turn?");
		} else if (pState.coord != coord) {
			throw new System.Exception($"{piece} expected to know it is as {coord}, it thinks it's at {pState.coord}");
		} else if(piece == null) {
			Debug.LogWarning($"explicitly null piece at {coord}? shouldn't it just be missing?");
		}
		return piece;
	}

	public bool RemoveExpectedPiece(Piece piece, Coord from) {		
		if (!pieceState.TryGetValue(piece, out PieceLogic.State pState)) { return false; }
#if HAVE_COORD_LOOKUP_TABLE
		piecesOnBoard.Remove(from);
#endif
		if (!pState.IsOnBoard) { throw new System.Exception("already not on board..."); }
		pState.IsOnBoard = false;
		return true;
	}

	//public bool PlacePiece(Piece piece, Coord to) {
	//	if (piecesOnBoard.TryGetValue(to, out Piece[] pieceHere)) {
	//		throw new System.Exception($"already a piece at {to}");
	//	}
	//	foreach(var kvp in piecesOnBoard) {
	//		if (kvp.Value[0] == piece) {
	//			throw new System.Exception($"piece already at {kvp.Key}, why is it going to {to}?");
	//		}
	//	}
	//	piecesOnBoard[to] = new Piece[] { piece };
	//	pieceState[piece] = pieceState[piece].MovedTo(to);
	//	return true;
	//}

	/// <summary>
	/// TODO restrict or deprecate?
	/// </summary>
	/// <param name="board"></param>
	private void GeneratePiecesOnBoardTable(Board board) {
		movesToLocations.Clear();
		pieceState.Clear();
#if HAVE_COORD_LOOKUP_TABLE
		piecesOnBoard.Clear();
#endif
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
		Piece atCoord = AssertPieceAt(coord);
		if (atCoord != null) {
		//if (piecesOnBoard.TryGetValue(coord, out Piece[] pieces) && pieces.Length > 0) {
			//if (pieces[0] == piece) {
			if (atCoord == piece) {
				Debug.LogWarning($"{piece}@{coord} already");
			} else throw new System.Exception($"can't place {piece}@{coord}, {atCoord} is already there");
		}
#if HAVE_COORD_LOOKUP_TABLE
		piecesOnBoard[coord] = new Piece[] { piece };
#endif
		pieceState[piece] = pieceState[piece].MovedTo(coord);
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
					moveStats.lost += CountValidMoves(older, moveStats.allMoves);
				} else if (older == null && these != null) {
					moveStats.added += CountValidMoves(these, moveStats.allMoves);
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

	private void Init(Board board) {
		BoardSize = board.BoardSize;
		//identity = board.ToXfen();
		GeneratePiecesOnBoardTable(board);
		//EnsureClearLedger(BoardSize, movesToLocation);
	}

	//private void Copy(GameState state) {
	//	BoardSize = state.BoardSize;
	//	CopyPiecesOnBoard(state);
	//	CopyMovesByValue(state);
	//}

	private void CopyState(GameState state) {
		pieceState.Clear();
#if HAVE_COORD_LOOKUP_TABLE
		piecesOnBoard.Clear();
#endif
		// copy the single source of truth
		foreach (var ps in state.pieceState) {
			pieceState[ps.Key] = ps.Value;
#if HAVE_COORD_LOOKUP_TABLE
			if (piecesOnBoard[ps.Value.coord][0] != ps.Key) {
				throw new System.Exception($"source {nameof(GameState)} not set correctly");
			}
#endif
		}
#if HAVE_COORD_LOOKUP_TABLE
		// prefetch the pieces by coordinate table
		foreach (var pieceGroup in state.piecesOnBoard) {
			Piece piece = pieceGroup.Value[0];
			pieceState[piece] = piece.Logic.state;
			piecesOnBoard[pieceGroup.Key] = pieceGroup.Value;
		}
#endif
	}

	//private void CopyMovesByValue(GameState state) {
	//	movesToLocations.Clear();
	//	foreach (var moveGroup in state.movesToLocations) {
	//		movesToLocations[moveGroup.Key] = Clone(moveGroup.Value);
	//	}
	//}

	//private static IGameMoveBase[] Clone(IGameMoveBase[] toCopy) {
	//	IGameMoveBase[] result = new IGameMoveBase[toCopy.Length];
	//	for (int i = 0; i < toCopy.Length; ++i) {
	//		result[i] = toCopy[i];
	//	}
	//	return result;
	//}

	public void RecalculatePieceMoves(Board board, IGameMoveBase moveThatPromptedThisBoardState) {
		Init(board);
		List<Piece> allPieces = GetAllPieces();
		List<IGameMoveBase> moves = new List<IGameMoveBase>();
		if (checks != null) { checks.Clear(); }
		for (int i = 0; i < allPieces.Count; ++i) {
			Piece p = allPieces[i];
			p.GetMovesForceCalculation(this, p.GetCoord(), moves);
			UpdateCheckMoves(moveThatPromptedThisBoardState, moves);
			AddToMapping(movesToLocations, moves);
			moves.Clear();
		}
	}

	public void RecalculatePieceMoves(GameState state, IGameMoveBase moveChaningState) {
		//Copy(state);
		BoardSize = state.BoardSize;
		CopyState(state);
		ApplyMove(moveChaningState); // TODO must apply move to GameState, not Board...
//		moveChaningState.Do();
		List<Piece> allPieces = GetAllPieces();
		List<IGameMoveBase> moves = new List<IGameMoveBase>();
		if (checks != null) { checks.Clear(); }
		for (int i = 0; i < allPieces.Count; ++i) {
			Piece p = allPieces[i];
			p.GetMovesForceCalculation(this, p.GetCoord(), moves);
			UpdateCheckMoves(moveChaningState, moves);
			AddToMapping(movesToLocations, moves);
			moves.Clear();
		}
	}

	public void ApplyMove(IGameMoveBase move) {
		// get the moving piece's possible moves from it's current location
		// remove those moves from the current board state
		// remove the piece from it's current location
		// move the piece to a new location (change it's state data in the master state list)
		// recalculate all moves
		// add the new piece
		// add the new moves
	}

	List<Piece> GetAllPieces() {
		List<Piece> pieces = new List<Piece>();
		//foreach(var kvp in piecesOnBoard) {
		//	pieces.AddRange(kvp.Value);
		//}
		foreach(var kvp in pieceState) {
			pieces.Add(kvp.Key);
		}
		return pieces;
	}

	int UpdateCheckMoves(IGameMoveBase triggeringMove, List<IGameMoveBase> moves) {
		if (triggeringMove == null) { return 0; }
		int count = 0;
		for(int i = 0; i < moves.Count; ++i) {
			if (!IsValidKingCheck(moves[i], out PieceMoveAttack capture)) { continue; }
			King.Check check = new King.Check(triggeringMove, capture);
			moves[i] = check;
			++count;
			//Debug.Log("CHECK! " + moves[i]);
			if (checks == null) { checks = new List<King.Check>(); }
			checks.Add(check);
		}
		return count;
	}

	private bool IsValidKingCheck(IGameMoveBase move, out PieceMoveAttack capture) {
		capture = move as PieceMoveAttack;
		if (capture == null) { return false; }
		Piece target = capture.pieceCaptured;
		return target != null && target.code.ToLower() == "k" && !capture.IsDefend;
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

	private static void UseMemoryFromOldStateWherePossible(GameState nextAnalysis, GameState older,
	List<IGameMoveBase> totalNewMoves) {
		// after fully calculating both board states, combine the new moves with the original in memory as much as possible
		foreach (var kvp in older.movesToLocations) {
			IGameMoveBase[] original = kvp.Value;
			if (!nextAnalysis.movesToLocations.TryGetValue(kvp.Key, out IGameMoveBase[] newMoves)) {
				newMoves = System.Array.Empty<IGameMoveBase>();
			}
			if (CanFullyCollapseNewMovesIntoOriginal(original, ref newMoves)) {
				nextAnalysis.movesToLocations[kvp.Key] = original;
			} else {
				AddToTotalNewMoves(totalNewMoves, original, newMoves);
			}
		}
		foreach(var kvp in nextAnalysis.movesToLocations) {
			if (older.movesToLocations.ContainsKey(kvp.Key)) { continue; }
			IGameMoveBase[] newMovesHere = kvp.Value;
			// any totally new moves should be added to the totalNewMoves list.
			totalNewMoves.AddRange(newMovesHere);
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

	private static void AddToMapping(Dictionary<Coord, IGameMoveBase[]> out_ledger, 
	List<IGameMoveBase> moves) {
		for (int m = 0; m < moves.Count; ++m) {
			IGameMoveBase mov = moves[m];
			Coord coord = mov.GetRelevantCoordinate();
			AddTo(out_ledger, coord, mov);
//			Debug.Log($"{{{mov.ToString()}}}");

			//if (mov.ToString().StartsWith("Nf6")) {
			//	IGameMoveBase[] doublecheck = out_ledger[mov.GetRelevantCoordinate()];
			//	bool foundit = false;
			//	for (int a = 0; a < doublecheck.Length; ++a) {
			//		if (doublecheck[a] is PieceMoveAttack pma && !pma.IsDefend) {
			//			Debug.Log("immediate doublecheck capture " + doublecheck[a]);
			//			foundit = true;
			//		}
			//	}
			//	if (!foundit) {
			//		Debug.Log("not here?...");
			//	}
			//}
		}
	}

	public static void AddTo(Dictionary<Coord, IGameMoveBase[]> map, Coord coord, IGameMoveBase move) {
		//if (move.ToString().StartsWith("Nf6")) {
		//	Debug.Log($"oooh, {coord} : {move}");
		//}
		if (!map.TryGetValue(coord, out IGameMoveBase[] movesToThisTile)) {
			map[coord] = new IGameMoveBase[] { move };
			//if (move.ToString().StartsWith("Nf6")) {
			//	Debug.Log($"newone {coord} : {move}");
			//	Debug.Log($"~~{map[coord].Length}~~{map[coord][0]}      {move}");


			//	IGameMoveBase[] doublecheck = map[move.GetRelevantCoordinate()];
			//	bool foundit = false;
			//	for (int a = 0; a < doublecheck.Length; ++a) {
			//		if (doublecheck[a] is PieceMoveAttack pma && !pma.IsDefend) {
			//			Debug.Log("###immediate doublecheck capture " + doublecheck[a]);
			//			foundit = true;
			//		}
			//	}
			//	if (!foundit) {
			//		Debug.Log("###not here?...");
			//	}
			//}
		} else {
			//movesToThisTile.Add(move);
			System.Array.Resize(ref movesToThisTile, movesToThisTile.Length + 1);
			movesToThisTile[movesToThisTile.Length - 1] = move;
			map[coord] = movesToThisTile;
			//if (move.ToString().StartsWith("Nf6")) {
			//	Debug.Log($"added {coord} : {move}");
			//}
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

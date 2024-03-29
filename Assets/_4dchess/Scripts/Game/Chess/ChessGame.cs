using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChessGame : MonoBehaviour {
	[ContextMenuItem(nameof(GenerateInEditor), nameof(GenerateInEditor)),
	 ContextMenuItem(nameof(GenerateAllBoards), nameof(GenerateAllBoards))]
	public BoardInfo[] boardsAtStart = new BoardInfo[] { new BoardInfo() };
	public PieceInfo[] pieceCodes = new PieceInfo[] {
		new PieceInfo("king", "K"),
		new PieceInfo("pawn", "P"),
		new PieceInfo("knight", "N"),
		new PieceInfo("bishop", "B"),
		new PieceInfo("rook", "R"),
		new PieceInfo("queen", "Q"),
	};
	public string PawnPromotionOptions = "NBRQ";
	private Dictionary<string, PieceInfo> _prefabByCode = null;

	public List<Board> boards = new List<Board>();
	public Board prefab_board;
	[ContextMenuItem(nameof(RecalculateMoves), nameof(RecalculateMoves))]
	public ChessAnalysis analysis;
	public MoveHistory chessMoves;
	public BriefMessage message;
	public Camera orthoMapCamera;
	public List<Team> teams = new List<Team>();
	private int _whoStartsTheGame = 0;
	private bool _gameStarted;
	public bool RespectTurnOrder = true;
	public MoveNode moveNodeBeingProcessed;

	public GameState CurrentGameState => chessMoves.CurrentMoveNode.BoardState;
	public Team TeamWhoseTurnItIs => teams[GetWhosTurnItIs()];
	public int NextMoveIndex => chessMoves.NextMoveIndex;
	public Board GameBoard {
		get {
			StartGameIfNotStartedAlready();
			return boards[0];
		}
	}

	public int WhoStartsTheGame {
		get => _whoStartsTheGame;
		set => _whoStartsTheGame = value;
	}

	public int WhoWentLast {
		get {
			IGameMoveBase im = chessMoves.CurrentMove;
			BasicMove m = im as BasicMove;
			if (m != null && m.pieceMoved != null) {
				return m.pieceMoved.team.TeamIndex;
			}
			return -1;
		}
	}

	public MoveNode FindMoveNode(IGameMoveBase move) {
		if (moveNodeBeingProcessed.move == move) {
			return moveNodeBeingProcessed;
		}
		return chessMoves.FindMoveNode(move);
	}

	public int GetWhosTurnItIs(int whoWentLast = -1) {
		if (teams.Count == 0) return -1;
		if (whoWentLast == -1) {
			whoWentLast = WhoWentLast;
		}
		if (whoWentLast < 0) { return _whoStartsTheGame; }
		int next = whoWentLast + 1;
		while (next >= teams.Count) {
			next -= teams.Count;
		}
		return next;
	}

	public int GetWhosTurnItIsNext() {
		return GetWhosTurnItIs(GetWhosTurnItIs());
	}

	public void OnValidate() {
	}

	private void Awake() {
		if (chessMoves == null) { chessMoves = FindObjectOfType<MoveHistory>(); }
		if (analysis == null) { analysis = FindObjectOfType<ChessAnalysis>(); }
		PurgeEmptyBoardSlots();
	}

	private void PurgeEmptyBoardSlots() {
		for (int i = boards.Count - 1; i >= 0; --i) {
			if (boards[i] == null) { boards.RemoveAt(i); }
		}
	}

	public void GenerateAllBoards() {
		DestroyListOfThingsBackwards(boards);
		for (int i = 0; i < boardsAtStart.Length; ++i) {
			BoardInfo binfo = boardsAtStart[i];
			Board board = CreateBoard();
			boards.Add(board);
			board.LoadXfen(binfo.xfen, teams);
			board.transform.localPosition = binfo.BoardOffset;
		}
	}

	private Board CreateBoard() {
		Board board = CreateObject(prefab_board.gameObject).GetComponent<Board>();
		board.game = this;
		board.transform.SetParent(transform);
		board.GenerateTiles();
		return board;
	}

	public void RecalculateNextUpdate() {
		StartCoroutine(RecalculateMovesCoroutine());
	}

	private IEnumerator RecalculateMovesCoroutine() {
		yield return null;
		RecalculateMoves();
	}

	public void RecalculateMoves() {
		//Debug.Log("recalc");
		for (int i = 0; i < boards.Count; ++i) {
			analysis.RecalculatePieceMoves(boards[i]);
		}
	}

	// TODO make this work correctly?
	public void GenerateInEditor() {
		GenerateAllBoards();
		EditorTool.MarkSceneDirty();
	}

	void Start() {
		StartGameIfNotStartedAlready();
	}

	public void StartGameIfNotStartedAlready() {
		if (_gameStarted) { return; }
		StartGame();
	}

	public void StartGame() {
		_gameStarted = true;
		GenerateAllBoards();
		RecalculateNextUpdate();
		chessMoves.AnnounceCurrentTurn();
	}

	public Piece GetPiece(Team team, string code, Coord coord, Board board, bool forceCreatePiece) {
		Piece piece = team.GetPiece(code, forceCreatePiece);
		Debug.Log("piece " + piece + "? board "+board+"? "+ forceCreatePiece);
		piece.transform.position = board.CoordToWorldPosition(coord);
		board.SetPiece(piece, coord);
		return piece;
	}

	public void UndoMove() {
		chessMoves.UndoMove();
	}

	public void RedoMove() {
		chessMoves.RedoMove();
	}

	public PieceInfo GetPieceInfo(string code) {
		if (_prefabByCode == null) {
			_prefabByCode = new Dictionary<string, PieceInfo>();
			for (int i = 0; i < pieceCodes.Length; ++i) {
				_prefabByCode[pieceCodes[i].code] = pieceCodes[i];
			}
		}
		if (_prefabByCode.TryGetValue(code, out PieceInfo info)) { return info; }
		return null;
	}

	public Piece GetPrefab(string code) {
		PieceInfo info = GetPieceInfo(code);
		return (info != null) ? info.prefab : null;
	}

	public static GameObject CreateObject(GameObject go) {
		if (go == null) { return null; }
#if UNITY_EDITOR
		if (!Application.isPlaying) {
			return (GameObject)PrefabUtility.InstantiatePrefab(go);
		}
#endif
		return Instantiate(go);
	}

	public static void DestroyChessObject(GameObject go) {
		if (go == null) { return; }
#if UNITY_EDITOR
		if (!Application.isPlaying) {
			DestroyImmediate(go);
			return;
		}
#endif
		Destroy(go);
	}

	public static void DestroyListOfThingsBackwards<T>(List<T> things) where T : MonoBehaviour {
		for (int i = things.Count - 1; i >= 0; --i) {
			T thing = things[i];
			if (thing != null) { DestroyChessObject(thing.gameObject); }
			things.RemoveAt(i);
		}
		things.Clear();
	}

	public static bool IsMoveCapture(GameState state, Piece piece, Coord move, out Piece capturedPiece) {
		capturedPiece = state.GetPieceAt(move);//piece.board.GetPiece(move);
		return capturedPiece != null && capturedPiece.team != piece.team;
	}

	public static bool IsMyKing(Piece me, Piece other) {
		if (me == null || other == null || other.code != "K" || other.team != me.team) { return false; }
		return true;
	}
}

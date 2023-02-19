using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessAnalysis : MonoBehaviour {
	private List<IGameMoveBase> currentMoves;
	private List<IGameMoveBase> validMoves;
	private Piece _selectedPiece;
	[SerializeField] private MoveHistory moves;
	private List<King> kingsInCheck = new List<King>();
	private Dictionary<Board,GameState> boardAnalysis = new Dictionary<Board,GameState>();
	[SerializeField] private ChessGame game;

	public List<IGameMoveBase> CurrentPieceCurrentMoves => currentMoves;
	public List<IGameMoveBase> CurrentPieceValidMoves => validMoves;

	public Piece SelectedPiece {
		get => _selectedPiece;
		set => _selectedPiece = value;
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
		if (SelectedPiece == null) { return; }
		RecalculatePieceMoves(SelectedPiece.board);
	}

	public GameState GetAnalysis(Board board, IGameMoveBase moveThatPromptedThisBoardState) {
		if (!boardAnalysis.TryGetValue(board, out GameState analysis)) {
			boardAnalysis[board] = analysis = new GameState(board, moveThatPromptedThisBoardState);
		}
		return analysis;
	}

	public void RecalculateAllPieceMoves() {
		IEnumerable<Board> boards = game.boards;
		foreach(Board board in boards) {
			RecalculatePieceMoves(board);
		}
	}

	// TODO take in a GameState instead of a board.
	public void RecalculatePieceMoves(Board board) {
		IGameMoveBase currentMove = board.game.chessMoves.CurrentMove;
		//selectedPiece?.board.RecalculatePieceMoves();
		GameState analysis = GetAnalysis(board, currentMove);
		analysis.RecalculatePieceMoves(board, currentMove);
		List<King.Check> checks = FindChecks(analysis, GetAllKings(), game.chessMoves.CurrentMove);
		//string xfen = XFEN.ToString(board);
		//if (board.tiles.Count > 0) {
		//	Debug.Log(xfen);
		//}
		if (checks != null && checks.Count > 0) {
			HashSet<Team> checkedTeams = checks.Count > 0 ? new HashSet<Team>() : null;
			HashSet<Team> checkMatedTeams = null;
			// for each check
			for (int i = 0; i < checks.Count; ++i) {
				King.Check check = checks[i];
				// which team is in check
				Team team = check.pieceCaptured.team;
				// if that team has already been processed for checkmate, skip this
				if (checkedTeams.Contains(team)) { continue; }
				checkedTeams.Add(team);
				// determine if that team has even a single move that would not result in check
				List<IGameMoveBase> safeMoveList = HasMoveThatIsntCheck(analysis, team);
				if (safeMoveList.Count == 0) {
					// if there are no non-check moves, it's check mate
					check.isMate = true;
					if (checkMatedTeams == null) { checkMatedTeams = new HashSet<Team> { team }; }
					else { checkMatedTeams.Add(team); }
				}
			}
			string message = "CHECK"+(checkMatedTeams.Count > 0? "MATE" : "")+"!" + string.Join(", ", checks);
			game.message.Text = message;
		}
	}

	private List<IGameMoveBase> HasMoveThatIsntCheck(GameState analysis, Team team) {
		List<IGameMoveBase> safeMoveList = new List<IGameMoveBase>();
		// TODO
		// for each possible move
		// if the move is doable by the given team
		// determine if the move would cause a check by doing analysis on the move
		// if it would not cause check, add it to the safe move list
		return safeMoveList;
	}

	public static List<King.Check> FindChecks(GameState analysis, List<Piece> allKings, IGameMoveBase currentMove) {
		List<King.Check> checks = new List<King.Check>();
		// check each king to see if there are unallied pieces that can capture him
		for (int i = 0; i < allKings.Count; ++i) {
			Piece king = allKings[i];
			Coord kingLocation = king.GetCoord();
			IGameMoveBase[] moves = analysis.GetMovesTo(kingLocation);
			if (moves != null) {
				for (int m = 0; m < moves.Length; ++m) {
					IGameMoveBase move = moves[m];
					if (!move.IsValid) { continue; }
					if (move is Pawn.Promotion pp) {
						move = pp.moreInterestingMove;
					}
					PieceMoveAttack cap = move as PieceMoveAttack;
					if (cap != null && cap.pieceCaptured == king && !cap.pieceMoved.team.IsAlliedWith(king.team)) {
						// the current move as enabling such a capture as a Check
						King.Check check = new King.Check(currentMove, cap);
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
		return validMoves != null && validMoves.FindIndex(im => im is BasicMove m && m.to == coord) >= 0;
	}

	public List<IGameMoveBase> GetMovesAt(Coord coord, Func<IGameMoveBase, bool> filter) {
		List<IGameMoveBase> moves = new List<IGameMoveBase>();
		for (int i = 0; i < currentMoves.Count; i++) {
			IGameMoveBase imove = currentMoves[i];
			BasicMove move = imove as BasicMove;
			if (filter != null && !filter(move)) { continue; }
			if (move.to == coord) {
				moves.Add(move);
			}
		}
		return moves;
	}

	public void SetCurrentPiece(GameState currentState, Piece piece) {
		if (currentMoves == null) { currentMoves = new List<IGameMoveBase>(); } else { currentMoves.Clear(); }
		if (validMoves == null) { validMoves = new List<IGameMoveBase>(); } else { validMoves.Clear(); }
		SelectedPiece = piece;
		if (SelectedPiece == null) { return; }
		List<IGameMoveBase> pieceMoves = new List<IGameMoveBase>();
		piece.GetMoves(currentState, pieceMoves);
		//for (int i = pieceMoves.Count-1; i >= 0; --i) { if (!pieceMoves[i].IsValid) { pieceMoves.RemoveAt(i); } }
		pieceMoves.RemoveAll(m => !m.IsValid);
		currentMoves.AddRange(pieceMoves);
		for (int i = 0; i < pieceMoves.Count; i++) {
			//if (!pieceMoves[i].IsValid) { continue; }
			//if (IsValidMove(piece, pieceMoves[i])) {
				if (validMoves.IndexOf(pieceMoves[i]) >= 0) {
					Debug.Log($"duplicate move? {string.Join(", ", pieceMoves)}");
				} else {
					validMoves.Add(pieceMoves[i]);
				}
			//}
		}
	}

	private bool IsValidMove(Piece piece, IGameMoveBase move) {
		if (piece == null) { return false; }
		PieceMoveAttack cap = move as PieceMoveAttack;
		if (cap != null && (cap.pieceCaptured == null || piece.team.IsAlliedWith(cap.pieceCaptured.team))) {
			return false;
		}
		return true;
	}
}

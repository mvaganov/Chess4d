using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Extended Forsyth-Edwards Notation
public static class XFEN {
	const string whitespace = " \n\t\r";
	[Flags] public enum B {
		None = 0,
		King = 1,
		Pawn = 2,
		Knight = 3,
		Bishop = 4,
		Rook = 5,
		Queen = 6,
		AnyPiece = 7,
		White = 8,
		Black = 16,
		Red = 16 | 8,
	}
	public static Dictionary<B, char> pieceBinaryCodeToLetterMap = new Dictionary<B, char>() {
		[B.Black | B.King] = 'k',
		[B.Black | B.Pawn] = 'p',
		[B.Black | B.Knight] = 'n',
		[B.Black | B.Bishop] = 'b',
		[B.Black | B.Rook] = 'r',
		[B.Black | B.Queen] = 'q',
		[B.White | B.King] = 'K',
		[B.White | B.Pawn] = 'P',
		[B.White | B.Knight] = 'N',
		[B.White | B.Bishop] = 'B',
		[B.White | B.Rook] = 'R',
		[B.White | B.Queen] = 'Q',
	};
	public static Dictionary<Type, B> pieceToBinaryCode = new Dictionary<Type, B>() {
		[typeof(King)] = B.King,
		[typeof(Pawn)] = B.Pawn,
		[typeof(Knight)] = B.Knight,
		[typeof(Bishop)] = B.Bishop,
		[typeof(Rook)] = B.Rook,
		[typeof(Queen)] = B.Queen
	};

	private static B[] teamFlags = { B.White, B.Black, B.Red };
	private static Dictionary<char, B> letterTopieceBinaryCodeMap = new Dictionary<char, B>();
	private static Dictionary<B, Type> binaryCodeToPieceType = new Dictionary<B, Type>();

	static XFEN() {
		foreach (var kvp in pieceBinaryCodeToLetterMap) {
			letterTopieceBinaryCodeMap[kvp.Value] = kvp.Key;
		}
		foreach (var kvp in pieceToBinaryCode) {
			binaryCodeToPieceType[kvp.Value] = kvp.Key;
		}
	}

	public static List<int> GetAllTeamIndex(B code) {
		List<int> teams = new List<int>();
		for (int i = 0; i < teamFlags.Length; ++i) {
			if ((teamFlags[i] & code) == 0) {
				teams.Add(i);
			}
		}
		return teams;
	}

	public static int GetTeamIndex(B code) {
		List<int> teams = GetAllTeamIndex(code);
		if (teams.Count > 1) {
			throw new Exception(code + " piece belongs to multiple teams: " + string.Join("|", teams));
		}
		if (teams.Count == 0) {
			throw new Exception(code + " piece belongs to no teams?");
		}
		return teams[0];
	}

	public static bool TryGetPieceType(char letter, out Type type, out int teamIndex) {
		type = null;
		teamIndex = -1;
		if (!letterTopieceBinaryCodeMap.TryGetValue(letter, out B code)) {
			//throw new Exception($"unable to process fen code '{letter}' as piece");
			return false;
		}
		int pieceCode = (int)code & (int)B.AnyPiece;
		if (!binaryCodeToPieceType.TryGetValue((B)pieceCode, out Type pieceType)) {
			//throw new Exception($"unable to process code {pieceCode} '{letter}' as piece");
			return false;
		}
		type = pieceType;
		teamIndex = GetTeamIndex(code);
		return true;
	}

	public static B ConvertPieceToCode(Piece p) {
		B team = teamFlags[p.team.TeamIndex];
		B piece = pieceToBinaryCode[p.MoveLogic.GetType()];
		return team | piece;
	}
	public static char ConvertPieceToLetter(Piece p) {
		return pieceBinaryCodeToLetterMap[ConvertPieceToCode(p)];
	}
	public static string ToString(Board board) {
		ChessGame game = board.game;
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		int emptySquares;
		Coord c = Coord.zero;
		for (c.row = 0; c.row < board.BoardSize.row; ++c.row) {
			emptySquares = 0;
			for (c.col = 0; c.col < board.BoardSize.col; ++c.col) {
				Piece p = board.GetPiece(c);
				if (p == null) {
					++emptySquares;
				} else {
					if (emptySquares != 0) {
						sb.Append(emptySquares.ToString());
						emptySquares = 0;
					}
					char ch = ConvertPieceToLetter(p);
					sb.Append(ch);
				}
			}
			sb.Append((c.row < board.BoardSize.row - 1) ? "/" : " ");
		}
		Move currentMove = game.chessMoves.CurrentMove.move;
		int teamIndex = currentMove == null ? -1 : currentMove.pieceMoved.team.TeamIndex;
		int currentTeamMove = (teamIndex + 1) % game.teams.Count;
		sb.Append(game.teams[currentTeamMove].name[0]).Append(" ");
		bool rooksCanCastle = false;
		for (int i = 0; i < game.teams.Count; ++i) {
			for (int p = 0; p < game.teams[i].Pieces.Count; ++p) {
				Piece piece = game.teams[i].Pieces[p];
				if (piece.code == "R" && piece.moveCount == 0) {
					if (piece.team.TeamIndex == 0) {
						sb.Append((char)(piece.GetCoord().col + 'A'));
					} else {
						sb.Append((char)(piece.GetCoord().col + 'a'));
					}
					rooksCanCastle = true;
				}
			}
		}
		if (!rooksCanCastle) {
			sb.Append("-");
		}
		if (currentMove != null && currentMove.GetType() == typeof(Pawn.DoubleMove)) {
			sb.Append(" ").Append(game.chessMoves.CurrentMove.move.pieceMoved.GetCoord()).Append(" ");
		} else {
			sb.Append(" - ");
		}
		int halfMoves = game.chessMoves.CountMovesSinceCaptureOrPawnAdvance(board);
		sb.Append(halfMoves % 2).Append(" ").Append(halfMoves / 2);
		return sb.ToString();
	}
	public static void FromString(Board board, IList<Team> teams, string xfen) {
		board.GenerateTilesIfMissing();
		board.ReclaimPieces();
		int index = 0;
		ProcessBoardString(board, teams, xfen, ref index);
		ProcessWhosTurnItIs(teams, xfen, ref index);
		ProcessCastleableRooks(teams, xfen, ref index);
		ProcessLastDoubleMovedPawn(teams, xfen, ref index);
		ProcessHalfMovesSinceCaptureOrPawnMove(board, xfen, ref index);
	}

	private static void ProcessLastDoubleMovedPawn(IList<Team> teams, string xfen, ref int index) {
		TrimWhitespace(xfen, ref index);
		string token = ReadToken(xfen, ref index);
		if (token != "-") {
			Debug.Log("TODO make the last move the pawn that double-moved to this location " + token);
		}
		TrimWhitespace(xfen, ref index);
	}

	private static string ReadToken(string text, ref int index) {
		TrimWhitespace(text, ref index);
		int i = index;
		for (; i < text.Length; i++) {
			if (whitespace.IndexOf(text[i]) >= 0) {
				break;
			}
		}
		string found = text.Substring(index, i - index);
		index = i;
		return found;
	}

	private static void ProcessHalfMovesSinceCaptureOrPawnMove(Board board, string xfen, ref int index) {
		TrimWhitespace(xfen, ref index);
		board.halfMovesSinceCaptureOrPawnMove += ReadNextInteger(board, xfen, ref index);
		board.halfMovesSinceCaptureOrPawnMove += ReadNextInteger(board, xfen, ref index) * 2;
		TrimWhitespace(xfen, ref index);
	}

	private static int ReadNextInteger(Board board, string xfen, ref int index) {
		TrimWhitespace(xfen, ref index);
		int digits = CountDigits(xfen, index);
		int number;
		if (digits > 0) {
			number = int.Parse(xfen.Substring(index, digits));
		} else {
			throw new Exception("expected number of turns since capture or pawn move. found \"" +
				xfen.Substring(index) + "\"");
		}
		TrimWhitespace(xfen, ref index);
		return number;
	}

	private static void ProcessBoardString(Board board, IList<Team> teams, string xfen, ref int i) {
		Coord cursor = Coord.zero;
		bool doneReadingBoard = false;
		TrimWhitespace(xfen, ref i);
		for (; !doneReadingBoard && i < xfen.Length; ++i) {
			char ch = xfen[i];
			int digits = CountDigits(xfen, i);
			if (digits > 0) {
				string numberString = xfen.Substring(i, digits);
				Debug.Log(digits+": "+numberString);
				int number = int.Parse(numberString);
				i += digits - 1;
				cursor.x += number;
			} else switch (ch) {
				case '/': case '\\': case '|': cursor.x = 0; ++cursor.y; break;
				case ' ': doneReadingBoard = true; break;
				default:
						Piece p = ProcessXfenLetter(teams, ch, board, FilterCursor(board, cursor));
						p.JumpToLocalCenter();
						cursor.x++;
						break;
			}
		}
		TrimWhitespace(xfen, ref i);
	}

	private static Coord FilterCursor(Board board, Coord coord) {
		return new Coord(coord.x, board.BoardSize.y - 1 - coord.y);
	}

	public static int CountDigits(string s, int index) {
		int count = 0;
		for (int i = index; i < s.Length; ++i) {
			if (char.IsDigit(s[i])) {
				//Debug.Log(s[i]);
				++count;
			} else {
				break;
			}
		}
		return count;
	}

	private static Piece ProcessXfenLetter(IList<Team> teams, char ch, Board board, Coord cursor) {
		Piece p = ProcessXfenLetter(teams, ch);
		board.SetPiece(p, cursor);
		return p;
	}

	private static Piece ProcessXfenLetter(IList<Team> teams, char ch) {
		char code = ch;
		char.IsUpper(ch);
		Team team;
		if (char.IsUpper(ch)) {
			team = teams[0];
		} else {
			team = teams[1];
			code = char.ToUpper(ch);
		}
		Piece p = team.GetPiece(code.ToString(), false);
		return p;
	}

	private static void TrimWhitespace(string xfen, ref int index) {
		if (whitespace.IndexOf(xfen[index]) >= 0) {
			++index;
		}
	}

	private static void ProcessWhosTurnItIs(IList<Team> teams, string xfen, ref int index) {
		TrimWhitespace(xfen, ref index);
		char nextPlayerInitial = xfen[index++];
		int playerIndex = FindIndex(teams, team => team.name[0] == nextPlayerInitial);
		Debug.Log(teams[playerIndex].name + " goes next");
		TrimWhitespace(xfen, ref index);
	}

	public static int FindIndex<T>(IList<T> list, Func<T, bool> predicate) {
		for (int i = 0; i < list.Count; ++i) {
			if (predicate(list[i])) { return i; }
		}
		return -1;
	}

	private static void ProcessCastleableRooks(IList<Team> teams, string xfen, ref int index) {
		foreach (Team t in teams) { t.PurgeEmptyPieceSlots(); }
		TrimWhitespace(xfen, ref index);
		MakePiecesUncastleable(teams);
		GetCastleColumns(xfen, ref index, out StringBuilder uppercase, out StringBuilder lowercase);
		MakePiecesCastleable(new Team[] { teams[0] }, uppercase.ToString());
		MakePiecesCastleable(new Team[] { teams[1] }, lowercase.ToString().ToUpper());
		TrimWhitespace(xfen, ref index);
	}

	private static void MakePiecesUncastleable(IList<Team> teams, string castleableTypes = "R") {
		ForEachPiece(teams, p => {
			if (castleableTypes.IndexOf(p.code) < 0) { return; }
			p.moveCount = 1;
		});
	}

	private static void GetCastleColumns(string xfen, ref int i, out StringBuilder uppercase, out StringBuilder lowercase) {
		uppercase = new StringBuilder();
		lowercase = new StringBuilder();
		for (; i < xfen.Length; ++i) {
			char ch = xfen[i];
			if (ch == ' ') { break; }
			if (char.IsLower(ch)) { lowercase.Append(ch); }
			if (char.IsUpper(ch)) { uppercase.Append(ch); }
		}
	}

	private static void MakePiecesCastleable(IList<Team> teams, string columns, string castleableTypes = "R") {
		ForEachPiece(teams, p => {
			if (castleableTypes.IndexOf(p.code) < 0) { return; }
			char column = (char)(p.GetCoord().col + 'A');
			if (columns.IndexOf(column) < 0) { return; }
			p.moveCount = 0;
		});
	}

	private static void ForEachPiece(IList<Team> teams, Action<Piece> action) {
		for (int t = 0; t < teams.Count; t++) {
			for (int p = 0; p < teams[t].Pieces.Count; ++p) {
				action.Invoke(teams[t].Pieces[p]);
			}
		}
	}
}

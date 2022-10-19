using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Extended Forsyth-Edwards Notation
public static class XFEN {
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
		int halfMoves = game.chessMoves.CountMovesSinceCaptureOrPawnAdvance();
		sb.Append(halfMoves % 2).Append(" ").Append(halfMoves / 2);
		return sb.ToString();
	}
	public static void FromString(Board board, Team[] teams, string xfen) {
		// TODO clear board
		board.ReclaimPieces();
		// iterate across the string
		Coord cursor = Coord.zero;
		bool doneReadingBoard = false;
		for (int i = 0; !doneReadingBoard && i < xfen.Length; ++i) {
			char ch = xfen[i];
			int number = CountDigits(xfen, i);
			if (number > 0) {
				cursor.x += number;
			} else switch(ch) {
					case '/': case '\\': case '|': cursor.x = 0; ++cursor.y; break;
					case ' ': doneReadingBoard = true; break;
					default:
						// get/make the pieces
						char code = ch;
						char.IsUpper(ch);
						Team team;
						if (char.IsUpper(ch)) {
							team = teams[0];
						} else {
							team = teams[1];
							code = char.ToUpper(ch);
						}
						Piece p = team.CreatePiece(code.ToString());
						board.SetPiece(p, cursor);
						break;
			}
		}
		// set castle-able pieces
		// TODO create a chess move that is a proxy for a pawn move or capture X number of moves ago.
	}

	public static int CountDigits(string s, int index) {
		int count = 0;
		for(int i = index; i < s.Length; ++i) {
			if (char.IsDigit(s[i])) { ++count; }
		}
		return count;
	}
}
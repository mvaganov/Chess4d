using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class PieceInfo {
	public string name;
	public string code;
	public Piece prefab;
	public Sprite[] icons;
	public PieceInfo(string name, string code, Piece prefab) {
		this.name = name;
		this.code = code;
		this.prefab = prefab;
		icons = new Sprite[2];
	}
	public PieceInfo(string name, string code) : this(name, code, null) { }
}

// Extended Forsyth-Edwards Notation
public static class XFEN {
	[Flags] public enum B {
		Black = 0,
		None = 0,
		King = 1,
		Pawn = 2,
		Knight = 3,
		Bishop = 4,
		Rook = 5,
		Queen = 6,
		White = 8,
		Red = 16,
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

	private static B[] teamFlags = { B.Black, B.White, B.Red };
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
		for(c.row = 0; c.row < board.BoardSize.row; ++c.row) {
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
			sb.Append((c.row < board.BoardSize.row-1) ? "/" : " ");
		}
		Move currentMove = game.chessMoves.CurrentMove.move;
		int teamIndex = currentMove == null ? -1 : currentMove.pieceMoved.team.TeamIndex;
		int currentTeamMove = (teamIndex + 1) % game.teams.Count;
		sb.Append(game.teams[currentTeamMove].name[0]).Append(" ");
		bool rooksCanCastle = false;
		for(int i = 0; i < game.teams.Count; ++i) {
			for(int p = 0; p < game.teams[i].Pieces.Count; ++p) {
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
		sb.Append(halfMoves % 2).Append(" ").Append(halfMoves/2);
		return sb.ToString();
	}
}

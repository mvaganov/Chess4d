using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceMove : BasicMove, IMove {
	public PieceMove(Board board, Piece pieceMoved, Coord from, Coord to)
		: base(board, pieceMoved, from, to) {
	}
	public override bool Equals(object obj) {
		return obj.GetType() == typeof(PieceMove) && DuckTypeEquals(obj as BasicMove);
	}

	public override int GetHashCode() {
		return HashCode.Combine(base.GetHashCode(), board, from, to, pieceMoved, Board, Piece);
	}
}

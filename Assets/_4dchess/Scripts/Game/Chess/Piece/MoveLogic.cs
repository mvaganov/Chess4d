using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Piece))]
public class MoveLogic : MonoBehaviour {
	public Piece piece => GetComponent<Piece>();
	public Team team => GetComponent<Piece>().team;
	public virtual void Moves(IEnumerable<Coord> directions, int maxSpaces,
	List<Coord> out_moves, List<Coord> out_captures, List<Coord> out_defends) {
		Piece p = piece;
		Moves(p, p.GetCoord(), directions, maxSpaces, out_moves, out_captures, out_defends);
	}
	public virtual void Moves(Coord position, IEnumerable<Coord> directions, int maxSpaces,
	List<Coord> out_moves, List<Coord> out_captures, List<Coord> out_defends) {
		Moves(piece, position, directions, maxSpaces, out_moves, out_captures, out_defends);
	}
	public static void Moves(Piece self, Coord position, IEnumerable<Coord> directions, int maxSpaces,
	List<Coord> out_moves, List<Coord> out_captures, List<Coord> out_defends) {
		Board board = self.board;
		Tile tile = self.GetTile();
		if (tile == null) { return; }
		Coord cursor;
		foreach (Coord dir in directions) {
			if (dir == Coord.zero && maxSpaces > 0) {
				out_moves?.Add(position);
				continue;
			}
			cursor = position;
			for (int i = 0; i < maxSpaces; ++i) {
				cursor += dir;
				if (!board.IsValid(cursor)) {
					break;
				}
				out_defends?.Add(cursor);
				Piece other = board.GetPiece(cursor);
				if (other != null) {
					bool isAllies = self.team.IsAlliedWith(other.team);
					if (!isAllies) {
						out_captures?.Add(cursor);
					}
					break;
				}
				out_moves?.Add(cursor);
			}
		}
	}
	// TODO GetMoves(List<Coord> out_moves, List<Coord> out_captures, List<Coord> out_defense)
	public virtual void GetMoves(List<Coord> out_moves, List<Coord> out_captures, List<Coord> out_defends) {}
	public virtual void DoMove(Coord coord) {
		piece.SetTile(coord);
		piece.MoveToLocalCenter();
	}
}
[Flags]
public enum MoveCalculation {
	None = 0,
	Moves = 1,
	Captures = 2,
	MovesOrCaptures = 3,
	Defense = 4,
	MovesOrDefense = 5, // not sure when this would be useful
	CapturesOrDefense = 6,
	All = 7
}

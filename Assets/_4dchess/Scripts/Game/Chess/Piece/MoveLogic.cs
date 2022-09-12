using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Piece))]
public class MoveLogic : MonoBehaviour {
	public Piece piece => GetComponent<Piece>();
	public Team team => GetComponent<Piece>().team;
	public virtual List<Coord> Moves(IEnumerable<Coord> directions, int maxSpaces, MoveCalculation calcType) {
		Piece p = piece;
		return Moves(p, p.GetCoord(), directions, maxSpaces, calcType);
	}
	public virtual List<Coord> Moves(Coord position, IEnumerable<Coord> directions, int maxSpaces, MoveCalculation calcType) {
		return Moves(piece, position, directions, maxSpaces, calcType);
	}
	public static List<Coord> Moves(Piece self, Coord position, IEnumerable<Coord> directions, int maxSpaces, MoveCalculation calcType) {
		List<Coord> result = new List<Coord>();
		Board board = self.board;
		Tile tile = self.GetTile();
		if (tile == null) { return result; }
		Coord cursor;
		foreach (Coord dir in directions) {
			if (dir == Coord.zero && maxSpaces > 0) {
				if (calcType.HasFlag(MoveCalculation.Moves)) {
					result.Add(position);
				}
				continue;
			}
			cursor = position;
			for (int i = 0; i < maxSpaces; ++i) {
				cursor += dir;
				if (!board.IsValid(cursor)) {
					break;
				}
				Piece other = board.GetPiece(cursor);
				if (other != null) {
					if (calcType.HasFlag(MoveCalculation.Captures) || calcType.HasFlag(MoveCalculation.Defense)) {
						bool isAllies = self.team.IsAlliedWith(other.team);
						if ((isAllies && calcType.HasFlag(MoveCalculation.Defense))
						|| (!isAllies && calcType.HasFlag(MoveCalculation.Captures))) {
							result.Add(cursor);
						}
					}
					break;
				}
				if (calcType.HasFlag(MoveCalculation.Moves)) {
					result.Add(cursor);
				}
			}
		}
		return result;
	}
	// TODO GetMoves(List<Coord> out_moves, List<Coord> out_captures, List<Coord> out_defense)
	public virtual List<Coord> GetMoves(MoveCalculation calcType) {
		return null;
	}
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

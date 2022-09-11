using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Piece))]
public class MoveLogic : MonoBehaviour {
	public Piece piece => GetComponent<Piece>();
	public Team team => GetComponent<Piece>().team;
	public enum MoveCalculation {
		OnlyMoves, OnlyCaptures, MovesAndCaptures
	}
	public virtual List<Coord> Moves(IEnumerable<Coord> directions, int maxSpaces, MoveCalculation calcType) {
		return Moves(GetComponent<Piece>(), directions, maxSpaces, calcType);
	}
	public static List<Coord> Moves(Piece self, IEnumerable<Coord> directions, int maxSpaces, MoveCalculation calcType) {
		List<Coord> result = new List<Coord>();
		Board board = self.board;
		Coord here = board.GetCoord(self.GetTile());
		Coord cursor;
		foreach (Coord dir in directions) {
			if (dir == Coord.zero) {
				if (calcType != MoveCalculation.OnlyCaptures) {
					result.Add(here);
				}
				continue;
			}
			cursor = here;
			for (int i = 0; i < maxSpaces; ++i) {
				cursor += dir;
				if (!board.IsValid(cursor)) {
					break;
				}
				Piece other = board.GetPiece(cursor);
				if (other != null) {
					if (calcType != MoveCalculation.OnlyMoves) {
						if (other.team != self.team) {
							result.Add(cursor);
						}
					}
					break;
				}
				if (calcType != MoveCalculation.OnlyCaptures) {
					result.Add(cursor);
				}
			}
		}
		return result;
	}
	public virtual List<Coord> GetMoves() {
		return null;
	}
	public virtual void DoMove(Coord coord) {
		piece.SetTile(coord);
		piece.MoveToTile();
	}
}

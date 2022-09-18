using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : MoveLogic {
	// TODO king-side and queen-side castle, optionally don't even give move-into-check as an option
	private static readonly Coord[] movePattern = new Coord[] {
			new Coord(+1, 0),
			new Coord(-1, 0),
			new Coord( 0,+1),
			new Coord( 0,-1),
			new Coord(+1,+1),
			new Coord(-1,+1),
			new Coord(+1,-1),
			new Coord(-1,-1),
	};
	public override void GetMoves(List<Coord> out_moves, List<Coord> out_captures, List<Coord> out_defends) {
		Moves(movePattern, 1, out_moves, out_captures, out_defends);
	}
}

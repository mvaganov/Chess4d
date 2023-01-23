using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defend : Capture {
	public Defend(Board board, Piece pieceMoved, Coord from, Coord to, Piece pieceCaptured, Coord fromCaptured)
		: base(board, pieceMoved, from, to, pieceCaptured, fromCaptured) {
	}
	public override TiledGameObject MakeMark(MemoryPool<TiledGameObject> markPool, bool reverse, Color color) {
		TiledGameObject tgo = base.MakeMark(markPool, reverse, color);
		if (tgo.Label != null) {
			tgo.Label.text = "defend";
		}
		return tgo;
	}
	public override bool Equals(object obj) {
		bool result = obj.GetType() == typeof(Defend) && base.DuckTypeEquals(obj as Capture);
		//if (result == false) {
		//	bool type = obj.GetType() == typeof(Defend);
		//	bool duckTypeCapcture = base.DuckTypeEquals(obj as Capture);
		//	bool BasicTypeCapcture = DuckTypeEquals(obj as BasicMove);
		//	Debug.Log(type + " " + duckTypeCapcture + " " + BasicTypeCapcture);
		//}
		return result;
	}
	public override int GetHashCode() {
		return base.GetHashCode();
	}
	public override string ToString() {
		// https://en.wikipedia.org/wiki/Algebraic_notation_(chess)
		// TODO look for other peices of the same type on the same board that are also able to move to the given coord.
		// if there is more than one, prepend the column id of from.
		// if there are multiple in the same column, provide the row id instead
		// if there are multiple in both row and column, provide the entire from coordinate.
		if (pieceCaptured != null) {
			string identifier = pieceMoved.code;
			if (identifier == "") {
				identifier = from.ColumnId;
			}
			string otherIdentifier = pieceCaptured.code;
			return $"{identifier}{from}^{otherIdentifier}{to}";
		} else {
			return "_" + base.ToString();
		}
	}
}

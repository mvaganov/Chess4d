using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class BoardInfo {
	public string name = "standard";
	[Tooltip("if negative values, board size will be calculated by xfen string")]
	[SerializeField] private Coord _size = Coord.negativeOne;
	[TextArea(1, 6)]
	public string xfen = "rnbqkbnr/pppppppp/////PPPPPPPP/RNBQKBNR W AHah - 0 0";
	public Vector3 BoardOffset = Vector3.zero;
	public Coord Size => _size.col >= 0 && _size.row >= 0 ? _size : _size = SizeFromXfen();
	public Coord SizeFromXfen() {
		int countRows = 1, countCols = 0;
		int col = 0;
		for (int i = 0; i < xfen.Length; i++) {
			char c = xfen[i];
			switch (c) {
				case '/':
					++countRows;
					if (col > countCols) {
						countCols = col;
					}
					col = 0;
					break;
				case ' ':
					i = xfen.Length;
					break;
				default:
					++col;
					break;
			}
		}
		return new Coord(countCols, countRows);
	}
}

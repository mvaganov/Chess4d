using UnityEngine;

[System.Serializable] public struct Coord {
	public Vector2Int vec2i;

	public static readonly Coord zero = new Coord(0, 0);

	public int x {
		get => vec2i.x;
		set => vec2i.x = value;
	}
	public int y {
		get => vec2i.y;
		set => vec2i.y = value;
	}
	public int col { get => x; set => x = value; }
	public int row { get => y; set => y = value; }
	public Coord(int col, int row) {
		vec2i = new Vector2Int(col, row);
	}

	public bool Iterate(Coord limit) {
		++col;
		if (col >= limit.col) {
			col = 0;
			++row;
		}
		return row < limit.row;
	}

	public static bool operator==(Coord a, Coord b) {
		return a.col == b.col && a.row == b.row;
	}
	public static bool operator !=(Coord a, Coord b) {
		return a.col != b.col || a.row != b.row;
	}
	public static Coord operator +(Coord a, Coord b) {
		return new Coord(a.x + b.x, a.y + b.y);
	}
	public static Coord operator -(Coord a, Coord b) {
		return new Coord(a.x - b.x, a.y - b.y);
	}
}

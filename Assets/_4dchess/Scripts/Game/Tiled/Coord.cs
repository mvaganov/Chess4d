using UnityEngine;

// TODO replace with pair of shorts or pair of bytes even?
[System.Serializable] public struct Coord {
	public Vector2Int vec2i;

	public static readonly Coord up = new Coord(0, 1);
	public static readonly Coord left = new Coord(-1, 0);
	public static readonly Coord down = new Coord(0, -1);
	public static readonly Coord right = new Coord(1, 0);
	public static readonly Coord zero = new Coord(0, 0);
	public static readonly Coord one = new Coord(1, 1);
	public static readonly Coord negativeOne = new Coord(-1, -1);

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
	public int MagnitudeManhattan => System.Math.Abs(x) + System.Math.Abs(y);
	public Coord normalized => new Coord(System.Math.Sign(x), System.Math.Sign(y));
	public Coord(int col, int row) {
		vec2i = new Vector2Int(col, row);
	}
	public Coord(Vector2Int v) {
		vec2i = v;
	}
	public bool Iterate(Coord limit) {
		++col;
		if (col >= limit.col) {
			col = 0;
			++row;
		}
		return row < limit.row;
	}

	public static bool operator ==(Coord a, Coord b) => a.vec2i == b.vec2i;
	public static bool operator !=(Coord a, Coord b) =>a.vec2i != b.vec2i;
	public static Coord operator +(Coord a, Coord b) => new Coord(a.vec2i + b.vec2i);
	public static Coord operator -(Coord a, Coord b) => new Coord(a.vec2i - b.vec2i);
	public static Coord operator *(Coord a, float n) => new Coord((int)(a.vec2i.x * n), (int)(a.vec2i.y * n));
	public override int GetHashCode() => vec2i.GetHashCode();
	public int Area() => row * col;
	public bool IsOutOfBounds(Coord size) => x < 0 && x >= size.x && y < 0 && y >= size.y;

	public string ToString(string v) {
		switch (v.ToLower()) {
			case "colrow": return $"{x},{y}";
			case "cr": return $"{x},{y}";
			case "rowcol": return $"{y},{x}";
			case "rc": return $"{y},{x}";
			case "xy": return $"{x},{y}";
			case "yx": return $"{y},{x}";
		}
		return ToString();
	}

	public override bool Equals(object obj) {
		if (obj == null) { return false; }
		if (obj is Coord c) { return this == c; }
		return false;
	}

	public string ColumnId => Counting.Alpha(col).ToLower();
	public string RowId => (row + 1).ToString();
	public override string ToString() => $"{ColumnId}{RowId}";
}

using UnityEngine;
using UnitType = System.Int16;

[System.Serializable] public struct Coord {
	public InnerCoord coord;
	[System.Serializable] public struct InnerCoord {
		public UnitType x, y;
		public InnerCoord(UnitType x, UnitType y) { this.x = x; this.y = y; }
		public InnerCoord(int x, int y) { this.x = (UnitType)x; this.y = (UnitType)y; }
		public static bool operator==(InnerCoord a, InnerCoord b) => a.x == b.x && a.y == b.y;
		public static bool operator!=(InnerCoord a, InnerCoord b) => a.x != b.x || a.y != b.y;
		public static InnerCoord operator+(InnerCoord a, InnerCoord b) => new InnerCoord(a.x + b.x, a.y + b.y);
		public static InnerCoord operator-(InnerCoord a, InnerCoord b) => new InnerCoord(a.x - b.x, a.y - b.y);
		public override bool Equals(object obj) => obj is InnerCoord a && this == a;
		public override int GetHashCode() => x ^ y;
	}

	public static readonly Coord up = new Coord(0, 1);
	public static readonly Coord left = new Coord(-1, 0);
	public static readonly Coord down = new Coord(0, -1);
	public static readonly Coord right = new Coord(1, 0);
	public static readonly Coord zero = new Coord(0, 0);
	public static readonly Coord one = new Coord(1, 1);
	public static readonly Coord negativeOne = new Coord(-1, -1);

	public int x {
		get => coord.x;
		set => coord.x = (UnitType)value;
	}
	public int y {
		get => coord.y;
		set => coord.y = (UnitType)value;
	}
	public int col { get => x; set => x = value; }
	public int row { get => y; set => y = value; }
	public int MagnitudeManhattan => System.Math.Abs(x) + System.Math.Abs(y);
	public Coord normalized => new Coord(System.Math.Sign(x), System.Math.Sign(y));
	public Coord(int col, int row) { coord = new InnerCoord(col, row); }
	public Coord(Vector2Int v) { coord = new InnerCoord(v.x, v.y); }
	public Coord(InnerCoord v) { coord = v; }
	public bool Iterate(Coord limit) {
		++col;
		if (col >= limit.col) {
			col = 0;
			++row;
		}
		return row < limit.row;
	}

	public static bool operator ==(Coord a, Coord b) => a.coord == b.coord;
	public static bool operator !=(Coord a, Coord b) =>a.coord != b.coord;
	public static Coord operator +(Coord a, Coord b) => new Coord(a.coord + b.coord);
	public static Coord operator -(Coord a, Coord b) => new Coord(a.coord - b.coord);
	public static Coord operator *(Coord a, float n) => new Coord((int)(a.coord.x * n), (int)(a.coord.y * n));
	public override int GetHashCode() => coord.GetHashCode();
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

using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
	public Coord BoardSize = new Coord(8, 8);
	public Vector3 tileSize = new Vector3(1,2,1);
	public Material[] TileMaterials;
	public List<Tile> tiles = new List<Tile>();
	[ContextMenuItem(nameof(Generate),nameof(Generate))]
	public Tile TilePrefab;
	private Transform _transform;
	private int TileIndex(Coord coord) { return coord.row * BoardSize.col + coord.col; }
	private Coord TileCoord(int index) { return new Coord(index % BoardSize.col, index / BoardSize.col); }
	public bool IsValid(Coord coord) {
		return coord.x >= 0 && coord.x < BoardSize.x && coord.y >= 0 && coord.y < BoardSize.y;
	}
	public Material MaterialOf(Coord tile) {
		int materialIndex = (tile.row + tile.col) % TileMaterials.Length;
		return TileMaterials[materialIndex];
	}

	public Tile GetTile(Coord coord) {
		return tiles[TileIndex(coord)];
	}

	public Piece GetPiece(Coord coord) {
		return GetTile(coord).GetPiece();
	}

	public Coord GetCoord(Tile tile) {
		int index = tiles.IndexOf(tile);
		if (index < 0) {
			throw new System.Exception($"{tile} is not in {this}");
		}
		return TileCoord(index);
	}

	public void Generate() {
		HaveTransform();
		Coord coord = new Coord();
		Game.DestroyListOfThingsBackwards(tiles);
		do {
			Vector3 position = CoordToLocalPosition(coord);
			GameObject tileObject = Game.CreateObject(TilePrefab.gameObject);
			Tile tile = tileObject.GetComponent<Tile>();
			tiles.Add(tile);
			string name = $"{(char)('a' + coord.x)}{coord.y + 1}";
			tileObject.name = name;
			Transform t = tileObject.transform;
			t.SetParent(_transform);
			t.localPosition = position;
			tile.Material = MaterialOf(coord);
			tile.Label.text = name;
		} while (coord.Iterate(BoardSize));
	}

	public Vector3 CoordToLocalPosition(Coord coord) {
		return new Vector3(coord.x * tileSize.x, 0, coord.y * tileSize.z);
	}

	public Vector3 CoordToWorldPosition(Coord coord) {
		HaveTransform();
		return _transform.TransformPoint(CoordToLocalPosition(coord));
	}

	private void HaveTransform() { if (_transform == null) { _transform = transform; } }

	private void DestroyTile(int i) {
		if (i >= tiles.Count || i < 0) { return; }
		Tile tile = tiles[i];
		if (tile == null) { return; }
		Game.DestroyObject(tile.gameObject);
		tiles.RemoveAt(i);
	}
	void Start() {

	}

	void Update() {

	}
}

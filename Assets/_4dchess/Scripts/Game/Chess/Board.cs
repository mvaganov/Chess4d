using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
	[TextArea(1,6)]
	public string xfen = "rnbqkbnr/pppppppp/////PPPPPPPP/RNBQKBNR W AHah - 0 0";
	[ContextMenuItem(nameof(RecalculatePieceMoves), nameof(RecalculatePieceMoves))]
	public ChessGame game;
	public Coord BoardSize = new Coord(8, 8);
	public Vector3 tileSize = new Vector3(1,2,1);
	public Material[] TileMaterials;
	public List<Tile> tiles = new List<Tile>();
	[ContextMenuItem(nameof(Generate),nameof(Generate))]
	public Tile TilePrefab;
	private Transform _transform;
	private BoardAnalysis _analysis;
	public BoardAnalysis Analysis => (_analysis != null) ? _analysis : _analysis = game.analysis.GetAnalysis(this);
	public int TileIndex(Coord coord) { return coord.row * BoardSize.col + coord.col; }
	private Coord TileCoord(int index) { return new Coord(index % BoardSize.col, index / BoardSize.col); }
	public bool IsValid(Coord coord) {
		return coord.x >= 0 && coord.x < BoardSize.x && coord.y >= 0 && coord.y < BoardSize.y;
	}
	public Material MaterialOf(Coord tile) {
		int materialIndex = (tile.row + tile.col) % TileMaterials.Length;
		return TileMaterials[materialIndex];
	}

	public Tile GetTile(Coord coord) {
		int index = TileIndex(coord);
		if (index < 0 || index >= tiles.Count) { return null; }
		return tiles[index];
	}

	public Piece GetPiece(Coord coord) {
		return GetTile(coord).GetPiece();
	}

	public List<Piece> GetAllPieces() {
		List<Piece> allPieces = new List<Piece>();
		for(int i = 0; i < tiles.Count; ++i) {
			Tile tile = tiles[i];
			Piece[] pieces = tile.GetComponentsInChildren<Piece>();
			if (pieces != null && pieces.Length > 1) {
				string errorMessage = ("there are "+ pieces.Length+" at "+tile+": "+
					string.Join(", ", System.Array.ConvertAll(pieces, p=>p.name)));
				//throw new System.Exception(errorMessage);
				Debug.LogError(errorMessage);
			}
			allPieces.AddRange(pieces);
		}
		return allPieces;
	}

	public void RecalculatePieceMoves() {
		game.analysis.RecalculatePieceMoves(this);
	}

	private Coord GetMoveLocation(Move m) => m.to;

	private Coord GetCaptureLocation(Move m) {
		if (m is Capture c) {
			return c.captureCoord;
		}
		return GetMoveLocation(m);
	}

	public List<Move> GetMovesTo(Coord coord) {
		return Analysis.GetMovesTo(coord);
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
		ChessGame.DestroyListOfThingsBackwards(tiles);
		do {
			Vector3 position = CoordToLocalPosition(coord);
			GameObject tileObject = ChessGame.CreateObject(TilePrefab.gameObject);
			Tile tile = tileObject.GetComponent<Tile>();
			tiles.Add(tile);
			string name = coord.ToString();// $"{(char)('a' + coord.x)}{coord.y + 1}";
			tileObject.name = name;
			Transform t = tileObject.transform;
			t.SetParent(_transform);
			t.localPosition = position;
			tile.Material = MaterialOf(coord);
			tile.Label.text = name;
		} while (coord.Iterate(BoardSize));
		if (Application.isPlaying) {
			RecalculatePieceMoves();
		}
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
		ChessGame.DestroyChessObject(tile.gameObject);
		tiles.RemoveAt(i);
	}
	void Start() {
		RecalculatePieceMoves();
		if (game == null) {
			game = GetComponentInParent<ChessGame>();
		}
	}

	public string ToXfen() {
		return XFEN.ToString(this);
	}

	public void LoadXfen(string xfen) {
		Coord cursor = Coord.zero;
		for(int i = 0; i < xfen.Length; ++i) {
			char ch = xfen[i];
			// TODO finish reading FEN string
		}
	}
}

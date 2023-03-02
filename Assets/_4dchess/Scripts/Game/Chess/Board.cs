using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
	[ContextMenuItem(nameof(RecalculatePieceMoves), nameof(RecalculatePieceMoves))]
	public ChessGame game;
	public Coord BoardSize = new Coord(8, 8);
	public Vector3 tileSize = new Vector3(1,2,1);
	public Material[] TileMaterials;
	public List<Tile> tiles = new List<Tile>();
	[ContextMenuItem(nameof(GenerateTiles),nameof(GenerateTiles))]
	public Tile TilePrefab;
	private Transform _transform;
	private GameState _analysis;
	public int halfMovesSinceCaptureOrPawnMove = 0;

	public GameState CurentState => (_analysis != null) ? _analysis
		: _analysis = game.analysis.GetAnalysis(this, game.chessMoves.CurrentMove);

	public void Start() {
		GenerateTilesIfMissing();
		//RecalculatePieceMoves();
		if (game == null) {
			game = GetComponentInParent<ChessGame>();
		}
	}

	public void GenerateTilesIfMissing() {
		if (tiles.Count == 0) {
			GenerateTiles();
		}
	}

	public static int TileIndex(Coord coord, Coord boardSize) => coord.row * boardSize.col + coord.col;
	public int TileIndex(Coord coord) => TileIndex(coord, BoardSize);
	private Coord TileCoord(int index) { return new Coord(index % BoardSize.col, index / BoardSize.col); }
	public bool IsValid(Coord coord) {
		return coord.x >= 0 && coord.x < BoardSize.x && coord.y >= 0 && coord.y < BoardSize.y;
	}

	public Tile GetTile(Coord coord) {
		int index = TileIndex(coord);
		if (index < 0 || index >= tiles.Count) { return null; }
		return tiles[index];
	}

	public Piece GetPieceOnBoard(Coord coord) => GetTile(coord).GetPiece();

	public void SetPiece(Piece piece, Coord coord) {
		Transform t = piece.transform;
		piece.board = this;
		t.SetParent(transform);
		t.Rotate(piece.team.PieceRotation);
		piece.SetTile(coord);
	}

	public List<Piece> GetAllPieces() {
		List<Piece> allPieces = new List<Piece>();
		for(int i = 0; i < tiles.Count; ++i) {
			Tile tile = tiles[i];
			Piece[] pieces = tile.GetComponentsInChildren<Piece>();
			if (pieces != null && pieces.Length > 1) {
				string errorMessage = ("there are " + pieces.Length + " at " + tile + ": " +
					string.Join(", ", System.Array.ConvertAll(pieces, p => p.name)));
				//throw new System.Exception(errorMessage);
				Debug.LogError(errorMessage);
			}
			allPieces.AddRange(pieces);
		}
		return allPieces;
	}

	public void RecalculatePieceMoves() {
		//game.analysis.RecalculatePieceMoves(this);
	}

	private Coord GetMoveLocation(BasicMove m) => m.to;

	private Coord GetCaptureLocation(BasicMove m) {
		if (m is PieceMoveAttack c) {
			return c.to;// captureCoord;
		}
		return GetMoveLocation(m);
	}

	public IList<IGameMoveBase> GetMovesTo(Coord coord) {
		return CurentState.GetMovesTo(coord);
	}

	public Coord GetCoord(Tile tile) {
		int index = tiles.IndexOf(tile);
		if (index < 0) {
			throw new System.Exception($"{tile} is not in {this}");
		}
		return TileCoord(index);
	}

	public void GenerateTiles() {
		HaveTransform();
		Coord coord = new Coord();
		ChessGame.DestroyListOfThingsBackwards(tiles);
		do {
			GenerateTile(coord);
		} while (coord.Iterate(BoardSize));
		//if (Application.isPlaying) {
		//	RecalculatePieceMoves();
		//}
	}

	private void GenerateTile(Coord coord) {
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
	}

	public Material MaterialOf(Coord tile) {
		int materialIndex = (tile.row + tile.col) % TileMaterials.Length;
		return TileMaterials[materialIndex];
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

	//public string ToXfen() { return XFEN.ToString(this); }

	public void LoadXfen(string xfen, IList<Team> teams) { XFEN.FromString(this, teams, xfen); }

	/// <summary>
	/// takes all pieces and puts them into the capture area of each team (for reuse when a new board is made)
	/// </summary>
	public void ReclaimPieces() {
		for(int i = 0; i < tiles.Count; ++i) {
			Tile t = tiles[i];
			Piece p = t.GetPiece();
			if (p != null) {
				p.team.Capture(p);
			}
		}
	}
}

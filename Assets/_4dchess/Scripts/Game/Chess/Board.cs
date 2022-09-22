using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
	public ChessGame game;
	[ContextMenuItem(nameof(RecalculatePieceMoves),nameof(RecalculatePieceMoves))]
	public Coord BoardSize = new Coord(8, 8);
	public Vector3 tileSize = new Vector3(1,2,1);
	public Material[] TileMaterials;
	public List<Tile> tiles = new List<Tile>();
	// TODO move this to ChessGame?
	private List<List<Move>> movesToLocation = new List<List<Move>>();
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
				throw new System.Exception("there are "+ pieces.Length+" at "+tile);
			}
			allPieces.AddRange(pieces);
		}
		return allPieces;
	}

	public void RecalculatePieceMoves() {
		EnsureClearLedger(movesToLocation);
		List<Piece> allPieces = GetAllPieces();
		allPieces.ForEach(p => p.MarkMovesAsInvalid());
		List<Move> moves = new List<Move>();
		for (int i = 0; i < allPieces.Count; ++i) {
			Piece p = allPieces[i];
			p.GetMoves(moves);
			AddToMapping(movesToLocation, moves);
			moves.Clear();
		}
	}

	private Coord GetMoveLocation(Move m) => m.to;

	private Coord GetCaptureLocation(Move m) {
		if (m is Capture c) {
			return c.fromCaptured;
		}
		return GetMoveLocation(m);
	}

	private void EnsureClearLedger<T>(List<List<T>> out_ledger) {
		for (int i = 0; i < out_ledger.Count; ++i) {
			out_ledger[i].Clear();
		}
		for (int i = out_ledger.Count; i < tiles.Count; ++i) {
			out_ledger.Add(new List<T>());
		}
	}

	private void AddToMapping(List<List<Move>> out_ledger, List<Move> moves) {
		for (int m = 0; m < moves.Count; ++m) {
			Move mov = moves[m];
			Coord coord = mov.to;
			switch (mov) {
				case Pawn.EnPassant ep:       coord = ep.fromCaptured;  break;
				case Capture cap:             coord = cap.fromCaptured; break;
				//case Pawn.DoublePawnMove dpm: coord = dpm.to;           break;
				//case Move move:               coord = move.to;          break;
			}
			int tileIndex = TileIndex(coord);
			out_ledger[tileIndex].Add(mov);
		}
	}

	private void AddToList(List<List<Piece>> out_ledger, Piece piece, List<Move> moves, Func<Move, Coord> location) {
		for (int m = 0; m < moves.Count; ++m) {
			Coord coord = location(moves[m]);
			int tileIndex = TileIndex(coord);
			out_ledger[tileIndex].Add(piece);
		}
	}

	public List<Move> GetMovesTo(Coord coord) {
		int index = TileIndex(coord);
		return movesToLocation[index];
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
		ChessGame.DestroyObject(tile.gameObject);
		tiles.RemoveAt(i);
	}
	void Start() {
		RecalculatePieceMoves();
		if (game == null) {
			game = GetComponentInParent<ChessGame>();
		}
	}
}

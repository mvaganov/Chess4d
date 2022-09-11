using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour {
	public Game game;
	public Material material;
	[TextArea(1,8), ContextMenuItem(nameof(Generate),nameof(Generate))]
	public string layout = "rnbkqbnr\npppppppp";
	public Coord start;
	public List<Piece> Pieces = new List<Piece>();
	public Vector3 PieceRotation = Vector3.zero;
	public float speed = 10;
	public Coord pawnDirection = new Coord(0, 1);
	public void Generate() {
		if (game == null) {
			game = FindObjectOfType<Game>();
		}
		Game.DestroyListOfThingsBackwards(Pieces);
		Coord coord = start;
		for(int i = 0; i < layout.Length; ++i) {
			string code = layout.Substring(i, 1);
			if (code == "\n") {
				coord.col = 0;
				coord.row++;
				continue;
			}
			Piece p = CreatePiece(game.GetPrefab(code), coord, game.board);
			p.transform.position += Vector3.up * i;
			coord.col++;
		}
		if (Application.isPlaying) {
			MovePiecesToTile();
		}
	}

	public void MovePiecesToTile() {
		Pieces.ForEach(p => p.MoveToTile());
	}

	public Piece CreatePiece(Piece prefab, Coord coord, Board board) {
		if (prefab == null) { return null; }
		GameObject pieceObject = Game.CreateObject(prefab.gameObject);
		Piece piece = pieceObject.GetComponent<Piece>();
		Pieces.Add(piece);
		string name = this.name + " " + prefab.name + " " + Pieces.Count;
		pieceObject.name = name;
		Transform t = pieceObject.transform;
		t.SetParent(board.transform);
		t.Rotate(PieceRotation);
		piece.team = this;
		piece.board = board;
		piece.Material = material;
		t.position = board.CoordToWorldPosition(coord) + Vector3.up * 10;
		piece.name = name;
		piece.SetTile(coord);
		return piece;
	}
}

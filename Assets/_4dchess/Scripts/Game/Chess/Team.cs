using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour {
	public ChessGame game;
	public Material material;
	[TextArea(1,8), ContextMenuItem(nameof(Generate),nameof(Generate))]
	public string layout = "rnbkqbnr\npppppppp";
	public Coord start;
	public List<Piece> Pieces = new List<Piece>();
	public Vector3 PieceRotation = Vector3.zero;
	public float speed = 10;
	public float jumpHeight = 1;
	public Coord pawnDirection = new Coord(0, 1);
	public int TeamIndex => game.teams.IndexOf(this);
	public bool IsAlliedWith(Team team) {
		return team == this;
	}
	public void Generate() {
		if (game == null) {
			game = FindObjectOfType<ChessGame>();
		}
		ChessGame.DestroyListOfThingsBackwards(Pieces);
		Coord coord = start;
		for(int i = 0; i < layout.Length; ++i) {
			string code = layout.Substring(i, 1);
			if (code == "\n") {
				coord.col = 0;
				coord.row++;
				continue;
			}
			Piece p = game.CreatePiece(this, code, coord, game.board);//CreatePiece(game.GetPrefab(code), coord, game.board);
			p.transform.position += Vector3.up * (10 + i);
			coord.col++;
		}
		if (Application.isPlaying) {
			MovePiecesToTile();
		}
	}

	public void MovePiecesToTile() {
		Pieces.ForEach(p => p.MoveToLocalCenter());
	}

}

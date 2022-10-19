using System;
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
			Piece p = game.GetPiece(this, code, coord, game.board, false);//CreatePiece(game.GetPrefab(code), coord, game.board);
			Pieces.Add(p);
			if (p == null) {
				throw new System.Exception($"no such piece type '{code}'");
			}
			p.transform.position += Vector3.up * (10 + i);
			coord.col++;
		}
		if (Application.isPlaying) {
			MovePiecesToTile();
		}
	}

	public void MovePiecesToTile() {
		Pieces.ForEach(p => p.LerpToLocalCenter());
	}

	public Piece FindSparePiece(string code) {
		Transform holdingArea = transform;
		for (int i = holdingArea.childCount - 1; i >= 0; ++i) {
			Transform child = holdingArea.GetChild(i);
			Piece p = child.GetComponent<Piece>();
			if (p.code == code) { return p; }
		}
		return null;
	}

	public Piece GetPiece(string code, bool forceCreatePiece) {
		Piece piece = !forceCreatePiece ? FindSparePiece(code) : null;
		if (piece == null) {
			piece = CreatePiece(code);
		}
		return piece;
	}

	public Piece CreatePiece(string code) {
		Piece prefab = game.GetPrefab(code);
		GameObject pieceObject = ChessGame.CreateObject(prefab.gameObject);
		Piece piece = pieceObject.GetComponent<Piece>();
		string name = this.name + " " + prefab.name;// + " " + Pieces.Count;
		pieceObject.name = name;
		piece.team = this;
		piece.Material = this.material;
		piece.name = name;
		return piece;
	}

	public void Capture(Piece captured) {
		Transform holdingArea = transform;
		captured.transform.SetParent(holdingArea);
		//Vector3 holdingLocation = Vector3.right * (holdingArea.childCount - 1) / 2f;
		//captured.JumpToLocalCenter(holdingLocation, 3);
		RefreshPiecesInHoldingLocation();
	}

	public void RefreshPiecesInHoldingLocation() {
		Transform holdingArea = transform;
		for (int i = 0; i < holdingArea.childCount; ++i) {
			Piece piece = holdingArea.GetChild(i).GetComponent<Piece>();
			if (piece == null) { continue; }
			Vector3 holdingLocation = Vector3.right * 0.5f * i;
			piece.JumpToLocalCenter(holdingLocation, 3);
		}
	}
}

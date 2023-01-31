using System;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour {
	public ChessGame game;
	public Material material;
	//[TextArea(1,8), ContextMenuItem(nameof(Generate),nameof(Generate))]
	//public string layout = "rnbkqbnr\npppppppp";
	public Coord start;
	public List<Piece> Pieces = new List<Piece>();
	public Vector3 PieceRotation = Vector3.zero;
	public float speed = 10;
	public float jumpHeight = 1;
	public Coord pawnDirection = new Coord(0, 1);
	public int TeamIndex => game.teams.IndexOf(this);

	private void Awake() {
		if (game == null) {
			game = FindObjectOfType<ChessGame>();
		}
		PurgeEmptyPieceSlots();
	}

	public void PurgeEmptyPieceSlots() {
		for (int i = Pieces.Count - 1; i >= 0; --i) {
			if (Pieces[i] == null) { Pieces.RemoveAt(i); }
		}
	}

	public bool IsAlliedWith(Team team) {
		return team == this;
	}

	public void MovePiecesToTile() {
		Pieces.ForEach(p => p.LerpToLocalCenter());
	}

	public Piece FindSparePiece(string code) {
		Transform holdingArea = transform;
		//for (int i = holdingArea.childCount - 1; i >= 0; --i) {
		for (int i = 0; i < holdingArea.childCount; ++i) {
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
		PieceInfo pInfo = game.GetPieceInfo(code);
		GameObject pieceObject = ChessGame.CreateObject(pInfo.prefab.gameObject);
		Piece piece = pieceObject.GetComponent<Piece>();
		string name = this.name + " " + pInfo.name;// + " " + Pieces.Count;
		pieceObject.name = name;
		piece.team = this;
		piece.Material = this.material;
		piece.name = name;
		piece.transform.position = transform.TransformPoint(GetHoldingLocalPosition(Pieces.Count));
		piece.worldIcon.sprite = pInfo.icons[TeamIndex];
		piece.worldIcon.color = material.color;
		Billboard bb = piece.worldIcon.GetComponent<Billboard>();
		bb._camera = game.orthoMapCamera;
		Pieces.Add(piece);
		MoveLogic logic = piece.GetComponent<MoveLogic>();
		if (logic != null) {
			logic.Initialize();
		}
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
			Vector3 holdingLocation = GetHoldingLocalPosition(i);
			if (piece.transform.localPosition != holdingLocation) {
				piece.JumpToLocalCenter(holdingLocation, 3);
			}
		}
	}

	public Vector3 GetHoldingLocalPosition(int index) {
		return Vector3.right * 0.5f * index;
	}
}

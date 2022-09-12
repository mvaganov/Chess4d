using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : TiledGameObject {
	public Piece GetPiece() {
		Piece piece = GetComponentInChildren<Piece>();
		return piece;
	}
}

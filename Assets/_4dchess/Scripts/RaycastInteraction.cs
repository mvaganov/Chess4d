using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastInteraction : MonoBehaviour {
	public Camera cam;
	public Transform rayHitMarker;
	public Gradient hoveredColor;
	public Gradient selectionColor;
	public Gradient moveOption;
	private TiledGameObject currentHovered;
	private IEnumerable<Coord> currentMoves;
	private Board currentMovesBoard;
	TiledGameObject selected;

	void Start() {
		if (cam == null) { cam = Camera.main; }
	}

	void Update() {
		if (Input.GetMouseButtonUp(0)) {
			if (selected != null) {
				if (currentMoves != null) {
					Piece piece = selected as Piece;
					if (currentMoves != null) {
						foreach (Coord coord in currentMoves) {
							Tile t = currentMovesBoard.GetTile(coord);
							t.ResetColor();
						}
					}
				}
				selected.ResetColor();
			}
			selected = currentHovered;
			if (selected != null) {
				selected.ColorCycle(selectionColor);
				Piece piece = selected as Piece;
				if (piece != null) {
					currentMoves = piece.GetMoves();
					currentMovesBoard = piece.board;
					if (currentMoves != null) {
						foreach (Coord coord in currentMoves) {
							Tile t = piece.board.GetTile(coord);
							t.ColorCycle(moveOption);
						}
					}
				}
			}
		}
		if (Input.GetMouseButton(0)) {
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit rh)) {
				if (rayHitMarker != null) {
					rayHitMarker.transform.position = rh.point;
					Vector3 up = rh.normal;
					Vector3 right = cam.transform.right;
					Vector3 forward = Vector3.Cross(up, right); ;
					rayHitMarker.transform.rotation = Quaternion.LookRotation(forward, up);
				}
				TiledGameObject tgo = rh.collider.GetComponent<TiledGameObject>();
				if (tgo != null) {
					if (tgo != currentHovered) {
						if (currentHovered != null) {
							currentHovered.ResetColor();
						}
						currentHovered = tgo;
						currentHovered.ColorCycle(hoveredColor);
					}
				}
			} else {
				if (currentHovered != null) {
					currentHovered.ResetColor();
				}
			}
		}
	}
}

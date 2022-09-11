using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastInteraction : MonoBehaviour {
	public Camera cam;
	public Transform rayHitMarker;
	public Gradient hoveredColor;
	private TiledGameObject currentHovered;
	private IList<Coord> currentMoves;
	TiledGameObject selected;
	public TileVisualization moves;
	public TileVisualization selection;

	void Start() {
		if (cam == null) { cam = Camera.main; }
	}

	void Update() {
		if (Input.GetMouseButtonUp(0)) {
			Coord coord = currentHovered.GetCoord();
			if (selected != null && selected is Piece p) {
				if (coord != p.GetCoord() && currentMoves.IndexOf(coord) >= 0) {
					p.MoveTo(coord);
				} else {
					currentHovered = null;
				}
			}
			//else {
				ClearPreviousSelectionVisuals();
				selected = currentHovered;
				ResetPieceSelectionVisuals();
			//}
		}
		//if (Input.GetMouseButton(0)) {
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out RaycastHit rh)) {
			if (Input.GetMouseButton(0)) {
				PlaceRayHitMarker(rh);
			}
			ColorAccentHovered(rh.collider.GetComponent<TiledGameObject>());
		} else {
			ClearHoverAccent();
		}
		//}
	}
	private void ClearPreviousSelectionVisuals() {
		if (selected == null) { return; }
		selected.ResetColor();
		selection.ClearTiles();
		moves.ClearTiles();
	}
	private void ResetPieceSelectionVisuals() {
		if (selected == null) { return; }
		Piece piece = selected as Piece;
		if (piece == null) { return; }
		currentMoves = piece.GetMoves();
		//if (currentMoves == null) {
		//	Debug.Log("undefined moves for " + piece);
		//} else {
		//	Debug.Log(currentMoves.Count + " moves for " + piece);
		//}
		moves.MarkTiles(currentMoves, piece.board, Color.red);
		selection.MarkTiles(new Coord[] { piece.GetCoord() }, piece.board, Color.green);
	}
	private void ColorAccentHovered(TiledGameObject hoveredObject) {
		if (hoveredObject == currentHovered) { return; }
		ClearHoverAccent();
		currentHovered = hoveredObject;
		if (hoveredObject == null) { return; }
		currentHovered.ColorCycle(hoveredColor, 20);
	}
	private void ClearHoverAccent() {
		if (currentHovered != null) {
			currentHovered.ResetColor();
		}
		currentHovered = null;
	}
	public void PlaceRayHitMarker(RaycastHit rh) {
		if (rayHitMarker == null) {
			return;
		}
		rayHitMarker.transform.position = rh.point;
		Vector3 up = rh.normal;
		Vector3 right = cam.transform.right;
		Vector3 forward = Vector3.Cross(up, right); ;
		rayHitMarker.transform.rotation = Quaternion.LookRotation(forward, up);
	}
}

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
	public TileVisualization moveArrows;

	void Start() {
		if (cam == null) { cam = Camera.main; }
		if (moves == null) { Debug.LogWarning($"missing value for {nameof(moves)}"); }
		if (selection == null) { Debug.LogWarning($"missing value for {nameof(selection)}"); }
		if (moveArrows == null) { Debug.LogWarning($"missing value for {nameof(moveArrows)}"); }
	}

	void Update() {
		if (Input.GetMouseButtonUp(0)) {
			if (currentHovered != null) {
				Coord coord = currentHovered.GetCoord();
				if (selected != null && selected is Piece selectedPiece) {
					if (coord != selectedPiece.GetCoord() && currentMoves.IndexOf(coord) >= 0) {
						if (ChessGame.IsMoveCapture(selectedPiece, coord, out Piece capturedPiece)) {
							Transform holdingArea = selectedPiece.team.transform;
							capturedPiece.transform.SetParent(holdingArea);
							capturedPiece.MoveToLocalCenter(Vector3.right * (holdingArea.childCount - 1) / 2f);
						}
						selectedPiece.MoveTo(coord);
						selectedPiece.board.RecalculatePieceMoves();
					} else {
						currentHovered = null;
					}
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
			DrawArrowsOfMoversToSquare(currentHovered);
		} else {
			ClearHoverAccent();
			DrawArrowsOfMoversToSquare(null);
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
		List<TiledGameObject> marks = moves.CreateMarks(currentMoves, piece.board, Color.yellow);
		for(int i = 0; i < currentMoves.Count; ++i) {
			if (ChessGame.IsMoveCapture(piece, currentMoves[i], out _)) {
				marks[i].Material.color = Color.red;
			}
		}
		selection.CreateMarks(new Coord[] { piece.GetCoord() }, piece.board, Color.green);
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
	public void DrawArrowsOfMoversToSquare(TiledGameObject target) {
		if (moveArrows == null) { return; }
		moveArrows.ClearTiles();
		if (target == null) { return; }
		Board board = target.GetBoard();
		Coord selectedCoord = target.GetCoord();
		if (board == null) { return; }
		List<Piece> pieces = board.GetPiecesThatCanMove(selectedCoord);
		List<Coord> moveSources = new List<Coord>();
		for (int i = 0; i < pieces.Count; ++i) {
			moveSources.Add(pieces[i].GetCoord());
		}
		List<TiledGameObject> arrows = moveArrows.CreateMarks(moveSources, board);
		for (int i = 0; i < arrows.Count; ++i) {
			if (arrows[i] is TiledArrow tiledArrow) {
				tiledArrow.Destination = selectedCoord;
			}
		}
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

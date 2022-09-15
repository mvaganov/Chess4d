using System.Collections.Generic;
using UnityEngine;

public class RaycastInteraction : MonoBehaviour {
	public Camera cam;
	public Transform rayHitMarker;
	public Gradient hoveredColor;
	private TiledGameObject currentHovered;
	private List<Coord> currentMoves;
	TiledGameObject selected;
	public TileVisualization moves;
	public TileVisualization selection;
	public TileVisualization moveArrows;
	public TileVisualization defendArrows;

	void Start() {
		if (cam == null) { cam = Camera.main; }
		if (moves == null) { Debug.LogWarning($"missing value for {nameof(moves)}"); }
		if (selection == null) { Debug.LogWarning($"missing value for {nameof(selection)}"); }
		if (moveArrows == null) { Debug.LogWarning($"missing value for {nameof(moveArrows)}"); }
	}

	void Update() {
		if (Input.GetMouseButtonUp(0)) {
			if (currentHovered != null) {
				// handle a click at the hovered coordinate
				Coord coord = currentHovered.GetCoord();
				if (selected != null && selected is Piece selectedPiece) {
					// move selected piece if the move is valid
					if (coord != selectedPiece.GetCoord() && currentMoves.IndexOf(coord) >= 0) {
						if (ChessGame.IsMoveCapture(selectedPiece, coord, out Piece capturedPiece)) {
							Transform holdingArea = selectedPiece.team.transform;
							capturedPiece.transform.SetParent(holdingArea);
							Vector3 holdingLocation = Vector3.right * (holdingArea.childCount - 1) / 2f;
							capturedPiece.JumpToLocalCenter(holdingLocation, 3);
							//capturedPiece.MoveToLocalCenter();
							currentHovered = null;
						}
						selectedPiece.MoveTo(coord);
						selectedPiece.board.RecalculatePieceMoves();
					} else {
						currentHovered = null;
					}
					defendArrows.ClearTiles();
				}
			}
			ClearPreviousSelectionVisuals();
			selected = currentHovered;
			ResetPieceSelectionVisuals();
		}
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out RaycastHit rh)) {
			if (Input.GetMouseButton(0)) {
				PlaceRayHitMarker(rh);
			}
			ColorAccentHovered(rh.collider.GetComponent<TiledGameObject>());
			DrawSquareDefenders(currentHovered);
			// TODO if currentHovered is one of the valid moves
				// draw threat&defense lines for this piece if it were to move to the currentHovered location
		} else {
			ClearHoverAccent();
			DrawSquareDefenders(null);
		}
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
		List<Coord> moveLocations = new List<Coord>();
		List<Coord> captureLocations = new List<Coord>();
		List<Coord> defendLocations = new List<Coord>();
		piece.GetMoves(moveLocations, captureLocations, defendLocations);
		if (currentMoves == null) { currentMoves = new List<Coord>(); }
		else { currentMoves.Clear(); }
		currentMoves.AddRange(moveLocations);
		currentMoves.AddRange(captureLocations);
		moves.ClearTiles();
		moves.CreateMarks(moveLocations, piece.board, Color.yellow);
		moves.CreateMarks(captureLocations, piece.board, Color.red);
		selection.ClearTiles();
		Coord pieceCoord = piece.GetCoord();
		selection.CreateMarks(new Coord[] { pieceCoord }, piece.board, Color.green);
		defendArrows.ClearTiles();
		for(int i = defendLocations.Count-1; i >= 0; --i) {
			Piece defended = piece.board.GetPiece(defendLocations[i]);
			if (defended == null || !piece.team.IsAlliedWith(defended.team)) {
				defendLocations.RemoveAt(i);
			}
		}
		Coord c = piece.GetCoord();
		defendArrows.CreateMarks(defendLocations, piece.board, tile => {
			if (tile is TiledWire tiledArrow) {
				tiledArrow.Destination = c;
				tiledArrow.Material.color = Color.yellow;
			}
		});
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

	public void DrawSquareDefenders(TiledGameObject target) {
		if (moveArrows == null) { return; }
		moveArrows.ClearTiles();
		if (target == null) { return; }
		Board board = target.GetBoard();
		Coord currentCoord = target.GetCoord();
		if (board == null) { return; }
		List<Piece> pieces = board.GetPiecesThatCanDefend(currentCoord);
		List<Coord> defenders = new List<Coord>();
		Piece selectedPiece = selected as Piece;
		Coord selectedCoord = (selected != null) ? selected.GetCoord() : Coord.zero;
		for (int i = 0; i < pieces.Count; ++i) {
			Coord defenderCoord = pieces[i].GetCoord();
			if (selected != null && defenderCoord == selectedCoord) { continue; }
			defenders.Add(defenderCoord);
		}
		moveArrows.ClearTiles();
		List<TiledGameObject> arrows = moveArrows.CreateMarks(defenders, board, tile => {
			TiledWire tiledArrow = tile as TiledWire;
			if (tiledArrow == null) { return; }
			tiledArrow.Destination = currentCoord;
			tiledArrow.Color = Color.yellow;
			if (selectedPiece != null) {
				Piece defender = board.GetPiece(tiledArrow.GetCoord());
				if (defender.team != selectedPiece.team) {
					tiledArrow.Color = Color.red;
				}
			}
		});
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

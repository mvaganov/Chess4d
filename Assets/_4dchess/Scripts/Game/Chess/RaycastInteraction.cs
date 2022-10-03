using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RaycastInteraction : MonoBehaviour {
	public ChessGame game;
	public ChessAnalysis analysis;
	public ChessVisuals visuals;
	public Camera cam;
	public Transform rayHitMarker;
	public Gradient hoveredColor;
	public TiledGameObject currentHovered;

	private KeyCode undoMove = KeyCode.Backspace;
	private KeyCode redoMove = KeyCode.Space;

	void Start() {
		if (cam == null) { cam = Camera.main; }
		if (game == null) { game = FindObjectOfType<ChessGame>(); }
		if (analysis == null) { analysis = FindObjectOfType<ChessAnalysis>(); }
		if (visuals == null) { visuals = FindObjectOfType<ChessVisuals>(); }
	}

	void Update() {
		if (EventSystem.current.currentSelectedGameObject != null) {
			return;
		}
		if (Input.GetMouseButtonUp(0)) {
			Click();
			RefreshVisuals(currentHovered);
		}
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out RaycastHit rh)) {
			if (Input.GetMouseButton(0)) {
				PlaceRayHitMarker(rh);
			}
			ColorAccentHovered(rh.collider.GetComponent<TiledGameObject>());
			visuals.DrawSquareDefenders(currentHovered);
			// TODO if currentHovered is one of the valid moves
				// draw threat&defense lines for this piece if it were to move to the currentHovered location
		} else {
			ClearHoverAccent();
			visuals.DrawSquareDefenders(null);
		}
	}

	public void RefreshVisuals(TiledGameObject tiledGameObjet) {
		visuals.ClearPreviousSelectionVisuals();
		visuals.selected = tiledGameObjet;
		Board currentPiecesBoard = currentHovered != null ? currentHovered.GetBoard() : null;
		Piece piece = currentPiecesBoard != null ? currentPiecesBoard.GetPiece(currentHovered.GetCoord()) : null;
		//Debug.Log("selecting " + piece);
		analysis.SetCurrentPiece(piece);
		visuals.ResetPieceSelectionVisuals(analysis);
	}

	private void Click() {
		if (currentHovered == null) { return; }
		// handle a click at the hovered coordinate
		Coord coord = currentHovered.GetCoord();
		Piece selectedPiece = analysis.SelectedPiece;
		if (selectedPiece != null) {
			DoMoveAt(selectedPiece, coord);
			visuals.defendArrows.ClearTiles();
			currentHovered = null;
		}
	}

	public void DoMoveAt(Piece selectedPiece, Coord coord) {
		List<Move> moves = analysis.GetMovesAt(coord, MoveIsNotDefensive);
		if (coord != selectedPiece.GetCoord() && moves.Count != 0) {
			switch (moves.Count) {
				case 1:
					game.chessMoves.MakeMove(moves[0], "");
					break;
				default:
					Debug.Log($"TODO must disambiguate between {moves.Count} moves: [{string.Join(", ", moves)}]");
					for (int i = 0; i < moves.Count; i++) {
						Move m = moves[i];
						Debug.Log($"{m.GetType().Name} {m.pieceMoved} {m.from} {m.to}");
					}
					break;
			}
			analysis.RecalculateSelectedPieceMoves();
		} else {
			Debug.Log("invalid move, unselecting.");
		}
	}

	private bool MoveIsNotDefensive(Move move) {
		Capture cap = move as Capture;
		if (cap == null) { return true;}
		return !cap.IsDefend;
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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RaycastInteraction : MonoBehaviour {
	public ChessGame game;
	public Camera cam;
	public Transform rayHitMarker;
	public Gradient hoveredColor;
	private TiledGameObject currentHovered;
	public ChessAnalysis analysis;
	//public TiledGameObject selected;
	//public TileVisualization moves;
	//public TileVisualization captures;
	//public TileVisualization selection;
	//public TileVisualization defendArrows;
	//public TileVisualization tempDefendArrows;
	public ChessVisuals visuals;

	private KeyCode undoMove = KeyCode.Backspace;
	private KeyCode redoMove = KeyCode.Space;
	//public bool showKingDefender;

	void Start() {
		if (cam == null) { cam = Camera.main; }
		if (game == null) { game = FindObjectOfType<ChessGame>(); }
		//if (moves == null) { Debug.LogWarning($"missing value for {nameof(moves)}"); }
		//if (captures == null) { Debug.LogWarning($"missing value for {nameof(captures)}"); }
		//if (selection == null) { Debug.LogWarning($"missing value for {nameof(selection)}"); }
		//if (defendArrows == null) { Debug.LogWarning($"missing value for {nameof(defendArrows)}"); }
		//if (tempDefendArrows == null) { Debug.LogWarning($"missing value for {nameof(tempDefendArrows)}"); }
	}

	void Update() {
		if (EventSystem.current.currentSelectedGameObject != null) {
			return;
		}
		if (Input.GetKeyDown(undoMove)) {
			game.UndoMove();
		}
		if (Input.GetKeyDown(redoMove)) {
			game.RedoMove();
		}
		if (Input.GetMouseButtonUp(0)) {
			if (currentHovered != null) {
				// handle a click at the hovered coordinate
				Coord coord = currentHovered.GetCoord();
				Piece selectedPiece = analysis.SelectedPiece;
				if (selectedPiece != null) {
					//Debug.Log($"...unselected {selected}");
					// move selected piece if the move is valid
					if (coord != selectedPiece.GetCoord() && analysis.IsValidMove(coord)) {
						if (ChessGame.IsMoveCapture(selectedPiece, coord, out Piece capturedPiece)) {
							game.Capture(selectedPiece, capturedPiece, coord, "");
							currentHovered = null;
						} else {
							game.Move(selectedPiece, coord, "");
						}
						selectedPiece.board.RecalculatePieceMoves();
					} else {
						Debug.Log("invalid move, unselecting.");
						currentHovered = null;
					}
					visuals.defendArrows.ClearTiles();
				}
			}
			visuals.ClearPreviousSelectionVisuals();
			visuals.selected = currentHovered;
			//if (selected != null) {
			//	Debug.Log($"selected {selected}");
			//}
			Piece piece = currentHovered != null ? currentHovered.GetBoard().GetPiece(currentHovered.GetCoord()) : null;
			ResetPieceSelectionVisuals(piece);
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

	//private void ClearPreviousSelectionVisuals() {
	//	if (selected == null) { return; }
	//	selected.ResetColor();
	//	defendArrows.ClearTiles();
	//	selection.ClearTiles();
	//	captures.ClearTiles();
	//	moves.ClearTiles();
	//}

	private void ResetPieceSelectionVisuals(Piece piece) {
		Debug.Log("selecting "+ piece);
		analysis.SetCurrentPiece(piece);
		visuals.ResetPieceSelectionVisuals(analysis);
		//if (selected == null) { return; }
		//Piece piece = selected as Piece;
		//if (piece == null) { return; }
		//analysis.SetCurrentPiece(piece);
		////List<Move> pieceMoves = new List<Move>();
		////piece.GetMoves(pieceMoves);
		////if (currentMoves == null) { currentMoves = new List<Move>(); } else { currentMoves.Clear(); }
		////if (validMoves == null) { validMoves = new List<Move>(); } else { validMoves.Clear(); }
		
		////currentMoves.AddRange(pieceMoves);
		////for (int i = 0; i < pieceMoves.Count; i++) {
		////	if (IsValidMove(piece, pieceMoves[i])) {
		////		validMoves.Add(pieceMoves[i]);
		////	}
		////}
		//for (int i = 0; i < analysis.CurrentMoves.Count; ++i) {
		//	Move move = analysis.CurrentMoves[i];
		//	if (!showKingDefender && move is Capture cap && IsMyKing(piece, cap.pieceCaptured)) { continue; }
		//	AddPieceSelectionVisualFor(move, piece.board);
		//}
		//moves.ClearTiles();
		//captures.ClearTiles();
		//selection.ClearTiles();
		//defendArrows.ClearTiles();
		//Coord pieceCoord = piece.GetCoord();
		//selection.CreateMarks(new Move[] { new Move(piece,pieceCoord, pieceCoord) }, piece.board, Color.green);
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

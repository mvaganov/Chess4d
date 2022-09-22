using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RaycastInteraction : MonoBehaviour {
	public ChessGame game;
	public Camera cam;
	public Transform rayHitMarker;
	public Gradient hoveredColor;
	private TiledGameObject currentHovered;
	private List<Move> currentMoves;
	TiledGameObject selected;
	public TileVisualization moves;
	public TileVisualization captures;
	public TileVisualization selection;
	public TileVisualization defendArrows;
	public TileVisualization tempDefendArrows;

	private KeyCode undoMove = KeyCode.Backspace;
	private KeyCode redoMove = KeyCode.Space;

	void Start() {
		if (cam == null) { cam = Camera.main; }
		if (game == null) { game = FindObjectOfType<ChessGame>(); }
		if (moves == null) { Debug.LogWarning($"missing value for {nameof(moves)}"); }
		if (captures == null) { Debug.LogWarning($"missing value for {nameof(captures)}"); }
		if (selection == null) { Debug.LogWarning($"missing value for {nameof(selection)}"); }
		if (defendArrows == null) { Debug.LogWarning($"missing value for {nameof(defendArrows)}"); }
		if (tempDefendArrows == null) { Debug.LogWarning($"missing value for {nameof(tempDefendArrows)}"); }
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
				if (selected != null && selected is Piece selectedPiece) {
					Debug.Log($"...unselected {selected}");
					// move selected piece if the move is valid
					int moveIndex = (currentMoves != null) ? currentMoves.FindIndex(m => m.to == coord) : -1;
					if (coord != selectedPiece.GetCoord() && moveIndex >= 0) {
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
					defendArrows.ClearTiles();
				}
			}
			ClearPreviousSelectionVisuals();
			selected = currentHovered;
			if (selected != null) {
				Debug.Log($"selected {selected}");
			}
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
		defendArrows.ClearTiles();
		selection.ClearTiles();
		captures.ClearTiles();
		moves.ClearTiles();
	}

	private void ResetPieceSelectionVisuals() {
		if (selected == null) { return; }
		Piece piece = selected as Piece;
		if (piece == null) { return; }
		List<Move> moveLocations = new List<Move>();
		piece.GetMoves(moveLocations);
		if (currentMoves == null) { currentMoves = new List<Move>(); }
		else { currentMoves.Clear(); }
		currentMoves.AddRange(moveLocations);
		moves.ClearTiles();
		captures.ClearTiles();
		selection.ClearTiles();
		defendArrows.ClearTiles();
		for (int i = 0; i < currentMoves.Count; ++i) {
			AddPieceSelectionVisualFor(currentMoves[i], piece.board);
		}
		Coord pieceCoord = piece.GetCoord();
		selection.CreateMarks(new Move[] { new Move(piece,pieceCoord, pieceCoord) }, piece.board, Color.green);
	}

	private TiledGameObject AddPieceSelectionVisualFor(Move someKindOfMove, Board board) {
		TiledGameObject tgo = null;
		switch (someKindOfMove) {
			case Pawn.EnPassant ep:
				Debug.Log("EN PASSANT!");
				tgo = moves.AddMark(ep, board);
				tgo.Color = new Color(1, .5f, 0);
				{
					tgo = defendArrows.AddMark(ep, board);
					TiledWire tw = tgo as TiledWire;
					tw.Destination = ep.from;
					tgo.Color = new Color(1, .5f, 0);
				}
				break;
			case Capture cap:
				if (cap.IsDefend) {
					if (cap.pieceCaptured != null) {
						tgo = defendArrows.AddMark(cap, board);
						TiledWire tw = tgo as TiledWire;
						tw.Destination = cap.from;
						tgo.Color = new Color(1,1,0);
					}
				} else {
					tgo = captures.AddMark(cap, board);
					tgo.Color = new Color(1, 0, 0);
					//TiledWire tw = tgo as TiledWire;
					//tw.Destination = cap.from;
					//tw.Color = Color.red;
				}
				break;
			case Pawn.DoublePawnMove dbp:
				tgo = moves.AddMark(dbp, board);
				tgo.Color = new Color(1,.75f,0);
				break;
			case Move move:
				tgo = moves.AddMark(move, board);
				tgo.Color = Color.yellow;
				break;
		}
		return tgo;
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
		if (tempDefendArrows == null) { return; }
		tempDefendArrows.ClearTiles();
		if (target == null) { return; }
		Board board = target.GetBoard();
		if (board == null) {
			Debug.Log("no board?");
			return;
		}
		Coord currentCoord = target.GetCoord();
		List<Move> activityAtSquare = board.GetMovesTo(currentCoord);
		List<Move> defenders = new List<Move>();
		//Piece selectedPiece = selected as Piece;
		Coord selectedCoord = (selected != null) ? selected.GetCoord() : Coord.zero;
		for (int i = 0; i < activityAtSquare.Count; i++) {
			Capture cap = activityAtSquare[i] as Capture;
			if (cap == null) { continue; }
			defenders.Add(cap);
		}
		Debug.Log($" {target} {activityAtSquare.Count} {defenders.Count}");
		for (int i = 0; i < defenders.Count; ++i) {
			TiledGameObject tiledObject = tempDefendArrows.AddMark(defenders[i], board);
			TiledWire tw = tiledObject as TiledWire;
			if (tw != null) {
				tw.Destination = defenders[i].from;
				tiledObject.Color = Color.magenta;
			}
		}
		//List<TiledGameObject> arrows = tempDefendArrows.CreateMarks(defenders, board, tile => {
		//	TiledWire tiledArrow = tile as TiledWire;
		//	if (tiledArrow == null) { return; }
		//	tiledArrow.Destination = currentCoord;
		//	tiledArrow.Color = Color.yellow;
		//	if (selectedPiece != null) {
		//		Piece defender = board.GetPiece(tiledArrow.GetCoord());
		//		if (selectedPiece == null || defender.team != selectedPiece.team) {
		//			tiledArrow.Color = Color.red;
		//		}
		//	}
		//});
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

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
	public TileVisualization moveArrows;
	public TileVisualization defendArrows;

	private KeyCode undoMove = KeyCode.Backspace;
	private KeyCode redoMove = KeyCode.Space;

	void Start() {
		if (cam == null) { cam = Camera.main; }
		if (game == null) { game = FindObjectOfType<ChessGame>(); }
		if (moves == null) { Debug.LogWarning($"missing value for {nameof(moves)}"); }
		if (captures == null) { Debug.LogWarning($"missing value for {nameof(captures)}"); }
		if (selection == null) { Debug.LogWarning($"missing value for {nameof(selection)}"); }
		if (moveArrows == null) { Debug.LogWarning($"missing value for {nameof(moveArrows)}"); }
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
			//DrawSquareDefenders(currentHovered);
			// TODO if currentHovered is one of the valid moves
				// draw threat&defense lines for this piece if it were to move to the currentHovered location
		} else {
			ClearHoverAccent();
			//DrawSquareDefenders(null);
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
		//List<Move> captureLocations = new List<Move>();
		//List<Move> defendLocations = new List<Move>();
		piece.GetMoves(moveLocations, MoveKind.Move);
		//piece.GetMoves(captureLocations, MoveKind.Attack);
		//piece.GetMoves(defendLocations, MoveKind.Defend);
		if (currentMoves == null) { currentMoves = new List<Move>(); }
		else { currentMoves.Clear(); }
		currentMoves.AddRange(moveLocations);
		//currentMoves.AddRange(captureLocations);
		moves.ClearTiles();
		captures.ClearTiles();
		selection.ClearTiles();
		defendArrows.ClearTiles();
		for (int i = 0; i < currentMoves.Count; ++i) {
			AddPieceSelectionVisualFor(currentMoves[i], piece.board);
		}
//		moves.CreateMarks(moveLocations, piece.board, Color.yellow);
//		moves.CreateMarks(captureLocations, piece.board, Color.red);
		Coord pieceCoord = piece.GetCoord();
		selection.CreateMarks(new Move[] { new Move(piece,pieceCoord, pieceCoord) }, piece.board, Color.green);
		return;
		//for(int i = defendLocations.Count-1; i >= 0; --i) {
		//	Capture cap = defendLocations[i] as Capture;
		//	if (cap == null) { continue; }
		//	Piece defended = cap.pieceCaptured;//piece.board.GetPiece(defendLocations[i]);
		//	if (defended == null || !piece.team.IsAlliedWith(defended.team)) {
		//		defendLocations.RemoveAt(i);
		//	}
		//}
		//Coord c = piece.GetCoord();
		//defendArrows.CreateMarks(defendLocations, piece.board, tile => {
		//	if (tile is TiledWire tiledArrow) {
		//		tiledArrow.Destination = c;
		//		tiledArrow.Material.color = Color.yellow;
		//	}
		//});
	}

	private TiledGameObject AddPieceSelectionVisualFor(Move someKindOfMove, Board board) {
		TiledGameObject tgo = null;
		switch (someKindOfMove) {
			case Pawn.EnPassant ep:
				Debug.Log("EN PASSANT!");
				tgo = captures.AddMark(ep, board);
				tgo.Color = new Color(1,.25f,0);
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
		if (moveArrows == null) { return; }
		moveArrows.ClearTiles();
		if (target == null) { return; }
		Board board = target.GetBoard();
		Coord currentCoord = target.GetCoord();
		if (board == null) { return; }
		//List<Piece> pieces = board.GetPiecesThatCanDefend(currentCoord);
		List<Move> activityAtSquare = board.GetMovesTo(currentCoord);
		List<Move> defenders = new List<Move>();
		Piece selectedPiece = selected as Piece;
		Coord selectedCoord = (selected != null) ? selected.GetCoord() : Coord.zero;
		//for (int i = 0; i < pieces.Count; ++i) {
		//	Coord defenderCoord = pieces[i].GetCoord();
		//	if (selected != null && defenderCoord == selectedCoord) { continue; }
		//	defenders.Add(new Capture(pieces[i], pieces[i].GetCoord(), currentCoord, selectedPiece, defenderCoord));
		//}
		for (int i = 0; i < activityAtSquare.Count; i++) {
			Capture cap = activityAtSquare[i] as Capture;
			if (cap == null) { continue; }
			defenders.Add(cap);
		}
		moveArrows.ClearTiles();
		List<TiledGameObject> arrows = moveArrows.CreateMarks(defenders, board, tile => {
			TiledWire tiledArrow = tile as TiledWire;
			if (tiledArrow == null) { return; }
			tiledArrow.Destination = currentCoord;
			tiledArrow.Color = Color.yellow;
			if (selectedPiece != null) {
				Piece defender = board.GetPiece(tiledArrow.GetCoord());
				if (selectedPiece == null || defender.team != selectedPiece.team) {
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

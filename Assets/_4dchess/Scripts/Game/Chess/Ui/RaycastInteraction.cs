using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RaycastInteraction : MonoBehaviour {
	public ChessGame game;
	public ChessAnalysis analysis;
	public ChessVisuals visuals;
	[SerializeField] private Camera _cam;
	public Transform rayHitMarker;
	public Gradient hoveredColor;
	public TiledGameObject currentHovered;
	public MoveCalculator moveCalculator;

	public GameState CurrentGameState => game.chessMoves.CurrentMoveNode.BoardState;

	public Camera RaycastCamera {
		get => _cam;
		set => _cam = value;
	}

	void Start() {
		if (_cam == null) { _cam = Camera.main; }
		if (game == null) { game = FindObjectOfType<ChessGame>(); }
		if (analysis == null) { analysis = FindObjectOfType<ChessAnalysis>(); }
		if (visuals == null) { visuals = FindObjectOfType<ChessVisuals>(); }
	}

	void Update() {
		if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != null) {
			return;
		}
		GameState state = CurrentGameState;
		if (Input.GetMouseButtonUp(0)) {
			Click(state);
		}
		Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out RaycastHit rh)) {
			HoverOverBoard(rh, state);
		} else {
			HoverOffOfBoard(state);
		}
	}

	private void Click(GameState state) {
		if (currentHovered == null) { return; }
		// handle a click at the hovered coordinate
		Coord coord = currentHovered.GetCoord();
		Piece selectedPiece = analysis.SelectedPiece;
		if (selectedPiece != null) {
			if (game.RespectTurnOrder && selectedPiece.team != game.TeamWhoseTurnItIs) {
				string errorMessage = $"not {selectedPiece.team}'s turn";
				game.message.Text = errorMessage;
			} else {
				IGameMoveBase moveSelected = GetMoveAt(selectedPiece, coord);
				if (moveSelected != null) {
					MoveNode moveToMake = moveCalculator.GetMoveNode(game.NextMoveIndex, moveSelected);
					if (moveToMake.OwnKingInCheck) {
						Debug.Log("will not put self in check.");
					} else {
						game.chessMoves.DoThis(moveToMake);
						visuals.GenerateHints(moveToMake);
						//game.chessMoves.MakeMove(moveSelected);
					}
				}
				//DoMoveAt(selectedPiece, coord);
			}
			currentHovered = null;
		}
		RefreshVisuals(state, currentHovered);
	}

	public void RefreshVisuals(GameState state, TiledGameObject tiledGameObjet) {
		visuals.ClearPreviousSelectionVisuals();
		visuals.selected = tiledGameObjet;
		Board currentPiecesBoard = currentHovered != null ? currentHovered.GetBoard() : null;
		Piece selectedPiece = currentPiecesBoard != null ?//currentPiecesBoard.GetPiece(currentHovered.GetCoord())
			state.GetPieceAt(currentHovered.GetCoord()) : null;
		//Debug.Log("selecting " + selectedPiece);
		analysis.SetCurrentPiece(state, selectedPiece);
		visuals.ResetPieceSelectionVisuals(analysis);
	}

	private void HoverOverBoard(RaycastHit rh, GameState state) {
		if (Input.GetMouseButton(0)) {
			PlaceRayHitMarker(rh);
		}
		ColorAccentHovered(rh.collider.GetComponent<TiledGameObject>());
		visuals.DrawSquareDefenders(state, currentHovered);
		if (analysis.SelectedPiece != null) {
			MoveNode moveNode = moveCalculator.GetMoveNode(game.NextMoveIndex,
				GetMoveAt(analysis.SelectedPiece, currentHovered.GetCoord()));
			if (moveNode != null) {
				visuals.GenerateHints(moveNode);
			}
		}
	}

	private void HoverOffOfBoard(GameState state) {
		ClearHoverAccent();
		visuals.DrawSquareDefenders(state, null);
	}

	public IGameMoveBase GetMoveAt(Piece selectedPiece, Coord coord) {
		List<IGameMoveBase> moves = analysis.GetMovesAt(coord, MoveIsValid);
		if (coord != selectedPiece.GetCoord() && moves.Count != 0) {
			switch (moves.Count) {
				case 1: return moves[0];
				default:
					Debug.Log($"TODO must disambiguate between {moves.Count} moves: [{string.Join(", ", moves.ConvertAll(m => m.ToString() + "{" + m.GetType().Name + "}"))}]");
					for (int i = 0; i < moves.Count; i++) {
						BasicMove m = moves[i] as BasicMove;
						Debug.Log($"{m.GetType().Name} {m.pieceMoved} {m.from} {m.to}");
					}
					return null;
			}
		}
		return null;
	}

	private bool MoveIsValid(IGameMoveBase move) {
		return move.IsValid;
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
		Vector3 right = _cam.transform.right;
		Vector3 forward = Vector3.Cross(up, right); ;
		rayHitMarker.transform.rotation = Quaternion.LookRotation(forward, up);
	}
}

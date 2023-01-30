using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessVisuals : MonoBehaviour {
	public ChessAnalysis analysis;
	public TileVisualization moves;
	public TileVisualization captures;
	public TileVisualization selection;
	public TileVisualization kingInCheck;
	public TileVisualization defendArrows;
	public TileVisualization tempDefendArrows;
	public TileVisualization specialTileAndArrows;
	public TiledGameObject selected;
	public bool showKingDefender;
	public bool showKingInCheck;
	private Dictionary<System.Type, TileVisualSpecifics> _tileVisualizationSettings = null;
	public Color threaten = new Color(1, .5f, 1);
	public Color activeAttack = new Color(1, 0.75f, .5f);

	private struct TileVisualSpecifics {
		public Color color;
		public TileVisualization visualizer;
		public TileVisualSpecifics(Color color, TileVisualization visualizer) {
			this.color = color;
			this.visualizer = visualizer;
		}
	}

	Dictionary<System.Type, TileVisualSpecifics> TileVisSettings {
		get => (_tileVisualizationSettings != null) ? _tileVisualizationSettings :
		_tileVisualizationSettings = new Dictionary<System.Type, TileVisualSpecifics>() {
			[typeof(Pawn.EnPassant)] = new TileVisualSpecifics(new Color(1, .5f, 0), specialTileAndArrows),
			//[typeof(Defend)] = new TileVisualSpecifics(new Color(1, 1, 0), defendArrows),
			[typeof(PieceMoveAttack)] = new TileVisualSpecifics(new Color(1, 0, 0), captures),
			[typeof(Pawn.DoubleMove)] = new TileVisualSpecifics(new Color(1, .75f, 0), moves),
			[typeof(King.Castle)] = new TileVisualSpecifics(new Color(1, .5f, 0), specialTileAndArrows),
			[typeof(PieceMove)] = new TileVisualSpecifics(new Color(1, 1, 0), moves),
			[typeof(King.Check)] = new TileVisualSpecifics(new Color(1, 0, 1), kingInCheck),
		};
	}

	private void Start() {
		if (moves == null) { Debug.LogWarning($"missing value for {nameof(moves)}"); }
		if (captures == null) { Debug.LogWarning($"missing value for {nameof(captures)}"); }
		if (selection == null) { Debug.LogWarning($"missing value for {nameof(selection)}"); }
		if (kingInCheck == null) { Debug.LogWarning($"missing value for {nameof(kingInCheck)}"); }
		if (defendArrows == null) { Debug.LogWarning($"missing value for {nameof(defendArrows)}"); }
		if (tempDefendArrows == null) { Debug.LogWarning($"missing value for {nameof(tempDefendArrows)}"); }
		if (specialTileAndArrows == null) { Debug.LogWarning($"missing value for {nameof(specialTileAndArrows)}"); }
	}

	public void ClearPreviousSelectionVisuals() {
		if (selected == null) { return; }
		selected.ResetColor();
		specialTileAndArrows.ClearTiles();
		defendArrows.ClearTiles();
		selection.ClearTiles();
		captures.ClearTiles();
		moves.ClearTiles();
	}

	public void ResetPieceSelectionVisuals(ChessAnalysis analysis) {
		if (selected == null) { return; }
		Piece piece = analysis.SelectedPiece;//selected as Piece;
		if (piece == null) { return; }
		moves.ClearTiles();
		captures.ClearTiles();
		selection.ClearTiles();
		defendArrows.ClearTiles();
		specialTileAndArrows.ClearTiles();
		Coord pieceCoord = piece.GetCoord();
		selection.CreateMarks(new BasicMove[] { new BasicMove(piece.board, piece, pieceCoord, pieceCoord) }, Color.cyan);

		if (analysis.CurrentPieceCurrentMoves != null) {
			for (int i = 0; i < analysis.CurrentPieceCurrentMoves.Count; ++i) {
				IGameMoveBase move = analysis.CurrentPieceCurrentMoves[i];
				if (!showKingDefender && move is PieceMoveAttack cap && ChessGame.IsMyKing(piece, cap.pieceCaptured)) { continue; }
				AddPieceSelectionVisualFor(move, piece.board);
			}
		}
	}

	private TiledGameObject AddPieceSelectionVisualFor(IGameMoveBase someKindOfMove, Board board) {
		TiledGameObject tgo;
		if (!TileVisSettings.TryGetValue(someKindOfMove.GetType(), out TileVisualSpecifics setting)) {
			setting.visualizer = specialTileAndArrows;
			setting.color = Color.magenta;
			Debug.Log("unknown type "+ someKindOfMove.GetType());
		}
		tgo = setting.visualizer.AddMark(someKindOfMove);
		tgo.Color = setting.color;
		return tgo;
	}

	public void DrawSquareDefenders(TiledGameObject target) {
		if (tempDefendArrows == null) { return; }
		tempDefendArrows.ClearTiles();
		if (target == null) { return; }
		Board board = target.GetBoard();
		if (board == null) {
			// this happens when seeking the defender squares of captured pieces
			//Debug.Log("no board");
			return;
		}
		Coord currentCoord = target.GetCoord();
		IList<IGameMoveBase> activityAtSquare = board.GetMovesTo(currentCoord);
		if (activityAtSquare == null) { return; }
		List<BasicMove> defenders = new List<BasicMove>();
		Piece selectedPiece = selected as Piece;
		//Coord selectedCoord = (selected != null) ? selected.GetCoord() : Coord.zero;
		Piece piece = selectedPiece;
		if (piece == null) { piece = board.GetPiece(currentCoord); }
		for (int i = 0; i < activityAtSquare.Count; i++) {
			PieceMoveAttack cap = activityAtSquare[i] as PieceMoveAttack;
			if (cap == null) { continue; }
			if (!showKingDefender && ChessGame.IsMyKing(piece, cap.pieceCaptured)) { continue; }
			defenders.Add(cap);
		}
		Piece hoveredOver = selectedPiece;
		if (hoveredOver == null) {
			hoveredOver = board.GetPiece(currentCoord);
		}
		//Debug.Log($" {target} {activityAtSquare.Count} {defenders.Count}");
		for (int i = 0; i < defenders.Count; ++i) {
			TiledGameObject tiledObject = tempDefendArrows.AddMark(defenders[i], true);
			if (hoveredOver != null && tiledObject != null) {
				if (!defenders[i].pieceMoved.team.IsAlliedWith(hoveredOver.team)) {
					tiledObject.Color = threaten;
				} else if (defenders[i].pieceMoved == selected && board.GetPiece(currentCoord) != null) {
					tiledObject.Color = activeAttack;
				}
			}
		}
	}
}

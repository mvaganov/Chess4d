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
			[typeof(Defend)] = new TileVisualSpecifics(new Color(1, 1, 0), defendArrows),
			[typeof(Capture)] = new TileVisualSpecifics(new Color(1, 0, 0), captures),
			[typeof(Pawn.DoubleMove)] = new TileVisualSpecifics(new Color(1, .75f, 0), moves),
			[typeof(King.Castle)] = new TileVisualSpecifics(new Color(1, .5f, 0), specialTileAndArrows),
			[typeof(Move)] = new TileVisualSpecifics(new Color(1, 1, 0), moves),
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
		selection.CreateMarks(new Move[] { new Move(piece, pieceCoord, pieceCoord) }, Color.green);

		if (analysis.CurrentMoves != null) {
			for (int i = 0; i < analysis.CurrentMoves.Count; ++i) {
				Move move = analysis.CurrentMoves[i];
				if (!showKingDefender && move is Capture cap && ChessGame.IsMyKing(piece, cap.pieceCaptured)) { continue; }
				AddPieceSelectionVisualFor(move, piece.board);
			}
		}
	}

	private TiledGameObject AddPieceSelectionVisualFor(Move someKindOfMove, Board board) {
		TiledGameObject tgo;
		if (!TileVisSettings.TryGetValue(someKindOfMove.GetType(), out TileVisualSpecifics setting)) {
			setting.visualizer = specialTileAndArrows;
			setting.color = Color.magenta;
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
		List<Move> activityAtSquare = board.GetMovesTo(currentCoord);
		List<Move> defenders = new List<Move>();
		Piece selectedPiece = selected as Piece;
		//Coord selectedCoord = (selected != null) ? selected.GetCoord() : Coord.zero;
		Piece piece = selectedPiece;
		if (piece == null) { piece = board.GetPiece(currentCoord); }
		for (int i = 0; i < activityAtSquare.Count; i++) {
			Capture cap = activityAtSquare[i] as Capture;
			if (cap == null) { continue; }
			if (!showKingDefender && ChessGame.IsMyKing(piece, cap.pieceCaptured)) { continue; }
			defenders.Add(cap);
		}
		//Debug.Log($" {target} {activityAtSquare.Count} {defenders.Count}");
		for (int i = 0; i < defenders.Count; ++i) {
			TiledGameObject tiledObject = tempDefendArrows.AddMark(defenders[i], true);
			tiledObject.Color = Color.magenta;
		}
	}
}

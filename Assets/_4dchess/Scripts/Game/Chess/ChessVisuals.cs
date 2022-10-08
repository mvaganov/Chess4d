using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessVisuals : MonoBehaviour {
	public ChessAnalysis analysis;
	public TileVisualization moves;
	public TileVisualization captures;
	public TileVisualization selection;
	public TileVisualization defendArrows;
	public TileVisualization tempDefendArrows;
	public TileVisualization specialTileAndArrows;
	public TiledGameObject selected;
	public bool showKingDefender;

	private void Start() {
		if (moves == null) { Debug.LogWarning($"missing value for {nameof(moves)}"); }
		if (captures == null) { Debug.LogWarning($"missing value for {nameof(captures)}"); }
		if (selection == null) { Debug.LogWarning($"missing value for {nameof(selection)}"); }
		if (defendArrows == null) { Debug.LogWarning($"missing value for {nameof(defendArrows)}"); }
		if (tempDefendArrows == null) { Debug.LogWarning($"missing value for {nameof(tempDefendArrows)}"); }
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
		TiledGameObject tgo = null;
		switch (someKindOfMove) {
			case Pawn.EnPassant ep:
				//Debug.Log("EN PASSANT!");
				tgo = specialTileAndArrows.AddMark(ep);
				tgo.Color = new Color(1, .5f, 0);
				break;
			case Defend def:
				tgo = defendArrows.AddMark(def);
				tgo.Color = new Color(1, 1, 0);
				break;
			case Capture cap:
				//if (cap.IsDefend) {
				//	if (cap.pieceCaptured != null) {
				//		tgo = defendArrows.AddMark(cap);
				//		tgo.Color = new Color(1, 1, 0);
				//	}
				//} else {
					tgo = captures.AddMark(cap);
					tgo.Color = new Color(1, 0, 0);
				//}
				break;
			case Pawn.DoubleMove dbp:
				tgo = moves.AddMark(dbp);
				tgo.Color = new Color(1, .75f, 0);
				break;
			case King.Castle cas:
				tgo = specialTileAndArrows.AddMark(cas);
				tgo.Color = new Color(1, .75f, 0);
				break;
			case Move move:
				tgo = moves.AddMark(move);
				tgo.Color = Color.yellow;
				break;
		}
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
			TiledGameObject tiledObject = tempDefendArrows.AddMarkReverse(defenders[i]);
			tiledObject.Color = Color.magenta;
			//TiledWire tw = tiledObject as TiledWire;
			//if (tw != null) {
			//	tw.Destination = defenders[i].to;
			//	tiledObject.Color = Color.magenta;
			//}
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
}

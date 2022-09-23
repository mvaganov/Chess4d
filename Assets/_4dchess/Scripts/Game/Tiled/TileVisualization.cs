using System.Collections.Generic;
using UnityEngine;

public class TileVisualization : MonoBehaviour {
	[SerializeField] private MemoryPool<TiledGameObject> markPool = new MemoryPool<TiledGameObject>();
	[SerializeField] private List<TiledGameObject> currentMarks = new List<TiledGameObject>();

	public void ClearTiles() {
		for(int i = currentMarks.Count-1; i >= 0; --i) {
			TiledGameObject marker = currentMarks[i];
			markPool.Reclaim(marker);
			currentMarks.RemoveAt(i);
		}
	}
	public List<TiledGameObject> CreateMarks(IEnumerable<Move> moves, Board board, Color c) {
		int markToColor = currentMarks.Count;
		CreateMarks(moves, board);
		for (int i = markToColor; i < currentMarks.Count; ++i) {
			TiledGameObject move = currentMarks[i];
			move.Material.color = c;
		}
		return currentMarks;
	}
	public List<TiledGameObject> CreateMarks(IEnumerable<Move> moves, Board board, 
	System.Action<TiledGameObject> markProcessing = null) {
		//ClearTiles();
		if (moves == null) { return currentMarks; }
		TiledGameObject marker = null;
		foreach (Move move in moves) {
			if (AddMark(move, board)) {
				markProcessing?.Invoke(marker);
			}
		}
		return currentMarks;
	}
	public TiledGameObject AddMark(Move move, Board board) {
		TiledGameObject marker = CreateMark(move, board, false);
		if (marker == null) { return null; }
		currentMarks.Add(marker);
		return marker;
	}
	public TiledGameObject CreateMark(Move move, Board board, bool reverse) {
		TiledGameObject marker = null;
		Tile tile;
		Transform markerTransform;
		Coord coord;
		switch (move) {
			// TODO for En Passant, show move and capture line branching out of same source? or looping back?
			// for castling show king arrow hopping and rook arrow sliding
			// for promotion show fancy up-arrow icon at the end? maybe an upward branch? maybe a question mark box?
			case Capture cap:
				marker = markPool.Get();
				coord = reverse ? cap.from : cap.fromCaptured;
				tile = board.GetTile(coord);
				markerTransform = marker.transform;
				markerTransform.SetParent(tile.transform);
				markerTransform.localPosition = Vector3.zero;
				Piece capturable = cap.pieceCaptured;
				if (marker.Label != null) {
					if (capturable != null) {
						marker.Label.text = $"capture";//\n[{capturable.code}]";
					} else {
						marker.Label.text = "defend";
					}
				}

				break;
			case Move mov:
				marker = markPool.Get();
				coord = reverse ? mov.from : mov.to;
				tile = board.GetTile(coord);
				markerTransform = marker.transform;
				markerTransform.SetParent(tile.transform);
				markerTransform.localPosition = Vector3.zero;
				break;
		}
		if (marker is TiledWire tw) {
			coord = reverse ? move.to : move.from;
			tw.Destination = coord;
		}
		return marker;
	}
	public TiledGameObject AddMarkReverse(Move move, Board board) {
		TiledGameObject marker = CreateMark(move, board, true);
		if (marker == null) { return null; }
		currentMarks.Add(marker);
		return marker;
	}
}

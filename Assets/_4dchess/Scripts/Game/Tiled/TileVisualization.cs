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
		TiledGameObject marker;
		Tile tile;
		Transform markerTransform;
		foreach (Move move in moves) {
			switch (move) {
				// TODO for En Passant, show move and capture line branching out of same source? or looping back?
				// for castling show king arrow hopping and rook arrow sliding
				// for promotion show fancy up-arrow icon at the end? maybe an upward branch? maybe a question mark box?
				case Capture cap:
					marker = markPool.Get();
					currentMarks.Add(marker);
					tile = board.GetTile(cap.fromCaptured);
					markerTransform = marker.transform;
					markerTransform.SetParent(tile.transform);
					markerTransform.localPosition = Vector3.zero;
					markProcessing?.Invoke(marker);
					break;
				case Move mov:
					marker = markPool.Get();
					currentMarks.Add(marker);
					tile = board.GetTile(mov.to);
					markerTransform = marker.transform;
					markerTransform.SetParent(tile.transform);
					markerTransform.localPosition = Vector3.zero;
					markProcessing?.Invoke(marker);
					break;
			}
		}
		return currentMarks;
	}
}

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
	public List<TiledGameObject> CreateMarks(IEnumerable<Coord> coords, Board board, Color c) {
		int markToColor = currentMarks.Count;
		CreateMarks(coords, board);
		for (int i = markToColor; i < currentMarks.Count; ++i) {
			TiledGameObject move = currentMarks[i];
			move.Material.color = c;
		}
		return currentMarks;
	}
	public List<TiledGameObject> CreateMarks(IEnumerable<Coord> coords, Board board, 
	System.Action<TiledGameObject> markProcessing = null) {
		//ClearTiles();
		if (coords == null) { return currentMarks; }
		foreach (Coord coord in coords) {
			TiledGameObject marker = markPool.Get();
			currentMarks.Add(marker);
			Tile tile = board.GetTile(coord);
			Transform t = marker.transform;
			t.SetParent(tile.transform);
			t.localPosition = Vector3.zero;
			markProcessing?.Invoke(marker);
		}
		return currentMarks;
	}
}

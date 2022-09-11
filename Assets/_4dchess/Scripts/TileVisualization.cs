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
	public void MarkTiles(IEnumerable<Coord> coords, Board board, Color c) {
		MarkTiles(coords, board);
		foreach(TiledGameObject move in currentMarks) {
			move.Material.color = c;
		}
	}
	public void MarkTiles(IEnumerable<Coord> coords, Board board) {
		ClearTiles();
		if (coords == null) { return; }
		foreach (Coord coord in coords) {
			TiledGameObject marker = markPool.Get();
			currentMarks.Add(marker);
			Tile tile = board.GetTile(coord);
			Transform t = marker.transform;
			t.SetParent(tile.transform);
			t.localPosition = Vector3.zero;
		}
	}
}

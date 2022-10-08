using System;
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

	public List<TiledGameObject> CreateMarks(IEnumerable<Move> moves, Color c) {
		int markToColor = currentMarks.Count;
		CreateMarks(moves);
		for (int i = markToColor; i < currentMarks.Count; ++i) {
			TiledGameObject move = currentMarks[i];
			move.Material.color = c;
		}
		return currentMarks;
	}

	public List<TiledGameObject> CreateMarks(IEnumerable<Move> moves, Action<TiledGameObject> markProcessing = null) {
		//ClearTiles();
		if (moves == null) { return currentMarks; }
		TiledGameObject marker = null;
		foreach (Move move in moves) {
			if (AddMark(move)) {
				markProcessing?.Invoke(marker);
			}
		}
		return currentMarks;
	}

	public TiledGameObject AddMark(Move move, bool reverse = false) {
		TiledGameObject marker = move.MakeMark(markPool, reverse);
		if (marker == null) { return null; }
		currentMarks.Add(marker);
		return marker;
	}
}

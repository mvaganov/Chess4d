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
	public TiledGameObject AddMark(Move move) {
		TiledGameObject marker = CreateMark(move, false);
		if (marker == null) { return null; }
		currentMarks.Add(marker);
		return marker;
	}
	public TiledGameObject AddMarkReverse(Move move) {
		TiledGameObject marker = CreateMark(move, true);
		if (marker == null) { return null; }
		currentMarks.Add(marker);
		return marker;
	}
	public TiledGameObject CreateMark(Move move, bool reverse) {
		TiledGameObject marker = null;
		//Tile tile;
		//Transform markerTransform;
		//Coord coord;
		switch (move) {
			// TODO for En Passant, show move and capture line branching out of same source? or looping back?
			// for castling show king arrow hopping and rook arrow sliding
			// for promotion show fancy up-arrow icon at the end? maybe an upward branch? maybe a question mark box?
			case Pawn.DoubleMove dm:
				marker = dm.MakeMark(markPool, reverse);
				break;
			case Pawn.EnPassant ep:
				marker = ep.MakeMark(markPool, reverse);
				//marker = markPool.Get();
				//coord = reverse ? ep.from : ep.to;
				//tile = board.GetTile(coord);
				//markerTransform = marker.transform;
				//markerTransform.SetParent(tile.transform);
				//markerTransform.localPosition = Vector3.zero;
				//if (marker.Label != null) {
				//	Piece capturable = ep.pieceCaptured;
				//	if (capturable != null) {
				//		marker.Label.text = $"en passant";//\n[{capturable.code}]";
				//	} else {
				//		marker.Label.text = "IMPOSSI BLE";
				//	}
				//}
				break;
			case Capture cap:
				marker = cap.MakeMark(markPool, reverse);
				//marker = markPool.Get();
				//coord = reverse ? cap.from : cap.fromCaptured;
				//tile = board.GetTile(coord);
				//markerTransform = marker.transform;
				//markerTransform.SetParent(tile.transform);
				//markerTransform.localPosition = Vector3.zero;
				//if (marker.Label != null) {
				//	Piece capturable = cap.pieceCaptured;
				//	if (capturable != null) {
				//		marker.Label.text = $"capture";//\n[{capturable.code}]";
				//	} else {
				//		marker.Label.text = "defend";
				//	}
				//}
				break;
			case King.Castle cas:
				marker = cas.MakeMark(markPool, reverse);
				break;
			default:
				if (move.GetType() == typeof(Move)) {
					//marker = markPool.Get();
					//coord = reverse ? mov.from : mov.to;
					//tile = board.GetTile(coord);
					//markerTransform = marker.transform;
					//markerTransform.SetParent(tile.transform);
					//markerTransform.localPosition = Vector3.zero;
					marker = move.MakeMark(markPool, reverse);
				} else {
					Debug.Log($"how to draw {move.GetType()}?");
				}
				break;
		}
		//if (marker is TiledWire tw) {
		//	tw.Destination = reverse ? move.to : move.from;
		//}
		return marker;
	}
}

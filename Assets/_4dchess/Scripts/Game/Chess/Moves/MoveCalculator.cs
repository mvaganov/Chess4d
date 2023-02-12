using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCalculator : MonoBehaviour {
	HashSet<MoveNode> allCalculatedMoves = new HashSet<MoveNode>();

	public MoveNode GetMoveNode(int turn, IGameMoveBase move) {
		MoveNode node = new MoveNode(turn, move, null);
		if (allCalculatedMoves.TryGetValue(node, out MoveNode found)) { return found; }
		allCalculatedMoves.Add(node);
		Debug.Log($"{allCalculatedMoves.Count} cached moves");
		return node;
	}
}

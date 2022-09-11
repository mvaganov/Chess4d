using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Game : MonoBehaviour {
	[System.Serializable]
	public struct PieceCode {
		public string name;
		public string code;
		public Piece prefab;
		public PieceCode(string name, string code, Piece prefab) {
			this.name = name;
			this.code = code;
			this.prefab = prefab;
		}
		public PieceCode(string name, string code) : this(name, code, null) { }
	}
	public PieceCode[] pieceCodes = new PieceCode[] {
		new PieceCode("pawn", "p"),
		new PieceCode("knight", "n"),
		new PieceCode("bishop", "b"),
		new PieceCode("rook", "r"),
		new PieceCode("queen", "q"),
		new PieceCode("king", "k"),
	};
	private Dictionary<string, Piece> _prefabByCode = null;

	[ContextMenuItem(nameof(Generate),nameof(Generate))]
	public Board board;

	public List<Team> teams = new List<Team>();

	public void Generate() {
		board.Generate();
		for(int i = 0; i < teams.Count; ++i) {
			teams[i].Generate();
		}
	}

	void Start() {
		teams.ForEach(t => t.MovePiecesToTile());
	}


	public Piece GetPrefab(string code) {
		if (_prefabByCode == null) {
			_prefabByCode = new Dictionary<string, Piece>();
			for (int i = 0; i < pieceCodes.Length; ++i) {
				_prefabByCode[pieceCodes[i].code] = pieceCodes[i].prefab;
			}
		}
		if (_prefabByCode.TryGetValue(code, out Piece prefab)) {
			return prefab;
		}
		return null;
	}

	public static GameObject CreateObject(GameObject go) {
		if (go == null) { return null; }
#if UNITY_EDITOR
		if (!Application.isPlaying) {
			return (GameObject)PrefabUtility.InstantiatePrefab(go);
		}
#endif
		return Instantiate(go);
	}

	public static void DestroyObject(GameObject go) {
		if (go == null) { return; }
#if UNITY_EDITOR
		if (!Application.isPlaying) {
			DestroyImmediate(go);
			return;
		}
#endif
		Destroy(go);
	}

	public static void DestroyListOfThingsBackwards<T>(List<T> things) where T : MonoBehaviour {
		for (int i = things.Count - 1; i >= 0; --i) {
			T thing = things[i];
			if (thing != null) { DestroyObject(thing.gameObject); }
			things.RemoveAt(i);
		}
		things.Clear();
	}
}

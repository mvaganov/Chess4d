using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChessGame : MonoBehaviour {
	[System.Serializable]
	public struct PieceCode {
		public string name;
		public string code;
		public Piece prefab;
		public Sprite[] icons;
		public PieceCode(string name, string code, Piece prefab) {
			this.name = name;
			this.code = code;
			this.prefab = prefab;
			icons =	new Sprite[2];
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
	public string PawnPromotionOptions = "nbrq";
	private Dictionary<string, Piece> _prefabByCode = null;

	[ContextMenuItem(nameof(Generate),nameof(Generate))]
	public Board board;
	public Moves chessMoves;
	public List<Team> teams = new List<Team>();
	public void OnValidate() {
	}
	public void Generate() {
		board.Generate();
		for(int i = 0; i < teams.Count; ++i) {
			teams[i].Generate();
		}
		EditorTool.MarkSceneDirty();
	}

	void Start() {
		teams.ForEach(t => t.MovePiecesToTile());
		if (chessMoves == null) { chessMoves = FindObjectOfType<Moves>(); }
		// TODO implement icon that hovers over piece's head, always orients to ortho camera rotation, and is only visible by ortho camera
	}

	public void Move(Piece p, Coord position, string notes) {
		chessMoves.MakeMove(p, p.GetCoord(), position, notes);
	}

	public void Capture(Piece moved, Piece captured, Coord movePosition, string notes) {
		chessMoves.MakeCapture(moved, moved.GetCoord(), movePosition, captured, captured.GetCoord(), notes);
	}

	public void UndoMove() {
		chessMoves.UndoMove();
	}

	public void RedoMove() {
		chessMoves.RedoMove(0);
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

	public static bool IsMoveCapture(Piece piece, Coord move, out Piece capturedPiece) {
		capturedPiece = piece.board.GetPiece(move);
		return capturedPiece != null && capturedPiece.team != piece.team;
	}
	public static bool IsMyKing(Piece me, Piece other) {
		if (me == null || other == null || other.code != "K" || other.team != me.team) { return false; }
		return true;
	}
}

using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChessGame : MonoBehaviour {
	[System.Serializable]
	public class PieceCode {
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
		new PieceCode("pawn", ""),
		new PieceCode("knight", "N"),
		new PieceCode("bishop", "B"),
		new PieceCode("rook", "R"),
		new PieceCode("queen", "Q"),
		new PieceCode("king", "K"),
	};
	public string PawnPromotionOptions = "NBRQ";
	private Dictionary<string, PieceCode> _prefabByCode = null;

	[ContextMenuItem(nameof(Generate),nameof(Generate))]
	public Board board;
	public MoveHistory chessMoves;
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
		if (chessMoves == null) { chessMoves = FindObjectOfType<MoveHistory>(); }
		// TODO implement icon that hovers over piece's head, always orients to ortho camera rotation, and is only visible by ortho camera
	}

	public Piece CreatePiece(Team team, string code, Coord coord, Board board) {
		Piece prefab = GetPrefab(code);
		if (prefab == null) { return null; }
		GameObject pieceObject = CreateObject(prefab.gameObject);
		Piece piece = pieceObject.GetComponent<Piece>();
		//Pieces.Add(piece);
		string name = team.name + " " + prefab.name;// + " " + Pieces.Count;
		pieceObject.name = name;
		piece.team = team;
		piece.board = board;
		piece.Material = team.material;
		piece.name = name;
		SetPiece(piece, board, coord);
		//Transform t = piece.transform;
		//t.SetParent(board.transform);
		//t.Rotate(team.PieceRotation);
		//t.position = board.CoordToWorldPosition(coord);
		//piece.SetTile(coord);
		return piece;
	}

	public void SetPiece(Piece piece, Board board, Coord coord) {
		Transform t = piece.transform;
		t.SetParent(board.transform);
		t.Rotate(piece.team.PieceRotation);
		t.position = board.CoordToWorldPosition(coord);
		piece.SetTile(coord);
	}

	public void UndoMove() {
		chessMoves.UndoMove();
	}

	public void RedoMove() {
		chessMoves.RedoMove(0);
	}

	public PieceCode GetPieceInfo(string code) {
		if (_prefabByCode == null) {
			_prefabByCode = new Dictionary<string, PieceCode>();
			for (int i = 0; i < pieceCodes.Length; ++i) {
				_prefabByCode[pieceCodes[i].code] = pieceCodes[i];
			}
		}
		if (_prefabByCode.TryGetValue(code, out PieceCode info)) { return info; }
		return null;
	}

	public Piece GetPrefab(string code) {
		PieceCode info = GetPieceInfo(code);
		return (info != null) ? info.prefab : null;
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

	public static void DestroyChessObject(GameObject go) {
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
			if (thing != null) { DestroyChessObject(thing.gameObject); }
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

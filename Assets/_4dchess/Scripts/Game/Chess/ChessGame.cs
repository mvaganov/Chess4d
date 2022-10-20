using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChessGame : MonoBehaviour {
	public PieceInfo[] pieceCodes = new PieceInfo[] {
		new PieceInfo("king", "K"),
		new PieceInfo("pawn", "P"),
		new PieceInfo("knight", "N"),
		new PieceInfo("bishop", "B"),
		new PieceInfo("rook", "R"),
		new PieceInfo("queen", "Q"),
	};
	public string PawnPromotionOptions = "NBRQ";
	private Dictionary<string, PieceInfo> _prefabByCode = null;

	[ContextMenuItem(nameof(Generate),nameof(Generate)),
	ContextMenuItem(nameof(GenerateFromXfen), nameof(GenerateFromXfen))]
	public Board board;
	public ChessAnalysis analysis;
	public MoveHistory chessMoves;
	public List<Team> teams = new List<Team>();

	public void OnValidate() {
	}

	public void GenerateFromXfen() {
		board.LoadXfen(teams);
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
		if (analysis == null) { analysis = FindObjectOfType<ChessAnalysis>(); }
		// TODO implement icon that hovers over piece's head, always orients to ortho camera rotation, and is only visible by ortho camera
	}

	public Piece GetPiece(Team team, string code, Coord coord, Board board, bool forceCreatePiece) {
		Piece piece = team.GetPiece(code, forceCreatePiece);
		Debug.Log("piece " + piece + "? board "+board+"? "+ forceCreatePiece);
		piece.transform.position = board.CoordToWorldPosition(coord);
		board.SetPiece(piece, coord);
		return piece;
	}

	public void UndoMove() {
		chessMoves.UndoMove();
	}

	public void RedoMove() {
		chessMoves.RedoMove();
	}

	public PieceInfo GetPieceInfo(string code) {
		if (_prefabByCode == null) {
			_prefabByCode = new Dictionary<string, PieceInfo>();
			for (int i = 0; i < pieceCodes.Length; ++i) {
				_prefabByCode[pieceCodes[i].code] = pieceCodes[i];
			}
		}
		if (_prefabByCode.TryGetValue(code, out PieceInfo info)) { return info; }
		return null;
	}

	public Piece GetPrefab(string code) {
		PieceInfo info = GetPieceInfo(code);
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

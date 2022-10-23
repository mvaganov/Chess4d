using NonStandard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : TiledGameObject {
	public Team team;
	public string code;
	public Board board;
	private MoveLogic moveLogic;
	public float jumpHeight = 0;
	[SerializeField] private List<Move> moves;
	private bool movesAreUpToDate;
	public int moveCount = 0;
	public SpriteRenderer worldIcon;

	public ChessGame Game => board.game;

	public MoveLogic MoveLogic => moveLogic;

	public void MarkMovesAsInvalid() {
		movesAreUpToDate = false;
	}

	protected override void Start() {
		base.Start();
		RefreshMoveLogic();
	}

	public void RefreshMoveLogic() {
		moveLogic = GetComponent<MoveLogic>();
	}

	// TODO can't move into check
	public virtual void DoMove(Move move) {
		if (moveLogic != null) {
			moveLogic.DoMove(move);
		} else {
			MoveInternal(move.to);
		}
		movesAreUpToDate = false;
		++moveCount;
	}

	public virtual void UndoMove(Move move) {
		if (moveLogic != null) {
			moveLogic.UndoMove(move);
		} else {
			MoveInternal(move.from);
		}
		movesAreUpToDate = false;
		--moveCount;
	}

	internal void MoveInternal(Coord coord) {
		SetTile(coord);
		if (jumpHeight == 0) {
			LerpToLocalCenter(Vector3.zero);
		} else {
			JumpToLocalCenter(Vector3.zero, jumpHeight);
		}
	}

	public void SetTile(Coord coord) {
		Tile tile = board.GetTile(coord);
		transform.SetParent(tile.transform, true);
	}

	public Tile GetTile() {
		return transform.GetComponentInParent<Tile>();
	}

	public virtual void LerpToLocalCenter() => LerpToLocalCenter(Vector3.zero);

	public virtual void LerpToLocalCenter(Vector3 offset) {
		Vector3[] path = new Vector3[2] { transform.localPosition, offset };
		MoveLogic.LerpPath(this, path, team.speed, true);
	}

	public void JumpToLocalCenter() {
		JumpToLocalCenter(Vector3.zero, team.jumpHeight);
	}

	public virtual void JumpToLocalCenter(Vector3 offset, float jumpHeight) {
		const int bezierPointCount = 24;
		Vector3[] bezier = new Vector3[bezierPointCount];
		Vector3 height = Vector3.up * jumpHeight;
		Vector3 start = transform.localPosition;
		Math3d.WriteBezier(bezier, start, start + height, offset + height, offset);
		MoveLogic.LerpPath(this, bezier, team.speed, true);
	}

	public void GetMoves(List<Move> out_moves, MoveKind moveKind = MoveKind.MoveAttackDefend) {
		if (moveLogic == null) { return; }
		if (movesAreUpToDate) {
			out_moves?.AddRange(moves);
			return;
		}
		if (moves != null) { GetMovesForceCalculation(out_moves, moveKind); }
	}

	public void GetMovesForceCalculation(List<Move> out_moves, MoveKind moveKind = MoveKind.MoveAttackDefend) {
		if (moves == null) { moves = new List<Move>(); } else { moves.Clear(); }
		moveLogic.GetMoves(moves, moveKind);
		movesAreUpToDate = true;
		out_moves?.AddRange(moves);
	}

	public override int GetHashCode() {
		return code.GetHashCode() ^ team.name.GetHashCode();
	}
}

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
	[SerializeField] private List<Coord> moves;
	[SerializeField] private List<Coord> captures;
	[SerializeField] private List<Coord> defends;
	private bool moveAreUpToDate;
	public int moveCount = 0;

	public void MarkMovesAsInvalid() {
		moveAreUpToDate = false;
	}

	protected override void Start() {
		base.Start();
		moveLogic = GetComponent<MoveLogic>();
	}

	// TODO can't move into check
	public virtual void DoMove(Coord coord) {
		if (moveLogic != null) {
			moveLogic.DoMove(coord);
		} else {
			MoveInternal(coord);
		}
		moveAreUpToDate = false;
		++moveCount;
	}

	public virtual void UndoMove(Coord coord) {
		if (moveLogic != null) {
			moveLogic.UndoMove(coord);
		} else {
			MoveInternal(coord);
		}
		moveAreUpToDate = false;
		--moveCount;
	}

	internal void MoveInternal(Coord coord) {
		SetTile(coord);
		if (jumpHeight == 0) {
			MoveToLocalCenter(Vector3.zero);
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

	public virtual void MoveToLocalCenter() => MoveToLocalCenter(Vector3.zero);

	public virtual void MoveToLocalCenter(Vector3 offset) {
		Vector3[] path = new Vector3[2] { transform.localPosition, offset };
		MoveLogic.LerpPath(this, path, team.speed, true);
	}

	public virtual void JumpToLocalCenter(Vector3 offset, float jumpHeight) {
		const int bezierPointCount = 24;
		Vector3[] bezier = new Vector3[bezierPointCount];
		Vector3 height = Vector3.up * jumpHeight;
		Vector3 start = transform.localPosition;
		Math3d.WriteBezier(bezier, start, start + height, offset + height, offset);
		MoveLogic.LerpPath(this, bezier, team.speed, true);
	}

	public void GetMoves(List<Coord> out_moves, List<Coord> out_captures, List<Coord> out_defends) {
		if (moveLogic == null) { return; }
		if (!moveAreUpToDate) {
			if (moves == null) { moves = new List<Coord>(); } else { moves.Clear(); }
			if (captures == null) { captures = new List<Coord>(); } else { captures.Clear(); }
			if (defends == null) { defends = new List<Coord>(); } else { defends.Clear(); }
			moveLogic.GetMoves(moves, captures, defends);
			moveAreUpToDate = true;
		}
		if (moves != null) { out_moves?.AddRange(moves); }
		if (captures != null) { out_captures?.AddRange(captures); }
		if (defends != null) { out_defends?.AddRange(defends); }
	}

	public override int GetHashCode() {
		return code.GetHashCode() ^ team.name.GetHashCode();
	}
}

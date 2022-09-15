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

	public void MarkMovesAsInvalid() {
		moveAreUpToDate = false;
	}

	protected override void Start() {
		base.Start();
		moveLogic = GetComponent<MoveLogic>();
	}

	public virtual void MoveTo(Coord coord) {
		if (moveLogic != null) {
			moveLogic.DoMove(coord);
		} else {
			MoveInternal(coord);
		}
		moveAreUpToDate = false;
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
		//Vector3 center = offset;
		//if (transform.parent != null) {
		//	center += transform.parent.position;
		//}
		//Vector3[] path = new Vector3[2] { transform.position, center };
		//MoveLogic.LerpPath(this, path, team.speed, false);
		Vector3[] path = new Vector3[2] { transform.localPosition, offset };
		MoveLogic.LerpPath(this, path, team.speed, true);

		//StartCoroutine(LerpToLocalCenterCoroutine(offset));
	}

	public virtual void JumpToLocalCenter(Vector3 offset, float jumpHeight) {
		const int bezierPointCount = 24;
		Vector3[] bezier = new Vector3[bezierPointCount];
		Vector3 height = Vector3.up * jumpHeight;
		Vector3 start = transform.localPosition;

		Math3d.WriteBezier(bezier, start, start + height, offset + height, offset);
		//Vector3[] path = new Vector3[2] { transform.position, center };
		MoveLogic.LerpPath(this, bezier, team.speed, true);
	}

	//public IEnumerator LerpToLocalCenterCoroutine(Vector3 targetPosition) {
	//	Transform _transform = transform;
	//	long then = System.Environment.TickCount;
	//	Vector3 delta = (_transform.localPosition - targetPosition);
	//	float distance = delta.magnitude;
	//	Vector3 dir = delta / distance;
	//	while (distance > 0) {
	//		long now = System.Environment.TickCount;
	//		long passed = now - then;
	//		then = now;
	//		float deltaTime = passed / 1000f;
	//		float move = team.speed * deltaTime;
	//		distance -= move;
	//		if (distance <= 0) {
	//			_transform.localPosition = targetPosition;
	//		} else {
	//			_transform.localPosition = targetPosition + dir * distance;
	//		}
	//		yield return null;
	//	}
	//}
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
}

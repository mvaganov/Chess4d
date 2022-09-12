using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : TiledGameObject {
	public Team team;
	public Board board;
	private MoveLogic moveLogic;
	private List<Coord> moves;
	protected override void Start() {
		base.Start();
		moveLogic = GetComponent<MoveLogic>();
	}

	public virtual void MoveTo(Coord coord) {
		if (moveLogic != null) {
			moveLogic.DoMove(coord);
		} else {
			SetTile(coord);
			MoveToLocalCenter(Vector3.zero);
		}
		moves = null;
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
		StartCoroutine(LerpToLocalCenterCoroutine(offset));
	}

	public IEnumerator LerpToLocalCenterCoroutine(Vector3 targetPosition) {
		Transform _transform = transform;
		const float frameRateDelay = 60f / 1000;
		long then = System.Environment.TickCount;
		Vector3 delta = (_transform.localPosition - targetPosition);
		float distance = delta.magnitude;
		Vector3 dir = delta / distance;
		while (distance > 0) {
			long now = System.Environment.TickCount;
			long passed = now - then;
			then = now;
			float deltaTime = passed / 1000f;
			float move = team.speed * deltaTime;
			distance -= move;
			if (distance <= 0) {
				_transform.localPosition = targetPosition;
			} else {
				_transform.localPosition = targetPosition + dir * distance;
			}
			yield return new WaitForSeconds(frameRateDelay);
		}
	}
	public List<Coord> GetMoves() {
		if (moveLogic == null) { return null; }
		return moves != null ? moves : moves = moveLogic.GetMoves(MoveCalculation.All);
	}
}

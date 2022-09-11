using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : TiledGameObject {
	public Team team;
	public Board board;
	private MoveLogic moveLogic;

	protected override void Start() {
		base.Start();
		moveLogic = GetComponent<MoveLogic>();
	}

	public virtual void MoveTo(Coord coord) {
		SetTile(coord);
		MoveToTile();
	}

	public void SetTile(Coord coord) {
		Tile tile = board.GetTile(coord);
		transform.SetParent(tile.transform, true);
	}

	public Tile GetTile() {
		return transform.GetComponentInParent<Tile>();
	}

	public virtual void MoveToTile() {
		StartCoroutine(LerpToCenterCoroutine());
	}

	public IEnumerator LerpToCenterCoroutine() {
		Transform _transform = transform;
		const float frameRateDelay = 60f / 1000;
		long then = System.Environment.TickCount;
		float distance = _transform.localPosition.magnitude;
		Vector3 dir = _transform.localPosition / distance;
		while (distance > 0) {
			long now = System.Environment.TickCount;
			long passed = now - then;
			then = now;
			float deltaTime = passed / 1000f;
			float move = team.speed * deltaTime;
			distance -= move;
			if (distance <= 0) {
				_transform.localPosition = Vector3.zero;
			} else {
				_transform.localPosition = dir * distance;
			}
			yield return new WaitForSeconds(frameRateDelay);
		}
	}
	public IEnumerable<Coord> GetMoves() {
		if (moveLogic == null) { return null; }
		return moveLogic.GetMoves();
	}
}

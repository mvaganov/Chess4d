using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum MoveKind {
	Move = 1,
	Attack = 2,
	MoveAttack = Move | Attack,
	Defend = 4,
	MoveDefend = Move | Defend,
	AttackDefend = Attack | Defend,
	MoveAttackDefend = Move | Attack | Defend
}

[RequireComponent(typeof(Piece))]
public class MoveLogic : MonoBehaviour {
	public Piece piece => GetComponent<Piece>();
	public Team team => GetComponent<Piece>().team;

	public void Awake() {
		if (!PieceHasCorrectLogic()) {
			piece.RefreshMoveLogic();
			PieceHasCorrectLogic();
		}
	}

	private bool PieceHasCorrectLogic() {
		Piece p = piece;
		if (p.MoveLogic == null) { return false; }
		if (p.MoveLogic != this) {
			throw new Exception($"{p.name} has {p.MoveLogic.GetType().Name}, trying to add {GetType().Name}?");
		}
		return true;
	}

	public virtual void StandardMoves(IEnumerable<Coord> directions, int maxSpaces,
	List<Move> out_moves, MoveKind moveKind) {
		Piece p = piece;
		StandardMoves(p, p.GetCoord(), directions, maxSpaces, out_moves, moveKind);
	}

	public virtual void StandardMoves(Coord position, IEnumerable<Coord> directions, int maxSpaces,
	List<Move> out_moves, MoveKind moveKind) {
		StandardMoves(piece, position, directions, maxSpaces, out_moves, moveKind);
	}

	public static void StandardMoves(Piece self, Coord position, IEnumerable<Coord> directions, int maxSpaces,
	List<Move> out_moves, MoveKind moveKind) {
		Board board = self.board;
		Tile tile = self.GetTile();
		if (tile == null) { return; }
		Coord cursor;
		foreach (Coord dir in directions) {
			cursor = position;
			if (dir == Coord.zero && maxSpaces > 0) {
				if (moveKind.HasFlag(MoveKind.Move)) {
					out_moves?.Add(new Move(self, position, cursor));
				}
				continue;
			}
			for (int i = 0; i < maxSpaces; ++i) {
				cursor += dir;
				if (!board.IsValid(cursor)) {
					break;
				}
				Piece other = board.GetPiece(cursor);
				Capture capture = new Capture(self, self.GetCoord(), cursor, other, cursor);
				if (moveKind.HasFlag(MoveKind.Defend)) {
					out_moves?.Add(capture);
				}
				if (other != null) {
					bool isAllies = self.team.IsAlliedWith(other.team);
					if (!isAllies) {
						if (moveKind.HasFlag(MoveKind.Attack)) {
							out_moves?.Add(capture);
						}
					}
					break;
				}
				if (moveKind.HasFlag(MoveKind.Move)) {
					out_moves?.Add(new Move(self, position, cursor));
				}
			}
		}
	}

	public virtual void GetMoves(List<Move> out_moves, MoveKind moveKind) {}

	public virtual void DoMove(Move move) {
		piece.MoveInternal(move.to);
	}

	public virtual void UndoMove(Move move) {
		piece.MoveInternal(move.from);
	}

	public static void LerpPath(MonoBehaviour script, Vector3[] path, float speed, bool localPosition = false) {
		if (!script.gameObject.activeInHierarchy) { return; }
		script.StartCoroutine(LerpToPath(script.transform, path, speed, localPosition));
	}

	public static IEnumerator LerpToPath(Transform _transform, Vector3[] path, float speed, bool localPosition = false) {
		Vector3[] deltas = new Vector3[path.Length - 1];
		float[] distances = new float[deltas.Length];
		float totalDistance = 0;
		for (int i = 0; i < deltas.Length; ++i) {
			Vector3 delta = path[i + 1] - path[i];
			float distance = delta.magnitude;
			deltas[i] = delta;
			distances[i] = distance;
			totalDistance += distance;
		}
		float distanceTraveled = 0;
		float segmentStartPoint = 0;
		float segmentEndPoint = distances[0];
		int index = 0;
		long then = System.Environment.TickCount;
		Transform currentParent = _transform.parent;
		while (index < path.Length && _transform.parent == currentParent) {
			long now = System.Environment.TickCount;
			long passed = now - then;
			then = now;
			float deltaTime = passed / 1000f;
			float move = speed * deltaTime;
			distanceTraveled += move;
			while(index < distances.Length && distanceTraveled > segmentEndPoint) {
				segmentStartPoint += distances[index];
				++index;
				if (index < distances.Length) {
					segmentEndPoint += distances[index];
				} else {
					segmentEndPoint = totalDistance;
				}
			}
			float progress;
			if (distanceTraveled < segmentEndPoint) {
				float distanceProgress = distanceTraveled - segmentStartPoint;
				progress = distanceProgress / distances[index];
			} else {
				if (localPosition) {
					_transform.localPosition = path[path.Length - 1];
				} else {
					_transform.position = path[path.Length - 1];
				}
				yield break;
			}
			if (localPosition) {
				_transform.localPosition = Vector3.Lerp(path[index], path[index + 1], progress);
			} else {
				_transform.position = Vector3.Lerp(path[index], path[index + 1], progress);
			}
			yield return null;
		}
	}
}

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
public class PieceLogic : MonoBehaviour {
	protected State _state = new State(Coord.zero, -1, false);
	public Piece piece => GetComponent<Piece>();
	public Team team => GetComponent<Piece>().team;
	public State state => _state;
	public Coord CurrentCoord { get => state.coord; }
	public int MoveCount { get => state.moveCount; }
	public virtual void SetState<STATE>(State state) where STATE : State {
		if (state is STATE s && !_state.Equals(s)) { _state = state; }
	}
	public void MoveTo(Coord coord, short newMoveCount = -1) {
		_state = _state.MovedTo(coord, newMoveCount);
	}
	/// <summary>
	/// for storing per turn state of this piece: position, move count, special status
	/// </summary>
	public class State {
		readonly public Coord coord;
		readonly public short moveCount = 0;
		/// <summary>
		/// 0: is removed from board
		/// </summary>
		public short flags;
		public bool IsOnBoard {
			get => (flags & 1) == 0;
			set => flags = (short)((flags & ~1) | (value?0:1));
		}
		public State(Coord coord) : this (coord, 0, true) { }
		/// <param name="coord"></param>
		/// <param name="moveCount">a negative value marks a state as invalid</param>
		/// <param name="isOnBoard"></param>
		public State(Coord coord, short moveCount, bool isOnBoard) {
			this.coord = coord; this.moveCount = moveCount; IsOnBoard = isOnBoard;
		}
		public State(State s) { coord = s.coord; moveCount = s.moveCount; flags = s.flags; }
		public virtual State MovedTo(Coord coord, short newMoveCount = -1) =>
			new State(coord, (short)(newMoveCount < 0 ? moveCount + 1 : newMoveCount), true);
		public override bool Equals(object obj) => obj is State s
			&& coord == s.coord && moveCount == s.moveCount;
		public override int GetHashCode() => coord.GetHashCode() ^ moveCount;
	}

	public virtual void Awake() {
		if (!PieceHasCorrectLogic()) {
			piece.RefreshMoveLogic();
			PieceHasCorrectLogic();
		}
	}

	public virtual void Initialize() { }

	private bool PieceHasCorrectLogic() {
		Piece p = piece;
		if (p.Logic == null) { return false; }
		if (p.Logic != this) {
			throw new Exception($"{p.name} has {p.Logic.GetType().Name}, trying to add {GetType().Name}?");
		}
		return true;
	}

	public virtual void StandardMoves(GameState state, IEnumerable<Coord> directions, int maxSpaces,
	List<IGameMoveBase> out_moves, MoveKind moveKind) {
		Piece p = piece;
		StandardMoves(state, p, p.GetCoord(), directions, maxSpaces, out_moves, moveKind);
	}

	public virtual void StandardMoves(GameState state, Coord position, IEnumerable<Coord> directions, int maxSpaces,
	List<IGameMoveBase> out_moves, MoveKind moveKind) {
		StandardMoves(state, piece, position, directions, maxSpaces, out_moves, moveKind);
	}

	public static void StandardMoves(GameState state, Piece self, Coord position, IEnumerable<Coord> directions, int maxSpaces,
	List<IGameMoveBase> out_moves, MoveKind moveKind) {
		Board board = self.board;
		Tile tile = self.GetTile();
		if (tile == null) { return; }
		Coord cursor;
		foreach (Coord dir in directions) {
			cursor = position;
			if (dir == Coord.zero && maxSpaces > 0) {
				if (moveKind.HasFlag(MoveKind.Move)) {
					out_moves?.Add(new PieceMove(self.board, self, position, cursor));
				}
				continue;
			}
			for (int i = 0; i < maxSpaces; ++i) {
				cursor += dir;
				if (!board.IsValid(cursor)) {
					break;
				}
				Piece other = state.GetPieceAt(cursor);//board.GetPiece(cursor);
				if (other != null) {
					bool isAllies = self.team.IsAlliedWith(other.team);
					//if (!isAllies) {
					//	if (moveKind.HasFlag(MoveKind.Attack)) {
					//		PieceMoveAttack capture = new PieceMoveAttack(self.board, self, self.GetCoord(), cursor, other/*, cursor*/);
					//		out_moves?.Add(capture);
					//	}
					//}
					//if (isAllies) {
					//	if (moveKind.HasFlag(MoveKind.Defend)) {
					//		PieceMoveAttack defend = new PieceMoveAttack(self.board, self, self.GetCoord(), cursor, other/*, cursor*/);
					//		out_moves?.Add(defend);
					//	}
					//}
					if((moveKind.HasFlag(MoveKind.Attack) && !isAllies)
					|| (moveKind.HasFlag(MoveKind.Defend) && isAllies)) {
						PieceMoveAttack move = new PieceMoveAttack(self.board, self, self.GetCoord(), cursor, other);
						out_moves?.Add(move);
					}
					break;
				} else {
					if (moveKind.HasFlag(MoveKind.Move) || moveKind.HasFlag(MoveKind.MoveAttack) || moveKind.HasFlag(MoveKind.Defend)) {
						PieceMoveAttack move = new PieceMoveAttack(self.board, self, self.GetCoord(), cursor, other);
						out_moves?.Add(move);
					}
				}
				if (moveKind.HasFlag(MoveKind.Move)) {
					out_moves?.Add(new PieceMove(self.board, self, position, cursor));
				}
			}
		}
	}

	public virtual void GetMoves(GameState state, List<IGameMoveBase> out_moves, MoveKind moveKind) {}

	public virtual void DoMove(BasicMove move) {
		piece.MoveTransform(move.to);
	}

	public virtual void UndoMove(BasicMove move) {
		piece.MoveTransform(move.from);
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

using NonStandard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : TiledGameObject {
	/// <summary>
	/// these are stateless variables: they don't change during the game
	/// </summary>
	public Team team;
	public string code;
	public Board board;
	private PieceLogic _logic;
	public float jumpHeight = 0;
	[SerializeField] private List<IGameMoveBase> moves;
	private Coord movesCalculatedAt;
	public SpriteRenderer worldIcon;
	public bool animating = true;
	public ChessGame Game => board.game;
	public PieceLogic Logic => _logic;
	public int moveCount { get => Logic.MoveCount; }
	public Coord CurrentCoord => Logic.CurrentCoord;

	public void MarkMovesAsInvalid() {
		movesCalculatedAt = Coord.negativeOne;
	}

	protected override void Start() {
		base.Start();
		RefreshMoveLogic();
	}

	public void SetCurrentCoordTo(Coord coord) {
		Logic.MoveTo(coord);
	}

	public override bool TryGetCoord(out Coord coord) {
		if (Logic == null || Logic.state == null) {
			Debug.LogWarning("piece does not have own coordinate");
			return base.TryGetCoord(out coord);
		}
		coord = Logic.CurrentCoord;
		return true;
	}

	public void RefreshMoveLogic() {
		_logic = GetComponent<PieceLogic>();
	}

	public virtual void DoMove(BasicMove move) {
		if (_logic != null) {
			_logic.DoMove(move);
		} else {
			MoveTransform(move.to);
		}
	}

	public virtual void UndoMove(BasicMove move) {
		if (_logic != null) {
			_logic.UndoMove(move);
		} else {
			MoveTransform(move.from);
		}
	}

	internal void MoveTransform(Coord coord) {
		SetTile(coord);
		if (animating) {
			AnimateMovement();
		}
	}

	public void AnimateMovement() {
		if (jumpHeight == 0) {
			LerpToLocalCenter(Vector3.zero);
			return;
		}
		JumpToLocalCenter(Vector3.zero, jumpHeight);
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
		PieceLogic.LerpPath(this, path, team.speed, true);
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
		PieceLogic.LerpPath(this, bezier, team.speed, true);
	}

	public void GetMoves(GameState state, List<IGameMoveBase> out_moves, MoveKind moveKind = MoveKind.MoveAttackDefend) {
		if (_logic == null) { return; }
		Coord here = GetCoord();
		if (movesCalculatedAt == here) {
			out_moves?.AddRange(moves);
			return;
		}
		if (moves != null) { GetMovesForceCalculation(state, here, out_moves, moveKind); }
	}

	public void GetMovesForceCalculation(GameState state, Coord here, List<IGameMoveBase> out_moves, MoveKind moveKind = MoveKind.MoveAttackDefend) {
		if (moves == null) { moves = new List<IGameMoveBase>(); } else { moves.Clear(); }
		if (here.col >= 0 && here.row >= 0) {
			_logic.GetMoves(state, moves, moveKind);
		}
		movesCalculatedAt = here;
		out_moves?.AddRange(moves);
	}

	public override int GetHashCode() {
		return code.GetHashCode() ^ team.name.GetHashCode();
	}
}

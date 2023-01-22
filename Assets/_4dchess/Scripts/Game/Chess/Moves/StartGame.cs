using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : IGameMoveBase {
	Board board;
	public Board Board => board;
	public Piece Piece => null;
	public StartGame(Board board) { this.board = board; }
	public Coord GetRelevantCoordinate() => Coord.zero;
	public bool Involves(Piece piece) => false;
	public void GetMovingPieces(HashSet<Piece> out_movingPieces) { }
	public void Do() { }
	public void Undo() { }
	public override string ToString() { return "start game"; }
	public Piece GetPiece(int index) => null; // TODO should get all pieces?
	public int GetPieceCount() => 0; // TODO should be the entire board?
	public void DoWithoutAnimation() { }
	public void UndoWithoutAnimation() { }
}

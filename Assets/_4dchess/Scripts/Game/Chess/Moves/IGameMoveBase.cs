using System.Collections.Generic;

/// <summary>
/// TODO implement multi-move, which allows board resets.
/// </summary>
public interface IGameMoveBase {
	public Board Board { get;}
	public Piece Piece { get;}
	public Piece GetPiece(int index);
	public int GetPieceCount();
	public Coord GetRelevantCoordinate();
	public bool Involves(Piece piece);
	public void GetMovingPieces(HashSet<Piece> out_movingPieces);
	public void Do();
	public void Undo();
	public void DoWithoutAnimation();
	public void UndoWithoutAnimation();
}

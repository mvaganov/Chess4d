using UnityEngine;

public class TiledArrow : TiledGameObject {
	public NonStandard.Wire Arrow;
	[SerializeField] private Coord _destinationCoord;
	public Coord Destination {
		get { return _destinationCoord; }
		set {
			if (Arrow == null) {
				Arrow = NonStandard.Lines.MakeWire();
				Arrow.transform.SetParent(transform);
				Arrow.LineRenderer.alignment = LineAlignment.TransformZ;
				Arrow.transform.Rotate(90, 0, 0);
			}
			_destinationCoord = value;
			Transform _transform = transform;
			Vector3 start = _transform.position;
			Vector3 end = GetBoard().CoordToWorldPosition(_destinationCoord);

			Vector3 startAdjust = Vector3.up;
			Tile tile = GetComponentInParent<Tile>();
			Piece p = tile.GetPiece();
			if (p != null) {
				Collider collider = p.GetComponent<Collider>();
				if (collider != null) {
					start.y += collider.bounds.size.y;
					startAdjust /= 2;
				}
			}

			Vector3 endAdjust = Vector3.up;
			tile = GetBoard().GetTile(_destinationCoord);
			p = tile.GetPiece();
			if (p != null) {
				Collider collider = p.GetComponent<Collider>();
				if (collider != null) {
					end.y += collider.bounds.size.y;
					endAdjust /= 2;
				}
			}
			Arrow.Bezier(start, start + startAdjust, end + endAdjust, end, Color.red, NonStandard.Lines.End.Arrow);
			//Arrow.Arc(start, end, Vector3.up, Color.red, NonStandard.Lines.End.Arrow);
		}
	}
}

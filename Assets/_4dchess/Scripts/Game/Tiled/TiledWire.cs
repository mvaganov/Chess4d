using UnityEngine;

public class TiledWire : TiledGameObject {
	public NonStandard.Wire Wire;
	public NonStandard.Lines.End destinationEnd = NonStandard.Lines.End.Arrow;
	public override Material Material {
		get => Wire.LineRenderer.material;
		set => Wire.LineRenderer.material = value;
	}
	[SerializeField] private Coord _destinationCoord;
	public Coord Destination {
		get { return _destinationCoord; }
		set {
			DrawLine(value);
		}
	}
	public void DrawLine(Coord value) {
		if (Wire == null) {
			Wire = NonStandard.Lines.MakeWire();
			Wire.transform.SetParent(transform);
			Wire.LineRenderer.alignment = LineAlignment.TransformZ;
			Wire.transform.Rotate(90, 0, 0);
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
		Wire.Bezier(start, start + startAdjust, end + endAdjust, end, Color.red, destinationEnd, 1f / 16);
		//Arrow.Arc(start, end, Vector3.up, Color.red, NonStandard.Lines.End.Arrow);
	}
}

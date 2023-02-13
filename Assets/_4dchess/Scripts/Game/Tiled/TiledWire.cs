using UnityEngine;

public class TiledWire : TiledGameObject {
	public NonStandard.Wire Wire;
	public NonStandard.LineEnd destinationEnd = NonStandard.LineEnd.Arrow;
	public bool swapStartAndEnd;
	public override Material Material {
		get => Wire.LineRenderer.material;
		set => Wire.LineRenderer.material = value;
	}
	public override Color Color {
		get => Material.color;
		set {
			Renderer[] renderers = GetComponentsInChildren<Renderer>();
			foreach (Renderer r in renderers) {
				r.material.color = value;
			}
		}
	}
	[SerializeField] private Coord _destinationCoord;
	// TODO allow setting the source as well, so that arrows can come and go.
	public Coord Destination {
		get { return _destinationCoord; }
		//set { DrawLine(GetCoord(), value); }
	}

	//public void SetDestination(Coord coord, Color color) {
	//	DrawLine(GetCoord(), coord, color);
	//}
	//public void DrawLine(Coord startCoord, Coord value) {
	//	DrawLine(startCoord, value, Color.green);
	//}

	public void DrawLine(Coord startCoord, Coord value, Color color) {
		if (Wire == null) {
			Wire = NonStandard.Wires.MakeWire();
			Wire.transform.SetParent(transform);
			Wire.LineRenderer.alignment = LineAlignment.TransformZ;
			Wire.transform.Rotate(90, 0, 0);
		}
		_destinationCoord = value;
		Coord coord0 = startCoord, coord1 = _destinationCoord;
		if (swapStartAndEnd) {
			Coord temp = coord0; coord0 = coord1; coord1 = temp;
		}
		Vector3 start = GetBoard().CoordToWorldPosition(coord0);
		Vector3 end = GetBoard().CoordToWorldPosition(coord1);

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
		tile = GetBoard().GetTile(coord1);
		p = tile.GetPiece();
		if (p != null) {
			Collider collider = p.GetComponent<Collider>();
			if (collider != null) {
				end.y += collider.bounds.size.y;
				endAdjust /= 2;
			}
		}
		Wire.Bezier(start, start + startAdjust, end + endAdjust, end, color, destinationEnd, 1f / 16);
		//Arrow.Arc(start, end, Vector3.up, Color.red, NonStandard.Lines.End.Arrow);
	}
}

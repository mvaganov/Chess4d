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
			}
			_destinationCoord = value;
			Transform _transform = transform;
			Vector3 start = _transform.position;
			Vector3 end = GetBoard().CoordToWorldPosition(_destinationCoord);
			Vector3 delta = end - start;
			Vector3 half = delta / 2;
			Vector3 center = start + half;
			Vector3 dir = delta.normalized;
			Vector3 normal = Vector3.Cross(dir, Vector3.up).normalized;
			//Arrow.Arrow(start + Vector3.up, destination, Color.red);
			//Arrow.Arc(180, normal, center-start, center, Color.red, NonStandard.Lines.End.Arrow);
			Arrow.Arc(start, end, Vector3.up, Color.red, NonStandard.Lines.End.Arrow);
		}
	}
}

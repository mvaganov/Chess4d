using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastInteraction : MonoBehaviour {
	public Camera cam;
	public Transform rayHitMarker;
	public Gradient selectionColor;
	private TiledGameObject currentHovered;
	void Start() {
		if (cam == null) { cam = Camera.main; }
	}

	void Update() {
		if (Input.GetMouseButton(0)) {
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit rh)) {
				if (rayHitMarker != null) {
					rayHitMarker.transform.position = rh.point;
					Vector3 up = rh.normal;
					Vector3 right = cam.transform.right;
					Vector3 forward = Vector3.Cross(up, right); ;
					rayHitMarker.transform.rotation = Quaternion.LookRotation(forward, up);
				}
				TiledGameObject tgo = rh.collider.GetComponent<TiledGameObject>();
				if (tgo != null) {
					if (tgo != currentHovered) {
						if (currentHovered != null) {
							currentHovered.ResetColor();
						}
						currentHovered = tgo;
						currentHovered.ColorCycle(selectionColor);
					}
				}
			} else {
				if (currentHovered != null) {
					currentHovered.ResetColor();
				}
			}
		}
	}
}

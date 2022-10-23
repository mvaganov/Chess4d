using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
	public bool _onlyOnce = true;
	public Camera _camera;

	public void Update() {
		Camera camera = (_camera != null) ? _camera : Camera.main;
		if (camera.orthographic) {
			transform.rotation = camera.transform.rotation;
		} else {
			Transform t = transform, cam = camera.transform;
			Vector3 delta = t.position - cam.position;
			float dist = delta.magnitude;
			Vector3 dir = delta / dist;
			Vector3 right = cam.right;
			Vector3 up = Vector3.Cross(dir, right);
			t.rotation = Quaternion.LookRotation(dir, up);
		}
		if (_onlyOnce) { enabled = false; }
	}

	public void Refresh() {
		if (_onlyOnce) {
			enabled = true;
		} else {
			Debug.LogWarning("refresh not needed, Billboard " + this + " is constantly updating");
		}
	}
}

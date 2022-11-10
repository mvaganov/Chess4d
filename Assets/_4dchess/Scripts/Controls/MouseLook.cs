using UnityEngine;

public class MouseLook : MonoBehaviour {
	public Transform CameraTransform;
	public float MouseSensitivityX = 5, MouseSensitivityY = -5;
	public Vector3 eulerRotation;
	public bool HideMouse = true;

	public virtual Vector2 PitchYaw => new Vector2(InputMap.MouseChangeY * MouseSensitivityY,
		InputMap.MouseChangeX * MouseSensitivityX);

	private void Reset() {
		Camera cam = Camera.main;
		if (cam != null) {
			CameraTransform = cam.transform;
		}
	}

	protected virtual void OnEnable() {
		if (!HideMouse) { return; }
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	protected virtual void OnDisable() {
		if (!HideMouse) { return; }
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	private void Start() {
		eulerRotation = CameraTransform.rotation.eulerAngles;
	}

	protected virtual void LateUpdate() {
		Vector2 pitchYaw = PitchYaw;
		if (pitchYaw.x != 0 || pitchYaw.x != 0) {
			eulerRotation.x += pitchYaw.x;
			eulerRotation.y += pitchYaw.y;
			CameraTransform.rotation = Quaternion.Euler(eulerRotation);
		}
	}

	public void CopyTransformRotation(Transform t) {
		CameraTransform.rotation = t.rotation;
		eulerRotation = CameraTransform.rotation.eulerAngles;
	}
}

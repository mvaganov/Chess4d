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
		// poll data directly, do not rely on callbacks.
		float yaw = InputMap.MouseChangeX * MouseSensitivityX;
		float pitch = InputMap.MouseChangeY * MouseSensitivityY;
		if (pitch != 0 || yaw != 0) {
			eulerRotation.x += pitch;
			eulerRotation.y += yaw;
			CameraTransform.rotation = Quaternion.Euler(eulerRotation);
		}
	}

	public void CopyTransformRotation(Transform t) {
		CameraTransform.rotation = t.rotation;
		eulerRotation = CameraTransform.rotation.eulerAngles;
	}
}

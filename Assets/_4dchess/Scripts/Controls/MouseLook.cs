using UnityEngine;

public class MouseLook : MonoBehaviour {
	public Transform CameraTransform;
	public float MouseSensitivityX = 5, MouseSensitivityY = -5;
	public Vector3 eulerRotation;
	public bool HideMouse = true;
	private void Reset() {
		Camera cam = Camera.main;
		if (cam != null) {
			CameraTransform = cam.transform;
		}
	}
	private void OnEnable() {
		if (!HideMouse) { return; }
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
	private void OnDisable() {
		if (!HideMouse) { return; }
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
	private void Start() {
		eulerRotation = CameraTransform.rotation.eulerAngles;
	}
	void LateUpdate() {
		float yaw = Input.GetAxis("Mouse X") * MouseSensitivityX;
		float pitch = Input.GetAxis("Mouse Y") * MouseSensitivityY;
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

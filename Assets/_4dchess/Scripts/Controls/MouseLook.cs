using UnityEngine;

public class MouseLook : MonoBehaviour {
	public Transform CameraTransform;
	public float MouseSensitivityX = 5, MouseSensitivityY = -5;
	public Vector3 eulerRotation;
	public bool HideMouse = true;
	private Vector2 mouseChange;
	private InputMap inputMap;
	private void Reset() {
		Camera cam = Camera.main;
		if (cam != null) {
			CameraTransform = cam.transform;
		}
	}
	private void OnEnable() {
		if (inputMap == null) { inputMap = GetComponent<InputMap>(); }
		if (inputMap != null) {
			inputMap.Add(KeyCode2.MouseChange, InputMap.KeyPressState.None, UpdateMouseMove, name);
		}
		if (!HideMouse) { return; }
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
	private void OnDisable() {
		if (inputMap != null) {
			inputMap.Remove(KeyCode2.MouseChange, InputMap.KeyPressState.None, name);
		}
		if (!HideMouse) { return; }
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
	public void UpdateMouseMove() => mouseChange = InputMap.MouseChange;
	private void Start() {
		eulerRotation = CameraTransform.rotation.eulerAngles;
	}
	void LateUpdate() {
		float yaw, pitch;
		if (inputMap) {
			yaw = mouseChange.x * MouseSensitivityX;
			pitch = mouseChange.y * MouseSensitivityY;
		} else {
			yaw = InputMap.MouseChangeX * MouseSensitivityX;
			pitch = InputMap.MouseChangeY * MouseSensitivityY;
		}
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

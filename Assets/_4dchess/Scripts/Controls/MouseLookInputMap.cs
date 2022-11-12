using UnityEngine;

public class MouseLookInputMap : MouseLook {
	private InputMap inputMap;
	protected Vector2 mouseChange;
	public bool gettingValue;
	public void UpdateMouseMove() {
		mouseChange = InputMap.MouseChange;
		gettingValue = true;
	}

	public override Vector2 PitchYaw => (inputMap != null) ?
		new Vector2(mouseChange.y * MouseSensitivityY, mouseChange.x * MouseSensitivityX) :
		base.PitchYaw;
	
	protected override void OnEnable() {
		if (inputMap == null) { inputMap = FindObjectOfType<InputMap>(); }
		if (inputMap != null) {
			inputMap.Add(KeyCode2.MouseChange, InputMap.KeyPressState.Press, UpdateMouseMove, name);
		}
		base.OnEnable();
	}

	protected override void OnDisable() {
		if (inputMap != null) {
			inputMap.Remove(KeyCode2.MouseChange, InputMap.KeyPressState.Press, name);
		}
		base.OnDisable();
	}
	private void Update() {
		gettingValue = false;
	}
	protected override void LateUpdate() {
		// TODO find out why mouseChange does not change correctly when the mouse input is detected by InputMap.ResolveMouseDeltaCallbacks
		// the code that does work (polls directly)

		//float yaw, pitch;
		//if (false && inputMap) {
		//	yaw = mouseChange.x * MouseSensitivityX;
		//	pitch = mouseChange.y * MouseSensitivityY;
		//} else {
		//	yaw = InputMap.MouseChangeX * MouseSensitivityX;
		//	pitch = InputMap.MouseChangeY * MouseSensitivityY;
		//}
		//if (pitch != 0 || yaw != 0) {
		//	eulerRotation.x += pitch;
		//	eulerRotation.y += yaw;
		//	CameraTransform.rotation = Quaternion.Euler(eulerRotation);
		//}

		// the code that should work (uses value updated by callback)
		Vector2 pitchYaw = PitchYaw;
		if (pitchYaw.x != 0 || pitchYaw.x != 0) {
			eulerRotation.x += pitchYaw.x;
			eulerRotation.y += pitchYaw.y;
			CameraTransform.rotation = Quaternion.Euler(eulerRotation);
		}
	}
}

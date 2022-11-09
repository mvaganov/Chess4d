using UnityEngine;

public class AxisMovementInputMap : AxisMovement {
	public bool useInputMap = true;
	private Vector3 inputMapDirection;
	private InputMap _inputMap;

	private string GetPress(int axisId) { return name + " " + axis[axisId].name + " press"; }
	private string GetRelease(int axisId) { return name + " " + axis[axisId].name + " release"; }
	private string GetIncreasePress(int axisId) => "+" + GetPress(axisId);
	private string GetIncreaseRelease(int axisId) => "+" + GetPress(axisId);
	private string GetDecreasePress(int axisId) => "-" + GetRelease(axisId);
	private string GetDecreaseRelease(int axisId) => "-" + GetRelease(axisId);

	private void OnEnable() {
		if (useInputMap && _inputMap == null) {
			_inputMap = FindObjectOfType<InputMap>();
		}
		if (!useInputMap || _inputMap == null) { return; }
		for (int a = 0; a < axis.Length; ++a) {
			KeyAxis ax = axis[a];
			for (int i = 0; i < ax._increase.Length; ++i) {
				_inputMap.Add((KeyCode2)ax._increase[i], InputMap.KeyPressState.Press, CheckValue[a], GetIncreasePress(a));
				_inputMap.Add((KeyCode2)ax._increase[i], InputMap.KeyPressState.Release, CheckValue[a], GetIncreaseRelease(a));
			}
			for (int i = 0; i < ax._decrease.Length; ++i) {
				_inputMap.Add((KeyCode2)ax._decrease[i], InputMap.KeyPressState.Press, CheckValue[a], GetDecreasePress(a));
				_inputMap.Add((KeyCode2)ax._decrease[i], InputMap.KeyPressState.Release, CheckValue[a], GetDecreaseRelease(a));
			}
		}
		// TODO when this AxisMovementInputMap is turned on, it should trigger one of the events above, not this code below.
		//inputMapDirection = new Vector3(axis[0].Value, axis[1].Value, axis[2].Value);
	}
	private void OnDisable() {
		if (!useInputMap || _inputMap == null) { return; }
		for (int a = 0; a < axis.Length; ++a) {
			KeyAxis ax = axis[a];
			for (int i = 0; i < ax._increase.Length; ++i) {
				_inputMap.Remove((KeyCode2)ax._increase[i], InputMap.KeyPressState.Press, GetIncreasePress(a));
				_inputMap.Remove((KeyCode2)ax._increase[i], InputMap.KeyPressState.Release, GetIncreaseRelease(a));
			}
			for (int i = 0; i < ax._decrease.Length; ++i) {
				_inputMap.Remove((KeyCode2)ax._decrease[i], InputMap.KeyPressState.Press, GetDecreasePress(a));
				_inputMap.Remove((KeyCode2)ax._decrease[i], InputMap.KeyPressState.Release, GetDecreaseRelease(a));
			}
		}
	}
	private InputMap.InputEventDelegate[] CheckValue => _IncreaseEvents != null ? _IncreaseEvents : _IncreaseEvents =
		new InputMap.InputEventDelegate[] { ValueX, ValueY, ValueZ };
	private InputMap.InputEventDelegate[] _IncreaseEvents = null;
	private void ValueX() { inputMapDirection[0] = axis[0].Value; }
	private void ValueY() { inputMapDirection[1] = axis[1].Value; }
	private void ValueZ() { inputMapDirection[2] = axis[2].Value; Debug.Log("???"); }

	public override void LateUpdate() {
		Debug.Log((useInputMap && _inputMap != null)+" "+ inputMapDirection);
		Vector3 delta = (useInputMap && _inputMap != null) ? inputMapDirection :
			new Vector3(axis[0].Value, axis[1].Value, axis[2].Value);
		if (delta != Vector3.zero) {
			Vector3 absoluteSpaceDelta = body.TransformDirection(delta);
			if (rb != null) {
				rb.velocity = absoluteSpaceDelta * speed;
			} else {
				body.transform.position += absoluteSpaceDelta * (Time.deltaTime * speed);
			}
		}
	}
}

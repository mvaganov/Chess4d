using UnityEngine;

/// <summary>
/// sensitivity to Unity keys in a way that is auditable by InputMap
/// </summary>
public class KeySensitivityInputMap : KeySensitivity {
	private InputMap _inputMap;

	private string PressDescription => name + " press";
	private string ReleaseDescription => name + " release";

	private void Reset() {
		KeySensitivity ks = GetComponent<KeySensitivity>();
		if (ks == null) { return; }
		this._keys = (KeyCode[])ks.Keys.Clone();
		this._onKeyDown = ks.OnKeyDown;
		this._onKeyUp = ks.OnKeyUp;
		this._trigger = ks.TriggerSetting;
	}

	private void OnEnable() {
		_inputMap = GameObject.FindObjectOfType<InputMap>();
		if (_inputMap == null) { return; }
		for (int i = 0; i < _keys.Length; i++) {
			if (HasKeyDownEvent) {
				_inputMap.Add((KeyCode2)_keys[i], InputMap.KeyPressState.Down, CheckPress, PressDescription);
			}
			if (HasKeyUpEvent) {
				_inputMap.Add((KeyCode2)_keys[i], InputMap.KeyPressState.Up, CheckRelease, ReleaseDescription);
			}
		}
	}

	private void OnDisable() {
		if (_inputMap == null) { return; }
		for (int i = 0; i < _keys.Length; i++) {
			if (HasKeyDownEvent) {
				var down = _inputMap.Remove((KeyCode2)_keys[i], InputMap.KeyPressState.Down, PressDescription);
				if (down == null || down.Count == 0) { Debug.Log("uhm... did not remove any Down"); }
			}
			if (HasKeyUpEvent) {
				var up = _inputMap.Remove((KeyCode2)_keys[i], InputMap.KeyPressState.Up, ReleaseDescription);
				if (up == null || up.Count == 0) { Debug.Log("uhm... did not remove any uP"); }
			}
		}
	}

	public override void Update() {
		if (_inputMap != null) { return; }
		base.Update();
	}
}

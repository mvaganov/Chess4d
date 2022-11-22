using UnityEngine;

/// <summary>
/// sensitivity to Unity keys in a way that is auditable by InputMap
/// </summary>
public class KeySensitivityInputMap : KeySensitivity_<KeyCode2> {
	private InputMap _inputMap;

	private string PressDescription => name + " press";
	private string ReleaseDescription => name + " release";

	private void Reset() {
		KeySensitivityInputMap ks = GetComponent<KeySensitivityInputMap>();
		if (ks == null) { return; }
		this._keys = (KeyCode2[])ks.Keys.Clone();
		this._onKeyDown = ks.OnKeyDown;
		this._onKeyUp = ks.OnKeyUp;
		this._trigger = ks.TriggerSetting;
	}

	private void OnEnable() {
		_inputMap = GameObject.FindObjectOfType<InputMap>();
		if (_inputMap == null) { return; }
		for (int i = 0; i < _keys.Length; i++) {
			if (HasKeyDownEvent) {
				_inputMap.Add(_keys[i], InputMap.KeyPressState.Press, CheckPressCallback, PressDescription);
			}
			if (HasKeyUpEvent) {
				_inputMap.Add(_keys[i], InputMap.KeyPressState.Release, CheckReleaseCallback, ReleaseDescription);
			}
		}
	}

	private void OnDisable() {
		if (_inputMap == null) { return; }
		for (int i = 0; i < _keys.Length; i++) {
			if (HasKeyDownEvent) {
				var down = _inputMap.Remove(_keys[i], InputMap.KeyPressState.Press, PressDescription);
				if (down == null || down.Count == 0) { Debug.Log("uhm... did not remove any Down"); }
			}
			if (HasKeyUpEvent) {
				var up = _inputMap.Remove(_keys[i], InputMap.KeyPressState.Release, ReleaseDescription);
				if (up == null || up.Count == 0) { Debug.Log("uhm... did not remove any uP"); }
			}
		}
	}

	public void CheckPressCallback() {
		//Debug.Log(name+ " press?");
		UpdatePressedStates();
		CheckPress();
	}

	public void CheckReleaseCallback() {
		//Debug.Log(name + " release?");
		UpdatePressedStates();
		CheckRelease();
	}

	public override void Update() {
		if (_inputMap != null) { return; }
		base.Update();
	}
}

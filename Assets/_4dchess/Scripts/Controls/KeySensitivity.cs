using UnityEngine;
using UnityEngine.Events;

public class KeySensitivity : MonoBehaviour {
	[SerializeField] private KeyCode[] _keys = new KeyCode[1];
	[SerializeField] private KeyEvent _onKeyDown = new KeyEvent();
	[SerializeField] private KeyEvent _onKeyUp = new KeyEvent();
	[SerializeField] private Trigger _trigger = Trigger.AnyPressedOrReleased;
	private InputMap _inputMap;
	private bool[] pressedStates;

	public KeyCode[] Keys { get => _keys; set => _keys = value; }
	public UnityEvent OnKeyDown => _onKeyDown.Actions;
	public UnityEvent OnKeyUp => _onKeyUp.Actions;
	private string PressDescription => name + " press";
	private string ReleaseDescription => name + " release";

	public enum Trigger { AnyPressedOrReleased, OnePressedAllReleased, AllPressedAllReleased }

	[System.Serializable] public class KeyEvent { public UnityEvent Actions; }

	private void Start() {
		pressedStates = new bool[_keys.Length];
	}

	private void OnEnable() {
		_inputMap = GameObject.FindObjectOfType<InputMap>();
		if (_inputMap == null) { return; }
		for (int i = 0; i < _keys.Length; i++) {
			_inputMap.Add((KeyCode2)_keys[i], InputMap.KeyPressState.Down, CheckPress, PressDescription);
			_inputMap.Add((KeyCode2)_keys[i], InputMap.KeyPressState.Up, CheckRelease, ReleaseDescription);
		}
	}

	private void OnDisable() {
		if (_inputMap == null) { return; }
		for (int i = 0; i < _keys.Length; i++) {
			var down = _inputMap.Remove((KeyCode2)_keys[i], InputMap.KeyPressState.Down, PressDescription);
			var up = _inputMap.Remove((KeyCode2)_keys[i], InputMap.KeyPressState.Up, ReleaseDescription);
			if (down.Count == 0) { Debug.Log("uhm... did not remove any Down"); }
			if (up.Count == 0) { Debug.Log("uhm... did not remove any uP"); }
		}
	}

	public void Update() {
		if (_inputMap != null) { return; }
		CheckPress();
		CheckRelease();
	}

	public void CheckPress() {
		for (int i = 0; i < _keys.Length; i++) {
			if (!Input.GetKeyDown(_keys[i])) { continue; }
			pressedStates[i] = true;
			bool doTrigger = IsPressed();
			if (doTrigger) { OnKeyDown.Invoke(); }
		}
	}

	public void CheckRelease() {
		for (int i = 0; i < _keys.Length; i++) {
			if (!Input.GetKeyUp(_keys[i])) { continue; }
			pressedStates[i] = false;
			bool doTrigger = IsUnpressed();
			if (doTrigger) { OnKeyUp.Invoke(); }
		}
	}

	public bool IsPressed() {
		switch (_trigger) {
			case Trigger.OnePressedAllReleased: return PressCountExactly(1);
			case Trigger.AnyPressedOrReleased: return true;
			case Trigger.AllPressedAllReleased: return AllPressed();
		}
		return false;
	}

	public bool IsUnpressed() {
		switch (_trigger) {
			case Trigger.OnePressedAllReleased: return NonePressed();
			case Trigger.AnyPressedOrReleased: return true;
			case Trigger.AllPressedAllReleased: return NonePressed();
		}
		return false;
	}

	public bool PressCountExactly(int count) {
		int found = 0;
		for (int i = pressedStates.Length-1; i >= 0; --i) {
			if (pressedStates[i]) {
				++found;
				if (found > count) { return false; }
			}
		}
		return found == count;
	}

	public bool NonePressed() {
		for (int i = 0; i < pressedStates.Length; i++) {
			if (pressedStates[i]) { return false; }
		}
		return true;
	}

	public bool AllPressed() => !NonePressed();
}

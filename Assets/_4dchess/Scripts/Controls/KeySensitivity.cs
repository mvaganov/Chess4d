using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// sensitivity to Unity keys
/// </summary>
public class KeySensitivity : KeySensitivity_<KeyCode> { }

public class KeySensitivity_<KeyCodeType> : MonoBehaviour where KeyCodeType : struct, System.IConvertible {
	[SerializeField] protected string description;
	[SerializeField] protected KeyCodeType[] _keys = new KeyCodeType[1];
	[SerializeField] protected KeyEvent _onKeyDown = new KeyEvent();
	[SerializeField] protected KeyEvent _onKeyUp = new KeyEvent();
	[SerializeField] protected Trigger _trigger = Trigger.AnyPressedOrReleased;
	protected bool[] _pressedStates;

	public KeyCodeType[] Keys { get => _keys; set => _keys = value; }
	public KeyEvent OnKeyDown { get => _onKeyDown; set => _onKeyDown = value; }
	public KeyEvent OnKeyUp { get => _onKeyUp; set => _onKeyUp = value; }
	public Trigger TriggerSetting { get => _trigger; set => _trigger = value; }
	public enum Trigger { AnyPressedOrReleased, OnePressedAllReleased, AllPressedAllReleased, AllPressedAnyReleased }
	public bool HasKeyDownEvent => _onKeyDown.Actions.GetPersistentEventCount() != 0;
	public bool HasKeyUpEvent => _onKeyUp.Actions.GetPersistentEventCount() != 0;
	[System.Serializable] public class KeyEvent { public UnityEvent Actions; }

	protected virtual void Start() {
		_pressedStates = new bool[_keys.Length];
	}

	public virtual void Update() {
		UpdatePressedStates();
		CheckPress();
		CheckRelease();
	}

	protected void UpdatePressedStates() {
		for (int i = 0; i < _keys.Length; i++) {
			int kcode = _keys[i].ToInt32(System.Globalization.CultureInfo.CurrentCulture.NumberFormat);
			_pressedStates[i] = Input.GetKey((KeyCode)kcode);
		}
	}

	public void CheckPress() {
		bool doTrigger = IsPressed();
		//Debug.Log(name+" IsPressed " + _trigger + " " + doTrigger + " (" + string.Join(", ", _pressedStates) + ")");
		if (doTrigger) { OnKeyDown.Actions.Invoke(); }
	}

	public void CheckRelease() {
		bool doTrigger = IsUnpressed();
		if (doTrigger) { OnKeyUp.Actions.Invoke(); }
	}

	public bool IsPressed() {
		switch (_trigger) {
			case Trigger.OnePressedAllReleased: return PressCountExactly(1);
			case Trigger.AnyPressedOrReleased: return true;
			case Trigger.AllPressedAllReleased:
			case Trigger.AllPressedAnyReleased: return AllPressed();
		}
		return false;
	}

	public bool IsUnpressed() {
		switch (_trigger) {
			case Trigger.OnePressedAllReleased: return NonePressed();
			case Trigger.AnyPressedOrReleased: return true;
			case Trigger.AllPressedAllReleased: return NonePressed();
			case Trigger.AllPressedAnyReleased: return !AllPressed();
		}
		return false;
	}

	public bool PressCountExactly(int count) {
		int found = 0;
		for (int i = _pressedStates.Length-1; i >= 0; --i) {
			if (_pressedStates[i]) {
				++found;
				if (found > count) { return false; }
			}
		}
		return found == count;
	}

	public bool NonePressed() {
		for (int i = 0; i < _pressedStates.Length; i++) {
			if (_pressedStates[i]) { return false; }
		}
		return true;
	}

	public bool AllPressed() {
		for (int i = 0; i < _pressedStates.Length; i++) {
			if (!_pressedStates[i]) { return false; }
		}
		return true;
	}
}

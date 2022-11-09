using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// sensitivity to Unity keys
/// </summary>
public class KeySensitivity : MonoBehaviour {
	[SerializeField] protected string description;
	[SerializeField] protected KeyCode[] _keys = new KeyCode[1];
	[SerializeField] protected KeyEvent _onKeyDown = new KeyEvent();
	[SerializeField] protected KeyEvent _onKeyUp = new KeyEvent();
	[SerializeField] protected Trigger _trigger = Trigger.AnyPressedOrReleased;
	protected bool[] pressedStates;

	public KeyCode[] Keys { get => _keys; set => _keys = value; }
	public KeyEvent OnKeyDown { get => _onKeyDown; set => _onKeyDown = value; }
	public KeyEvent OnKeyUp { get => _onKeyUp; set => _onKeyUp = value; }
	public Trigger TriggerSetting { get => _trigger; set => _trigger = value; }
	public enum Trigger { AnyPressedOrReleased, OnePressedAllReleased, AllPressedAllReleased }
	public bool HasKeyDownEvent => _onKeyDown.Actions.GetPersistentEventCount() != 0;
	public bool HasKeyUpEvent => _onKeyUp.Actions.GetPersistentEventCount() != 0;
	[System.Serializable] public class KeyEvent { public UnityEvent Actions; }

	private void Start() {
		pressedStates = new bool[_keys.Length];
	}

	public virtual void Update() {
		CheckPress();
		CheckRelease();
	}

	public void CheckPress() {
		for (int i = 0; i < _keys.Length; i++) {
			if (!Input.GetKeyDown(_keys[i])) { continue; }
			pressedStates[i] = true;
			bool doTrigger = IsPressed();
			if (doTrigger) { OnKeyDown.Actions.Invoke(); }
		}
	}

	public void CheckRelease() {
		for (int i = 0; i < _keys.Length; i++) {
			if (!Input.GetKeyUp(_keys[i])) { continue; }
			pressedStates[i] = false;
			bool doTrigger = IsUnpressed();
			if (doTrigger) { OnKeyUp.Actions.Invoke(); }
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

using UnityEngine;
using UnityEngine.Events;

public class KeySensitivity : MonoBehaviour {
	[SerializeField] private KeyCode[] _keys = new KeyCode[1];
	[SerializeField] private KeyEvent _onKeyDown = new KeyEvent();
	[SerializeField] private KeyEvent _onKeyUp = new KeyEvent();
	[SerializeField] private Trigger _trigger = Trigger.AnyPressedOrReleased;
	private bool[] pressedStates;

	public KeyCode[] Keys { get => _keys; set => _keys = value; }
	public UnityEvent OnKeyDown => _onKeyDown.Actions;
	public UnityEvent OnKeyUp => _onKeyUp.Actions;

	public enum Trigger { AnyPressedOrReleased, OnePressedAllReleased, AllPressedAllReleased }

	[System.Serializable] public class KeyEvent { public UnityEvent Actions; }

	private void Start() {
		pressedStates = new bool[_keys.Length];
	}

	public void Update() {
		for(int i = 0; i < _keys.Length; i++) {
			if (Input.GetKeyDown(_keys[i])) {
				pressedStates[i] = true;
				bool doTrigger = false;
				switch (_trigger) {
					case Trigger.OnePressedAllReleased: doTrigger = PressCountExactly(1); break;
					case Trigger.AnyPressedOrReleased: doTrigger = true; break;
					case Trigger.AllPressedAllReleased: doTrigger = AllPressed(); break;
				}
				if (doTrigger) { OnKeyDown.Invoke(); }
			} else if (Input.GetKeyUp(_keys[i])) {
				pressedStates[i] = false;
				bool doTrigger = false;
				switch (_trigger) {
					case Trigger.OnePressedAllReleased: doTrigger = NonePressed(); break;
					case Trigger.AnyPressedOrReleased: doTrigger = true; break;
					case Trigger.AllPressedAllReleased: doTrigger = NonePressed(); break;
				}
				if (doTrigger) { OnKeyUp.Invoke(); }
			}
		}
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

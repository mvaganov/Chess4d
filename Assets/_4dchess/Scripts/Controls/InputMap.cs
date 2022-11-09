using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// maps keycodes to callback events, inspectable in the editor
/// </summary>
public class InputMap : MonoBehaviour {
	// TODO make these dictionaries of InputEventEntry for better debugging and reflection.
	public Dictionary<int, InputEventDelegate> inputDownEvents = new Dictionary<int, InputEventDelegate>();
	public Dictionary<int, InputEventDelegate> inputHoldEvents = new Dictionary<int, InputEventDelegate>();
	public Dictionary<int, InputEventDelegate> inputUpEvents = new Dictionary<int, InputEventDelegate>();
	public List<InputEventEntry> inputEventEntries = new List<InputEventEntry>();
	private bool _runningInGameLoop = false;
	private bool _inputManifestNeedsRefresh = false;
	[SerializeField] private bool _updateInputManifestOften = false;
	private List<Action> adjustmentsToMakeBetweenUpdates = new List<Action>();
	//private List<InputEventDelegate> pendingDelegates = new List<InputEventDelegate>();
	private InputEventDelegate mouseChangeListener;
	private float lastMouseChangeX, lastMouseChangeY;
	private bool[] trackingKeyAsPressed = new bool[510];
	private HashSet<KeyCode> beingPressed = new HashSet<KeyCode>();
	private List<KeyCode> beingReleased = new List<KeyCode>();

	public static Vector2 MouseChange => new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
	public static float MouseChangeX => Input.GetAxis("Mouse X");
	public static float MouseChangeY => Input.GetAxis("Mouse Y");

	public delegate void InputEventDelegate();

	[TextArea(1,50)]
	public string inputManifest;

	[System.Serializable] public class InputEventEntry {
		public string description;
		public KeyCode2 keyCode;
		public KeyPressState inputType;
		public InputEvent action;
		/// <summary>
		/// this will not be visiblie in the Unity editor
		/// </summary>
		public InputEventDelegate eventDelegate;
		public bool processed;
		public static int Comparer(InputEventEntry a, InputEventEntry b) {
			int diff = a.inputType.CompareTo(b.inputType);
			if (diff != 0) { return diff; }
			diff = a.keyCode.CompareTo(b.keyCode);
			if (diff != 0) { return diff; }
			diff = a.description.CompareTo(b.description);
			if (diff != 0) { return diff; }
			return 0;
		}
		public override string ToString() { return inputType + " " + keyCode + " " + description; }
	}
	
	public enum KeyPressState {
		None,         // 0000
		Press,        // 0001
		Release,      // 0010
		Hold,         // 0011
	}

	[System.Serializable] public class InputEvent : UnityEvent { }

	/// <summary>
	/// keys in the KeyCode listing that are not part of any sequence
	/// </summary>
	private static readonly KeyCode[] _singleKeys = new KeyCode[] {
		KeyCode.Backspace, KeyCode.Tab, KeyCode.Clear, KeyCode.Return, KeyCode.Pause, KeyCode.Escape,// 8, 9, 12, 13, 19, 27
		// 32 to 64 are valid (space, some special punctuation, numbers)
		// 91 to 122 are valid (more punctuation, alphabetic characters)
		KeyCode.Delete, // 127
		// 256 to 296 (numpad keys, arrow keys, function keys)
		// 300 to 319 (modifier keys)
		// 323 to 509 (mouse buttons, many joystick buttons)
	};

	private static readonly (int,int)[] _keyRanges = new (int,int)[] {
		(32,64), (91,122), (256,296), (300,319), (323,509),
	};

	private void Awake() {
		for(int i = 0; i < inputEventEntries.Count; ++i) {
			InputEventEntry entry = inputEventEntries[i];
			if (entry.processed) { continue; }
			Debug.Log(name + " " + i + " " + entry.keyCode + " " + ((int)entry.keyCode));
			int code = (int)entry.keyCode;
			InputEventDelegate inputDelegate = entry.action.Invoke;
			if (AddSpecialListener(code, inputDelegate)) {
				continue;
			}
			Debug.Log("adding to map");
			AddToMap(GetMap(entry.inputType), (int)entry.keyCode, entry.action.Invoke);
			entry.processed = true;
		}
		_inputManifestNeedsRefresh = true;
	}

	private Dictionary<int, InputEventDelegate> GetMap(KeyPressState inputType) {
		switch (inputType) {
			case KeyPressState.Press: return inputDownEvents;
			case KeyPressState.Hold: return inputHoldEvents;
			case KeyPressState.Release: return inputUpEvents;
		}
		return null;
	}

	private bool IsSpecialListenerCode(int keyCode)
		=> keyCode >= (int)KeyCode2.MouseChange && keyCode <= (int)KeyCode2.GazeChange;

	private bool AddSpecialListener(int keyCode, InputEventDelegate inputEvent) {
		switch (keyCode) {
			case (int)KeyCode2.MouseChange:
				mouseChangeListener += inputEvent;
				return true;
			case (int)KeyCode2.LeftControllerMove:
			case (int)KeyCode2.RightControllerMove:
			case (int)KeyCode2.HMDMove:
			case (int)KeyCode2.GazeChange:
				throw new Exception("TODO implement " + ((KeyCode2)keyCode).ToString());
			default:
				return false;
		}
	}

	private bool RemoveSpecialListener(int keyCode, InputEventDelegate inputEvent) {
		switch (keyCode) {
			case (int)KeyCode2.MouseChange: mouseChangeListener -= inputEvent; return true;
			case (int)KeyCode2.LeftControllerMove:
			case (int)KeyCode2.RightControllerMove:
			case (int)KeyCode2.HMDMove:
			case (int)KeyCode2.GazeChange:
				throw new Exception("TODO implement " + ((KeyCode2)keyCode).ToString());
		}
		return false;
	}

	private static void AddToMap(Dictionary<int, InputEventDelegate> map, int keyCode, InputEventDelegate inputEvent) {
		if (!map.TryGetValue(keyCode, out InputEventDelegate existingEvent)) {
			map[keyCode] = inputEvent;
			return;
		}
		existingEvent += inputEvent;
		map[keyCode] = existingEvent;
	}

	private static void RemoveFromMap(Dictionary<int, InputEventDelegate> map, int keyCode, InputEventDelegate inputEvent) {
		if (!map.TryGetValue(keyCode, out InputEventDelegate existingEvent)) {
			return;
		}
		existingEvent -= inputEvent;
		System.Delegate[] invocationList = existingEvent != null ? existingEvent.GetInvocationList() : null;
		if (invocationList == null || invocationList.Length == 0) {
			map.Remove(keyCode);
			return;
		}
		map[keyCode] = existingEvent;
	}

	public string AddEntry(int keyCode, KeyPressState inputType, InputEventDelegate inputEvent, string description = null) {
		if (description == null) {
			description = (inputEvent.Target != null ? inputEvent.Target.ToString() + "." : "") + inputEvent.Method.Name;
		}
		//Debug.Log(((KeyCode2)keyCode)+": "+(inputEvent.Target != null ? inputEvent.Target.ToString() + "." : "") + inputEvent.Method.Name);
		InputEventEntry entry = new InputEventEntry() {
			keyCode = (KeyCode2)keyCode, description = description, inputType = inputType, eventDelegate = inputEvent, processed = true
		};
		//Debug.Log("adding " + entry);
		inputEventEntries.Add(entry);
		_inputManifestNeedsRefresh = true;
		return description;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="keyCode"></param>
	/// <param name="eventType"></param>
	/// <param name="description">if null, will remove all keys bound this way</param>
	/// <returns></returns>
	private List<InputEventEntry> RemoveEntry(int keyCode, KeyPressState eventType, string description) {
		List<InputEventEntry> removed = new List<InputEventEntry>();
		for (int i = inputEventEntries.Count -1; i >= 0; --i) {
			InputEventEntry ie = inputEventEntries[i];
			if ((int)ie.keyCode == keyCode && ie.inputType == eventType && (description == null || ie.description == description)) {
				removed.Add(ie);
				inputEventEntries.RemoveAt(i);
				//Debug.Log("removed " + ie.description);
				_inputManifestNeedsRefresh = true;
			}
		}
		return removed;
	}

	public string Add(KeyCode2 keyCode, KeyPressState eventType, InputEventDelegate inputEvent, string description = null)
		=> Add((int)keyCode, eventType, inputEvent, description);

	public string Add(int keyCode, KeyPressState eventType, InputEventDelegate inputEvent, string description = null) {
		string desc = AddEntry(keyCode, eventType, inputEvent, description);
		if (!_runningInGameLoop) {
			if (!AddSpecialListener(keyCode, inputEvent)) {
				AddToMap(GetMap(eventType), keyCode, inputEvent);

				// TODO find out if this input event should be active right now.
				bool checkingHeld = eventType == KeyPressState.Press || eventType == KeyPressState.Hold;
				if (trackingKeyAsPressed[keyCode] == checkingHeld) {
					Debug.Log("pending " + eventType + ": (" + eventType + " " + ((KeyCode2)keyCode) + " " + description + ")");
					//pendingDelegates.Add(inputEvent);
					adjustmentsToMakeBetweenUpdates.Add(inputEvent.Invoke);
				}
			}
		} else {
			if (IsSpecialListenerCode(keyCode)) {
				adjustmentsToMakeBetweenUpdates.Add(() => AddSpecialListener(keyCode, inputEvent));
			} else {
				adjustmentsToMakeBetweenUpdates.Add(() => AddToMap(GetMap(eventType), keyCode, inputEvent));
			}
		}
		return desc;
	}

	public List<InputEventEntry> Remove(KeyCode2 keyCode, KeyPressState eventType, string description = null)
		=> Remove((int)keyCode, eventType, description);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="keyCode"></param>
	/// <param name="eventType"></param>
	/// <param name="description">if null, will remove all keys bound this way</param>
	/// <returns></returns>
	public List<InputEventEntry> Remove(int keyCode, KeyPressState eventType, string description = null) {
		List<InputEventEntry> removed = RemoveEntry(keyCode, eventType, description);
		if (removed.Count == 0) {
			throw new Exception("removed 0? "+((KeyCode2)keyCode)+" "+ eventType+" "+description);
		}
		for (int i = removed.Count - 1; i >= 0; --i) {
			InputEventEntry removedEntry = removed[i];
			if (!_runningInGameLoop) {
				RemoveFromMap(GetMap(eventType), keyCode, removedEntry.eventDelegate);
			} else {
				if (IsSpecialListenerCode(keyCode)) {
					RemoveSpecialListener(keyCode, removedEntry.eventDelegate);
				} else {
					adjustmentsToMakeBetweenUpdates.Add(() => RemoveFromMap(GetMap(eventType), keyCode, removedEntry.eventDelegate));
				}
			}
		}
		return removed;
	}

	/// <summary>
	/// list which keys are being pressed/held right now
	/// </summary>
	/// <returns></returns>
	public static List<KeyCode> GetKeyCodes() {
		List<KeyCode> keyList = new List<KeyCode>();
		GetKeyCodes(keyList);
		return keyList;
	}

	public static void GetKeyCodes(List<KeyCode> out_getKey) {
		for(int i = 0; i < _singleKeys.Length; ++i) {
			KeyCode key = _singleKeys[i];
			if (Input.GetKey(key)) { out_getKey.Add(key); }
		}
		foreach ((int,int) range in _keyRanges) {
			int start = range.Item1, end = range.Item2;
			for(int i = start; i <= end; ++i) {
				KeyCode key = (KeyCode)i;
				if (Input.GetKey(key)) { out_getKey.Add(key); }
			}
		}
	}

	void Update() {
		_runningInGameLoop = true;
		//InvokeDelayedDelegates();
		InvokePressEvents();
		InvokeHoldEvents();
		InvokeReleaseEvents();
		FallbackKeyRelease();
		ResolveMouseDeltaCallbacks();
		_runningInGameLoop = false;
		ResolveChangesMadeDuringUpdate();
		if (_updateInputManifestOften && _inputManifestNeedsRefresh) {
			UpdateInputManifest();
			_inputManifestNeedsRefresh = false;
		}
	}

	//private void InvokeDelayedDelegates() {
	//	if (pendingDelegates.Count == 0) { return; }
	//	pendingDelegates.ForEach(d => d.Invoke());
	//	pendingDelegates.Clear();
	//}

	private void InvokePressEvents() {
		foreach (var entry in inputDownEvents) {
			KeyCode k = (KeyCode)entry.Key;
			if (Input.GetKeyDown(k) || (!trackingKeyAsPressed[entry.Key] && Input.GetKey(k))) {
				//if (keyKnownToBePressed[entry.Key]) {
				//	Debug.LogWarning(((KeyCode)entry.Key) + " is this getting a double-press? ");
				//}
				DebugLogInputEvents(entry.Key, KeyPressState.Press);
				//Debug.Log(entry.Value.GetInvocationList().Length);
				
				entry.Value.Invoke();
				//adjustmentsToMakeBetweenUpdates.Add(entry.Value.Invoke);
				trackingKeyAsPressed[entry.Key] = true;
				beingPressed.Add(k);
			}
		}
	}

	private void DebugLogInputEvents(int keyCode, KeyPressState keyState) {
		List<InputEventEntry> triggered = InputEventFor(keyCode, keyState);
		int count = 0;
		if (GetMap(keyState).TryGetValue(keyCode, out InputEventDelegate d)) {
			count = d.GetInvocationList().Length;
		}
		Debug.Log("about to trigger: (" + string.Join("), (", triggered) + ") ("+count+")");
	}

	private void InvokeHoldEvents() {
		foreach (var entry in inputHoldEvents) {
			KeyCode k = (KeyCode)entry.Key;
			if (Input.GetKey(k)) {
				if (!trackingKeyAsPressed[entry.Key]) {
					if (inputDownEvents.TryGetValue(entry.Key, out InputEventDelegate shouldHaveBeenCalledSooner)) {
						DebugLogInputEvents(entry.Key, KeyPressState.Press);
						shouldHaveBeenCalledSooner.Invoke();
						//adjustmentsToMakeBetweenUpdates.Add(shouldHaveBeenCalledSooner.Invoke);
					}
					trackingKeyAsPressed[entry.Key] = true;
					beingPressed.Add(k);
				}
				DebugLogInputEvents(entry.Key, KeyPressState.Hold);
				entry.Value.Invoke();
				//adjustmentsToMakeBetweenUpdates.Add(entry.Value.Invoke);
			}
		}
	}

	private void InvokeReleaseEvents() {
		foreach (var entry in inputUpEvents) {
			KeyCode k = (KeyCode)entry.Key;
			if (Input.GetKeyUp(k)) {
				//if (!keyKnownToBePressed[entry.Key]) {
				//	Debug.LogWarning(((KeyCode)entry.Key) + " is this getting a double-release?");
				//}
				DebugLogInputEvents(entry.Key, KeyPressState.Release);
				entry.Value.Invoke();
				//adjustmentsToMakeBetweenUpdates.Add(entry.Value.Invoke);
				trackingKeyAsPressed[entry.Key] = false;
				beingPressed.Remove(k);
			}
		}
	}

	private void FallbackKeyRelease() {
		beingReleased.Clear();
		foreach (KeyCode pressed in beingPressed) {
			if (!Input.GetKey(pressed)) {
				if (inputUpEvents.TryGetValue((int)pressed, out InputEventDelegate shouldHaveBeenCalledSooner)) {
					DebugLogInputEvents((int)pressed, KeyPressState.Release);
					shouldHaveBeenCalledSooner.Invoke();
					//adjustmentsToMakeBetweenUpdates.Add(shouldHaveBeenCalledSooner.Invoke);
				}
				trackingKeyAsPressed[(int)pressed] = false;
				beingReleased.Add(pressed);
			}
		}
		foreach (KeyCode released in beingReleased) { beingPressed.Remove(released); }
	}

	private void UpdateInputManifest() {
		inputEventEntries.Sort(InputEventEntry.Comparer);
		StringBuilder sb = new StringBuilder();
		KeyPressState kpstate = KeyPressState.None;
		for (int i = 0; i < inputEventEntries.Count; i++) {
			InputEventEntry entry = inputEventEntries[i];
			if (kpstate != entry.inputType) {
				kpstate = entry.inputType;
				sb.Append($"[{kpstate}]\n");
			}
			sb.Append($"  {entry.keyCode}: {entry.description}\n");
		}
		inputManifest = sb.ToString();
	}

	private void ResolveMouseDeltaCallbacks() {
		float dx = MouseChangeX, dy = MouseChangeX;
		// also call mouseChangeListener if the mouse stopped moving and it was moving last frame
		if (dx != 0 || dy != 0 || lastMouseChangeX != dx || lastMouseChangeY != dy) {
			mouseChangeListener?.Invoke();
		}
		lastMouseChangeX = dx;
		lastMouseChangeY = dy;
	}

	public void ResolveChangesMadeDuringUpdate() {
		if (adjustmentsToMakeBetweenUpdates.Count == 0) { return; }
		adjustmentsToMakeBetweenUpdates.ForEach(a => a.Invoke());
		adjustmentsToMakeBetweenUpdates.Clear();
	}

	public string GetInputManifest() {
		if(_inputManifestNeedsRefresh) {
			UpdateInputManifest();
			_inputManifestNeedsRefresh = false;
		}
		return inputManifest;
	}

	public List<InputEventEntry> InputEventFor(int keyCode, KeyPressState keyState) {
		List<InputEventEntry> inputEvents = new List<InputEventEntry>();
		foreach (var entry in inputEventEntries) {
			if ((int)entry.keyCode == keyCode && entry.inputType == keyState) {
				inputEvents.Add(entry);
			}
		}
		return inputEvents;
	}
}

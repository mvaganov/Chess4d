using UnityEngine;
using UnityEngine.Events;

public class KeySensitivity : MonoBehaviour {
	[SerializeField] private KeyCode _key;
	[SerializeField] private KeyEvent _onKeyDown = new KeyEvent();
	[SerializeField] private KeyEvent _onKeyUp = new KeyEvent();

	public KeyCode Key { get => _key; set => _key = value; }
	public UnityEvent OnKeyDown => _onKeyDown.Actions;
	public UnityEvent OnKeyUp => _onKeyUp.Actions;

	[System.Serializable] public class KeyEvent { public UnityEvent Actions; }

	public void Update() {
		if (Input.GetKeyDown(Key)) {
			OnKeyDown.Invoke();
		} else if (Input.GetKeyUp(Key)) {
			OnKeyUp.Invoke();
		}
	}
}

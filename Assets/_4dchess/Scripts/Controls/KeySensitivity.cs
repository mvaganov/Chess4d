using UnityEngine;
using UnityEngine.Events;

public class KeySensitivity : MonoBehaviour {
	public KeyCode key;
	public UnityEvent OnKeyDown, OnKeyUp;
	public void Update() {
		if (Input.GetKeyDown(key)) {
			OnKeyDown.Invoke();
		} else if (Input.GetKeyUp(key)) {
			OnKeyUp.Invoke();
		}
	}
}

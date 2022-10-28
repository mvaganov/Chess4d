using UnityEngine;
using UnityEngine.Events;

public class OnEnableDisable : MonoBehaviour {
	public UnityEvent onEnable, onDisable;
	private void OnEnable() { onEnable.Invoke(); }
	private void OnDisable() { onDisable.Invoke(); }
}

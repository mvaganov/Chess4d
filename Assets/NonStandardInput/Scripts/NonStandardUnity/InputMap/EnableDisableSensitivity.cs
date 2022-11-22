using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnableDisableSensitivity : MonoBehaviour
{
	[SerializeField] private KeyEvent _onEnable = new KeyEvent();
	[SerializeField] private KeyEvent _onDisable = new KeyEvent();

	public UnityEvent EnableActions => _onEnable.Actions;
	public UnityEvent DisableActions => _onDisable.Actions;

	[System.Serializable] public class KeyEvent { public UnityEvent Actions; }

	private void OnEnable() {
		EnableActions.Invoke();
	}

	private void OnDisable() {
		DisableActions.Invoke();
	}
}

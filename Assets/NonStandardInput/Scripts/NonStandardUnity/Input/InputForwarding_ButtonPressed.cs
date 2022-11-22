using UnityEngine;
using UnityEngine.Events;
#if HAS_INPUTSYSTEM
using UnityEngine.InputSystem;
using CallbackContext = InputAction.CallbackContext;
#else
using CallbackContext = System.Boolean;
#endif

public class InputForwarding_ButtonPressed : MonoBehaviour {
	[System.Serializable] public class UnityEvent_bool : UnityEvent<bool> { }
	public UnityEvent_bool boolEvent;
	public void ForwardPressed(CallbackContext context) {
#if HAS_INPUTSYSTEM
	switch (context.phase) {
		case InputActionPhase.Started: boolEvent.Invoke(true); break;
		case InputActionPhase.Canceled: boolEvent.Invoke(false); break;
	}
#else
		boolEvent.Invoke(context);
#endif
	}
}

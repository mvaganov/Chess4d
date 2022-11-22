using UnityEngine;
using UnityEngine.Events;
#if USE_INPUTSYSTEM
using UnityEngine.InputSystem;
using CallbackContext = InputAction.CallbackContext;
#else
using CallbackContext = UnityEngine.Vector2;
#endif

public class InputForwarding_Vector2 : MonoBehaviour {
	[System.Serializable] public class UnityEvent_Vector2 : UnityEvent<Vector2> { }
	public UnityEvent_Vector2 vector2Event;
	public void ForwardVector2(CallbackContext context) {
#if USE_INPUTSYSTEM
		vector2Event.Invoke(context.ReadValue<Vector2>());
#else
		vector2Event.Invoke(context);
#endif
	}
}

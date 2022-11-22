using UnityEngine;
using UnityEngine.EventSystems;
#if USE_INPUTSYSTEM
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
#endif

namespace NonStandard.Inputs {
	public class OnScreenStickAnimated :
#if USE_INPUTSYSTEM
		OnScreenControl,
#else
		MonoBehaviour,
#endif
		IPointerDownHandler, IPointerUpHandler, IDragHandler {
		public void OnPointerDown(PointerEventData eventData) {
			if (eventData == null)
				throw new System.ArgumentNullException(nameof(eventData));
			RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out m_PointerDownPos);
			held = true;
		}

		public void OnDrag(PointerEventData eventData) {
			if (eventData == null)
				throw new System.ArgumentNullException(nameof(eventData));
			RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out var position);
			var delta = position - m_PointerDownPos;
			switch (axisLock) {
				case AxisLock.XAxis: delta.x = 0; break;
				case AxisLock.YAxis: delta.y = 0; break;
			}
			delta = Vector2.ClampMagnitude(delta, movementRange);
			((RectTransform)transform).anchoredPosition = m_StartPos + (Vector3)delta;
			inputPosition = new Vector2(delta.x / movementRange, delta.y / movementRange);
#if USE_INPUTSYSTEM
			SendValueToControl(inputPosition);
#else
			// TODO send input to InputMap?
#endif
			held = true;
		}

		public void OnPointerUp(PointerEventData eventData) {
			((RectTransform)transform).anchoredPosition = m_StartPos;
#if USE_INPUTSYSTEM
			SendValueToControl(Vector2.zero);
#else
			// TODO send input to InputMap?
#endif
			held = false;
		}

		public float movementRange {
			get => m_MovementRange;
			set => m_MovementRange = value;
		}

		[SerializeField]
		private float m_MovementRange = 50;

#if USE_INPUTSYSTEM
		[InputControl(layout = "Vector2")]
#endif
		[SerializeField]
		private string m_ControlPath;

		private Vector3 m_StartPos;
		private Vector2 m_PointerDownPos;

#if USE_INPUTSYSTEM
		protected override string controlPathInternal {
			get => m_ControlPath;
			set => m_ControlPath = value;
		}
#endif

		RectTransform rt;
		public float stickAnimationSpeed = 1024;
		Vector2 targetPosition;
		Vector2 inputPosition;
		public AxisLock axisLock;
		bool held = false;
		public bool executeOnHeld = false;
		public float holdMultiplier = 1;
		public enum AxisLock { None, XAxis, YAxis }

		public bool Held => held;
		private void Start() {
			m_StartPos = ((RectTransform)transform).anchoredPosition;
			rt = GetComponent<RectTransform>();
		}
		public void SetStickPositionFrominput(Vector2 input) {
			switch (axisLock) {
				case AxisLock.XAxis: input.x = 0; break;
				case AxisLock.YAxis: input.y = 0; break;
			}
			targetPosition = input.normalized * movementRange;
		}
		private int waitForZeroTill = 0;
		public int msToWaitBeforeZeroing = 100;
		private void Update() {
			if (held) return;
			if (targetPosition == Vector2.zero) {
				if (System.Environment.TickCount < waitForZeroTill) { return; }
			} else {
				waitForZeroTill = System.Environment.TickCount + msToWaitBeforeZeroing;
			}
			Vector2 d = targetPosition - rt.anchoredPosition;
			float distance = d.magnitude;
			if (distance > stickAnimationSpeed * Time.deltaTime) {
				Vector2 normalized = d / distance;
				rt.anchoredPosition += normalized * stickAnimationSpeed * Time.deltaTime;
			} else {
				rt.anchoredPosition = targetPosition;
			}
		}
		private void FixedUpdate() {
			if (executeOnHeld && held) {
#if USE_INPUTSYSTEM
				SendValueToControl(inputPosition * holdMultiplier);
#else
				// TODO send input to InputMap?
#endif
			}
		}
	}
}

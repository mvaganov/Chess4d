using UnityEngine;

public class AxisMovement : MonoBehaviour {
	public float speed = 5;
	public Transform body;
	public bool useInputMap = false;
	private Rigidbody rb;
	private Vector3 inputMapDirection;
	private InputMap _inputMap;

	public KeyAxis[] axis = new KeyAxis[] {
		new KeyAxis("Horizontal", KeyCode.D, KeyCode.A, 1),
		new KeyAxis("Altitude", KeyCode.Q, KeyCode.E, 1),
		new KeyAxis("Advance", KeyCode.W, KeyCode.S, 1),
	};

	[System.Serializable] public struct KeyAxis {
		public string name;
		public KeyCode[] _increase, _decrease;
		public float baseValue;
		public KeyAxis(string name, KeyCode increase, KeyCode decrease, float baseValue) {
			this.name = name;
			this.baseValue = baseValue;
			this._increase = new KeyCode[] { increase };
			this._decrease = new KeyCode[] { decrease };
		}
		public float Value {
			get {
				for (int i = 0; i < _increase.Length; ++i) { if (Input.GetKey(_increase[i])) { return baseValue; } }
				for (int i = 0; i < _decrease.Length; ++i) { if (Input.GetKey(_decrease[i])) { return -baseValue; } }
				return 0;
			}
		}
	}

	private string GetPress(int axisId) { return axis[axisId].name + " press"; }
	private string GetRelease(int axisId) { return axis[axisId].name + " release"; }
	private string GetIncreasePress(int axisId) => "+" + GetPress(axisId);
	private string GetIncreaseRelease(int axisId) => "+" + GetPress(axisId);
	private string GetDecreasePress(int axisId) => "-" + GetRelease(axisId);
	private string GetDecreaseRelease(int axisId) => "-" + GetRelease(axisId);

	private void OnEnable() {
		if (useInputMap && _inputMap == null) {
			_inputMap = FindObjectOfType<InputMap>();
		}
		if (!useInputMap || _inputMap == null) { return; }
		for (int a = 0; a < axis.Length; ++a) {
			KeyAxis ax = axis[a];
			for(int i = 0; i < ax._increase.Length; ++i) {
				_inputMap.Add((KeyCode2)ax._increase[i], InputMap.KeyPressState.Down, CheckValue[a], GetIncreasePress(a));
				_inputMap.Add((KeyCode2)ax._increase[i], InputMap.KeyPressState.Up, CheckValue[a], GetIncreaseRelease(a));
			}
			for (int i = 0; i < ax._decrease.Length; ++i) {
				_inputMap.Add((KeyCode2)ax._decrease[i], InputMap.KeyPressState.Down, CheckValue[a], GetDecreasePress(a));
				_inputMap.Add((KeyCode2)ax._decrease[i], InputMap.KeyPressState.Up, CheckValue[a], GetDecreaseRelease(a));
			}
		}
		inputMapDirection = new Vector3(axis[0].Value, axis[1].Value, axis[2].Value);
	}
	private void OnDisable() {
		if (!useInputMap || _inputMap == null) { return; }
		for (int a = 0; a < axis.Length; ++a) {
			KeyAxis ax = axis[a];
			for (int i = 0; i < ax._increase.Length; ++i) {
				_inputMap.Remove((KeyCode2)ax._increase[i], InputMap.KeyPressState.Down, GetIncreasePress(a));
				_inputMap.Remove((KeyCode2)ax._increase[i], InputMap.KeyPressState.Up, GetIncreaseRelease(a));
			}
			for (int i = 0; i < ax._decrease.Length; ++i) {
				_inputMap.Remove((KeyCode2)ax._decrease[i], InputMap.KeyPressState.Down, GetDecreasePress(a));
				_inputMap.Remove((KeyCode2)ax._decrease[i], InputMap.KeyPressState.Up, GetDecreaseRelease(a));
			}
		}
	}
	private InputMap.InputEventDelegate[] CheckValue => _IncreaseEvents != null ? _IncreaseEvents : _IncreaseEvents =
		new InputMap.InputEventDelegate[] { ValueX, ValueY, ValueZ };
	private InputMap.InputEventDelegate[] _IncreaseEvents = null;
	private void ValueX() { inputMapDirection[0] = axis[0].Value; }
	private void ValueY() { inputMapDirection[1] = axis[1].Value; }
	private void ValueZ() { inputMapDirection[2] = axis[2].Value; }

	private void Start() {
		rb = body.GetComponent<Rigidbody>();
	}

	public void LateUpdate() {
		Vector3 delta = (useInputMap && _inputMap != null) ? inputMapDirection :
			new Vector3(axis[0].Value, axis[1].Value, axis[2].Value);
		if (delta != Vector3.zero) {
			Vector3 absoluteSpaceDelta = body.TransformDirection(delta);
			if (rb != null) {
				rb.velocity = absoluteSpaceDelta * speed;
			} else {
				body.transform.position += absoluteSpaceDelta * (Time.deltaTime * speed);
			}
		}
	}

	/// <summary>
	/// referenced by Unity callbacks
	/// </summary>
	/// <param name="t"></param>
	public void CopyTransform(Transform t) {
		body.position = t.position;
	}
}

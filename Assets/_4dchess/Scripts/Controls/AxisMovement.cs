using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisMovement : MonoBehaviour {
	public float speed = 5;
	public Transform body;
	private Rigidbody rb;

	public KeyAxis[] axis = new KeyAxis[] {
		new KeyAxis("Horizontal", KeyCode.D, KeyCode.A, 1),
		new KeyAxis("Altitude", KeyCode.Q, KeyCode.E, 1),
		new KeyAxis("Advance", KeyCode.W, KeyCode.S, 1),
	};
	
	[System.Serializable] public struct KeyAxis {
		public string name;
		public KeyCode increase, decrease;
		public float baseValue;
		public KeyAxis(string name, KeyCode increase, KeyCode decrease, float baseValue) {
			this.name = name;
			this.increase = increase;
			this.decrease = decrease;
			this.baseValue = baseValue;
		}
		public float Value {
			get {
				if (Input.GetKey(increase)) { return baseValue; }
				if (Input.GetKey(decrease)) { return -baseValue; }
				return -0;
			}
		}
	}

	private void Start() {
		rb = body.GetComponent<Rigidbody>();
	}

	public void LateUpdate() {
		Vector3 delta = new Vector3(axis[0].Value, axis[1].Value, axis[2].Value);
		if (delta != Vector3.zero) {
			Vector3 absoluteSpaceDelta = body.TransformDirection(delta);
			if (rb != null) {
				rb.velocity = absoluteSpaceDelta * speed;
			} else {
				body.transform.position += absoluteSpaceDelta * (Time.deltaTime * speed);
			}
		}
	}
}

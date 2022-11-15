using NonStandard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireTest : MonoBehaviour {
	// Start is called before the first frame update
	void Start() {
	}

	// Update is called once per frame
	void Update() {
		Wires.Make("wiretest0").Arrow(Vector3.zero + Vector3.up * 2, Vector3.one + Vector3.up * 2, Color.black);
	}
}

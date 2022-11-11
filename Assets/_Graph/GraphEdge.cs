using NonStandard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class GraphEdge {
	public GraphNode from, to;
	public float cost;

	public Wire wire;

	public GraphEdge(GraphNode from, GraphNode to) {
		this.from = from;
		this.to = to;
		cost = Vector3.Distance(from.transform.position, to.transform.position);
		Init();
	}

	protected void Init() {
		wire = Wires.Make("from " + from.name + " to " + to.name);
		wire.transform.SetParent(from.transform, false);
	}

	public void Update() {
		Vector3 delta = to.transform.position - from.transform.position;
		Vector3 dir = delta.normalized;
		Vector3 start = from.transform.position + dir * from.radius;
		Vector3 end = to.transform.position - dir * to.radius;
		wire.Arrow(start, end, Color.black);
	}

	public void Clear() {
		GameObject.Destroy(wire);
	}
}

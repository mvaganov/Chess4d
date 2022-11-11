using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode : MonoBehaviour {
	public List<GraphEdge> graphEdges = new List<GraphEdge>();
	public float radius = 1;
	public float avgEdgeDistance = 0;

	public void AddNeighbor(GraphNode node) {
		GraphEdge e = new GraphEdge(this, node);
		graphEdges.Add(e);
	}

	public void Update() {
		graphEdges.ForEach(e => e.Update());
		Vector3 avgLocation = Vector3.zero;
		int count = 0;
		graphEdges.ForEach((edge) => {
			float d = Vector3.Distance(transform.position, edge.to.transform.position);
			if (d < (radius + edge.to.radius + 1)) { return; }
			count++;
			avgLocation += edge.to.transform.position;
			avgEdgeDistance += edge.cost;
		});
		avgLocation /= count;
		avgEdgeDistance /= count;
		Rigidbody rigidbody = GetComponent<Rigidbody>();
		Vector3 delta = avgLocation - transform.position;
		float distance = delta.magnitude;
		Vector3 dir = delta / distance;
		rigidbody.velocity = dir;
		for(int i = graphEdges.Count - 1; i >= 0; i--) {
			GraphEdge e = graphEdges[i];
			if (e.cost > avgEdgeDistance * 1.5f) {
				e.Clear();
				graphEdges.RemoveAt(i);
			}
		}
	}
}

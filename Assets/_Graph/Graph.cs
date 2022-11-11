using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour {
	public int count;
	public float radius = 10;

	public GraphNode nodePrefab;

	public List<GraphNode> nodes = new List<GraphNode>();

	public int neighborCount = 3;

	public void Start() {
		for(int i = 0; i < count; ++i) {
			GraphNode gn = Instantiate(nodePrefab.gameObject).GetComponent<GraphNode>();
			gn.name = "node " + i;
			nodes.Add(gn);
			gn.transform.position = Random.insideUnitSphere * radius;
		}

		for (int i = 0; i < nodes.Count; ++i) {
			//List<int> connections = RandomNodes(i, neighborCount);
			List<int> connections = ClosestNodes(i, neighborCount);
			for (int n = 0; n < connections.Count; ++n) {
				int neighborIndex = connections[n];
				if (neighborIndex == n) { continue; }
				nodes[i].AddNeighbor(nodes[neighborIndex]);
			}
		}
	}

	private List<int> RandomNodes(int index, int count) {
		List<int> results = new List<int>();
		for (int i = 0; i < count; ++i) {
			int connectIndex = Random.Range(0, nodes.Count-1);
			if (connectIndex >= index) {
				connectIndex++;
			}
			results.Add(connectIndex);
		}
		return results;
	}

	private struct Vector3ByDistance {
		public Vector3 position;
		public float distance;
		public int nodeIndex;
		public Vector3ByDistance(int nodeIndex, Vector3 position, float distance) {
			this.nodeIndex = nodeIndex;
			this.position = position;
			this.distance = distance;
		}
	}

	private List<int> ClosestNodes(int index, int count) {
		List<int> results = new List<int>();
		List<Vector3ByDistance> nodesByDistance = new List<Vector3ByDistance>();
		Vector3 here = nodes[index].transform.position;
		for (int i = 0; i < nodes.Count; ++i) {
			Vector3 there = nodes[i].transform.position;
			nodesByDistance.Add(new Vector3ByDistance(i, there, Vector3.Distance(here, there)));
		}
		for (int sortIndex = 0; sortIndex < count + 1; ++sortIndex) {
			float best = nodesByDistance[sortIndex].distance;
			for(int i = sortIndex + 1; i < nodes.Count; ++i) {
				float d = nodesByDistance[i].distance;
				if (d < best) {
					Vector3ByDistance temp = nodesByDistance[sortIndex];
					nodesByDistance[sortIndex] = nodesByDistance[i];
					nodesByDistance[i] = temp;
				}
			}
			results.Add(nodesByDistance[sortIndex].nodeIndex);
		}
		results.RemoveAt(0); // remove self from list
		return results;
	}
}

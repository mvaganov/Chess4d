using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class MemoryPool<T> where T : MonoBehaviour {
	public Transform unallocatedLocation;
	public MonoBehaviour prefab;
	public List<T> unallocated = new List<T>();
	public Action<T> onReclaim;
	public Action<T> onInitialize;

	public MemoryPool() {
	}
	public void SetData(Transform unallocatedLocation, T prefab) {
		this.unallocatedLocation = unallocatedLocation;
		this.prefab = prefab;
	}
	public T Get() {
		T thing;
		if (unallocated.Count > 0) {
			thing = unallocated[unallocated.Count - 1];
			unallocated.RemoveAt(unallocated.Count - 1);
			thing.gameObject.SetActive(true);
			onInitialize?.Invoke(thing);
		} else {
			thing = ChessGame.CreateObject(prefab.gameObject).GetComponent<T>();
			if (unallocatedLocation != null) {
				thing.transform.SetParent(null, false);
			}
			thing.gameObject.SetActive(true);
		}
		onInitialize?.Invoke(thing);
		return thing;
	}
	public void Reclaim(T thing) {
		onReclaim?.Invoke(thing);
		thing.gameObject.SetActive(false);
		if (unallocatedLocation != null) {
			thing.transform.SetParent(unallocatedLocation, false);
		}
		unallocated.Add(thing);
	}
}

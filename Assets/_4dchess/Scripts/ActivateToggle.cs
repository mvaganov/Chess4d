using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActivateToggle : MonoBehaviour {
	public List<Set> ToggleSets = new List<Set> ();
	public int _index = 0;

	public int Index {
		get => _index;
		set => _index = value;
	}

	[System.Serializable] public class Set {
		bool active;
		public string name;
		public List<GameObject> objects;
		public UnityEvent OnActivate;
		public UnityEvent OnDeactivate;
		public void SetActive(bool active) {
			bool activate = active && !this.active;
			bool deactivate = !active && this.active;
			this.active = active;
			if (activate) { OnActivate.Invoke(); Debug.Log("activated " + name); }
			if (deactivate) { OnDeactivate.Invoke(); Debug.Log("deactivated "+name); }
			objects.ForEach(o => o.SetActive(active));
		}
	}

	private void Start() {
		Refresh();
	}

	public void Refresh() {
		for (int i = 0; i < ToggleSets.Count; i++) {
			ToggleSets[i].SetActive(i == _index);
		}
	}

	public void Next() {
		if(++_index >= ToggleSets.Count) { _index = 0; }
		Refresh();
	}

	public void Prev() {
		if (--_index < 0) { _index = ToggleSets.Count-1; }
		Refresh();
	}
}

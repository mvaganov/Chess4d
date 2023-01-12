using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Toggle : MonoBehaviour {
	[System.Serializable] public class Option {
		public string name;
		public UnityEvent action;
	}
	public List<Option> Options = new List<Option>();
	public int Index = 0;
	private void Start() { Refresh(); }
	public void Refresh() { Options[Index].action.Invoke(); }
	public void DoToggle() {
		SetOption(Index + 1);
		Refresh();
	}
	public void SetOption(int index) {
		if ((Index = index) >= Options.Count || Index < 0) { Index = 0; }
		Refresh();
	}
}

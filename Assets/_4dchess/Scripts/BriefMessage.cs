using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BriefMessage : MonoBehaviour {
	[SerializeField] private TMPro.TMP_Text _text;
	[SerializeField] private float _duration = 5;
	[SerializeField] private Color _fullColor = Color.white;
	[SerializeField] private Color _fadedColor = Color.clear;
	private float _timer;

	public string Text {
		get { return _text.text; }
		set {
			_text.text = value;
			_text.color = _fullColor;
			_timer = _duration;
			enabled = true;
		}
	}

	private void Update() {
		_timer -= Time.deltaTime;
		if (_timer >= 1) {
			return;
		}
		if (_timer < 0) {
			enabled = false;
			_timer = 0;
			_text.color = _fadedColor;
			return;
		}
		_text.color = Color.Lerp(_fadedColor, _fullColor, _timer);
	}
}

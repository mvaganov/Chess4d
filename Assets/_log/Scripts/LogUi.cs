using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LogUi : MonoBehaviour {
	[SerializeField] private LogMessage _prefabLogMessage;
	[SerializeField] private RectTransform _messageList;
	[SerializeField] private ScrollRect _scrollRect;
	private Dictionary<LogType, MessageColor> _colorDict = new Dictionary<LogType, MessageColor>();

	[SerializeField] private MessageColor[] _messageColors = new MessageColor[] {
		new MessageColor(LogType.Log, new Color(.875f,.875f,.875f)),
		new MessageColor(LogType.Error, Color.red),
		new MessageColor(LogType.Exception, Color.magenta),
		new MessageColor(LogType.Warning, Color.yellow),
		new MessageColor(LogType.Assert, new Color (1,.5f,0)),
	};

	[System.Serializable] public struct MessageColor {
		public LogType type; public Color color;
		public MessageColor(LogType type, Color color) { this.type = type; this.color = color; }
	}

	[System.Serializable] public class StringCallback : UnityEvent<string,string> { }

	private void Awake() {
		foreach(MessageColor mc in _messageColors) { _colorDict[mc.type] = mc; }
	}

	private void Start() {
		Application.logMessageReceived += LogReceived;
	}

	public void LogReceived(string text, string stackTrace, LogType type) {
		LogMessage msg = Instantiate(_prefabLogMessage.gameObject).GetComponent<LogMessage>();
		msg.gameObject.SetActive(true);
		msg.Text.text = text;
		msg.Text.color = _colorDict[type].color;
		msg.Source.text = stackTrace;
		msg.transform.SetParent(_messageList);
		if (_scrollRect != null) {
			StartCoroutine(ScrollToBottomCoroutine());
		}
	}
	private IEnumerator ScrollToBottomCoroutine() {
		yield return null;
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(_messageList.GetComponent<RectTransform>());
		_scrollRect.verticalScrollbar.value = 0;
	}
}

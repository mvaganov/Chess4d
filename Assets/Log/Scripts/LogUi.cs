using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LogUi : MonoBehaviour {
	[SerializeField] private LogMessage _prefabLogMessage;
	[SerializeField] private RectTransform _messageList;
	[SerializeField] private ScrollRect _scrollRect;
	[SerializeField] private bool _autoScrollToEnd = true;
	[ContextMenuItem(nameof(RebuildUi),nameof(RebuildUi))]
	[SerializeField] private bool _collapseDuplicates = true;
	private int _countMessages;
	private long _timeStart;
	const float ScrollBarEpsilon = 1f / 1024;
	private Dictionary<LogType, MessageColor> _colorDict = new Dictionary<LogType, MessageColor>();
	private DictionaryTree<LogType, DictionaryTree<string, DictionaryTree<string, LogMessageData>>> _messageTree =
		new DictionaryTree<LogType, DictionaryTree<string, DictionaryTree<string, LogMessageData>>>();

	[SerializeField]
	private MessageColor[] _messageColors = new MessageColor[] {
		new MessageColor(LogType.Log, new Color(.875f,.875f,.875f)),
		new MessageColor(LogType.Error, Color.red),
		new MessageColor(LogType.Exception, Color.magenta),
		new MessageColor(LogType.Warning, Color.yellow),
		new MessageColor(LogType.Assert, new Color (1,.5f,0)),
	};

	public long TimeStart => _timeStart;
	public bool CollapseDuplicates {
		get => _collapseDuplicates;
		set {
			_collapseDuplicates = value;
			RebuildUi();
		}
	}

	[System.Serializable]
	public struct MessageColor {
		public LogType type; public Color color;
		public MessageColor(LogType type, Color color) { this.type = type; this.color = color; }
	}

	[System.Serializable] public class StringCallback : UnityEvent<string, string> { }

	private void Awake() {
		foreach (MessageColor mc in _messageColors) { _colorDict[mc.type] = mc; }
		Initialize();
	}

	private void Initialize() {
		_countMessages = 0;
		_timeStart = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
	}

	private void Start() {
		Application.logMessageReceived += LogReceived;
	}

	private void OnEnable() {
		UpdateScrollableUi();
	}

	private void UpdateScrollableUi() {
		StartCoroutine(UpdateMessageContentAreaForScrollBars());
	}

	private IEnumerator UpdateMessageContentAreaForScrollBars() {
		bool wasAtTheBottomAlready = _scrollRect.verticalNormalizedPosition < ScrollBarEpsilon;
		yield return null; // wait for the new children to register in the UI
		LayoutRebuilder.ForceRebuildLayoutImmediate(_messageList.GetComponent<RectTransform>());
		yield return null; // wait for the UI to recalculate
		if (_autoScrollToEnd && wasAtTheBottomAlready) { _scrollRect.verticalScrollbar.value = 0; }
	}

	public void LogReceived(string text, string stackTrace, LogType type) {
		object[] uniqueLogPath = new object[] { type, stackTrace == null ? "" : stackTrace, text == null ? "" : text };
		LogMessageData messageData = null;
		bool isDuplicate = _messageTree != null && _messageTree.TryGet(uniqueLogPath, out messageData);
		if (isDuplicate) {
			messageData.Instances.First.Value.Ui.AddInstance(this, _countMessages++, _collapseDuplicates);
		}
		if (isDuplicate && _collapseDuplicates) { return; }
		if (!isDuplicate) {
			messageData = new LogMessageData(type, text, stackTrace);
		}
		LogMessage msg = ForceAddMessageUi(messageData);
		if (_collapseDuplicates) { msg.SetCountLabel(messageData.Count); }
		if (!isDuplicate) {
			msg.AddInstance(this, _countMessages++, _collapseDuplicates);
			_messageTree?.Set(uniqueLogPath, messageData);
		}
		if (gameObject.activeSelf) {
			UpdateScrollableUi();
		}
	}

	public LogMessage ForceAddMessageUi(LogMessageData data, bool autoSetParent = true, bool showDuplicateCount = true) {
		LogMessage msg = Instantiate(_prefabLogMessage.gameObject).GetComponent<LogMessage>();
		msg.gameObject.SetActive(true);
		msg.Data = data;
		msg.MessageLabel.color = _colorDict[data.LogType].color;
		if (autoSetParent) {
			msg.transform.SetParent(_messageList, false);
		}
		return msg;
	}

	public void ToggleDuplicateMessageCollapse() {
		_collapseDuplicates = !_collapseDuplicates;
		RebuildUi();
	}

	public void RebuildUi() {
		if (_messageTree == null) {
			Debug.LogWarning($"need {nameof(_messageTree)} to rebuild message list");
			return;
		}
		if (_collapseDuplicates) {
			RebuildUiCollapsed();
		} else {
			RebuildUiUncollapsed();
		}
	}

	public void ClearMessageUi() {
		Transform prefabTransform = _prefabLogMessage.transform;
		for(int i = _messageList.childCount-1; i >= 0; --i) {
			Transform t = _messageList.GetChild(i).transform;
			if (t == prefabTransform) { continue; }
			if (Application.isPlaying) {
				Destroy(t.gameObject);
			} else {
				DestroyImmediate(t.gameObject);
			}
		}
	}

	public void DoUiCollapsed() => CollapseDuplicates = true;
	public void DoUiUncollapsed() => CollapseDuplicates = false;
	private void RebuildUiCollapsed() {
		List<LogMessageData> messagesInOrder = new List<LogMessageData>();
		foreach (LogMessageData messageData in _messageTree) {
			int index = messagesInOrder.BinarySearch(messageData, LogMessageData.Comparer);
			if (index < 0) { index = ~index; }
			messagesInOrder.Insert(index, messageData);
		}
		ClearMessageUi();
		foreach (LogMessageData messageData in messagesInOrder) {
			LogMessage msgUi = ForceAddMessageUi(messageData);
			LinkedListNode<LogMessageInstance> cursor = messageData.Instances.First;
			while(cursor != null) {
				cursor.Value.Ui = msgUi;
				cursor = cursor.Next;
			}
			msgUi.SetCountLabel(messageData.Count);
		}
	}

	private void RebuildUiUncollapsed() {
		List<LogMessageInstance> messagesInOrder = new List<LogMessageInstance>(_countMessages);
		for(int i = 0; i < _countMessages; ++i) { messagesInOrder.Add(null); }
		foreach (LogMessageData messageData in _messageTree) {
			LinkedListNode<LogMessageInstance> cursor = messageData.Instances.First;
			while (cursor != null) {
				messagesInOrder[cursor.Value.Index] = cursor.Value;
				LogMessage msg = ForceAddMessageUi(messageData, false);
				cursor.Value.Ui = msg;
				cursor = cursor.Next;
			}
		}
		ClearMessageUi();
		for (int i = 0; i < messagesInOrder.Count; ++i) {
			LogMessageInstance messageInstance = messagesInOrder[i];
			messageInstance.Ui.transform.SetParent(_messageList, false);
		}
	}

	public void Clear() {
		if (_messageTree != null) { _messageTree.Clear(); }
		ClearMessageUi();
		Initialize();
	}
}

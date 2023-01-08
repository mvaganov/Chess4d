using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LogMessage : MonoBehaviour, IPointerClickHandler {
	[SerializeField] private TMPro.TMP_Text _messageLabel;
	[SerializeField] private TMPro.TMP_Text _detailLabel;
	[SerializeField] private TMPro.TMP_Text _countLabel;
	private LogMessageData _data;
	public TMPro.TMP_Text MessageLabel => _messageLabel;
	public TMPro.TMP_Text DetailLabel => _detailLabel;
	public TMPro.TMP_Text CountLabel => _countLabel;
	public LogMessageData Data {
		get => _data;
		set {
			_data = value;
			if (_messageLabel != null) { _messageLabel.text = _data.Message; }
			if (_detailLabel != null) { _detailLabel.text = _data.StackTrace; }
			//if (_countLabel != null) { SetCountLabel(_data.Count); }
		}
	}
	public void SetCountLabel(int count) {
		if (_countLabel == null) { return; }
		_countLabel.gameObject.SetActive(count < 0 || count > 1);
		_countLabel.text = count.ToString();
	}
	public void SetCountLabel(string countText) {
		if (_countLabel == null) { return; }
		_countLabel.gameObject.SetActive(true);
		_countLabel.text = countText;
	}
	public LogMessageInstance AddInstance(LogUi log, int index, bool updateCountLabel) {
		long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		int time = (int)(now - log.TimeStart);
		LogMessageInstance instance = new LogMessageInstance(this, time, index);
		_data.Add(instance);
		if (updateCountLabel) {
			SetCountLabel(_data.Count);
		}
		if (gameObject.activeInHierarchy && _data.Count > 1) {
			StartCoroutine(ForceUiUpdate(transform));
		}
		return instance;
	}
	/// <summary>
	/// called by Unity UI
	/// </summary>
	public void OnPointerClick(PointerEventData eventData) {
		if (_detailLabel != null) {
			_detailLabel.gameObject.SetActive(!_detailLabel.gameObject.activeSelf);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
	}
	public static IEnumerator ForceUiUpdate(Transform t) {
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(t as RectTransform);
	}
}

public class LogMessageData {
	public readonly LogType LogType;
	public readonly string Message;
	public readonly string StackTrace;
	private LinkedList<LogMessageInstance> _instances;
	public LinkedList<LogMessageInstance> Instances => _instances;
	public int Count => _instances != null ? _instances.Count : 0;
	public LogMessageData(LogType type, string message, string stackTrace) {
		LogType = type; Message = message; StackTrace = stackTrace;
	}
	public void Add(LogMessageInstance instance) {
		if (_instances == null) {
			_instances = new LinkedList<LogMessageInstance>();
		}
		_instances.AddLast(instance);
	}
	public override string ToString() => LogType + "#" + _instances.Count + ":" + Message;
	public class LogMessageComparer : IComparer<LogMessageData> {
		public int Compare(LogMessageData a, LogMessageData b) {
			if (a.Instances.Count == 0 && b.Instances.Count == 0) { return 0; }
			if (a.Instances.Count == 0) { return -1; }
			if (b.Instances.Count == 0) { return 1; }
			return a.Instances.First.Value.Index.CompareTo(b.Instances.First.Value.Index);
		}
	}
	public static readonly LogMessageComparer Comparer = new LogMessageComparer();
}

public class LogMessageInstance {
	public LogMessage Ui;
	public readonly int Timestamp;
	public readonly int Index;
	public LogMessageInstance(LogMessage ui, int timestamp, int index) {
		Ui = ui; Timestamp = timestamp; Index = index;
	}
}

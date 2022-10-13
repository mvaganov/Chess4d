using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LogMessage : MonoBehaviour, IPointerClickHandler {
	[SerializeField] private TMPro.TMP_Text _text;
	[SerializeField] private TMPro.TMP_Text _source;

	public TMPro.TMP_Text Text => _text;
	public TMPro.TMP_Text Source => _source;

	public void OnPointerClick(PointerEventData eventData) {
		_source.gameObject.SetActive(!_source.gameObject.activeSelf);
		LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
	}

}

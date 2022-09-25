using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionButtonUi : MonoBehaviour {
	public UnityEngine.UI.Image icon;
	[SerializeField] private GameObject selectionMarker;

	public bool IsSelected => selectionMarker.activeSelf;

	public TMPro.TMP_Text SelectionTMP_Text {
		get => selectionMarker.GetComponentInChildren<TMPro.TMP_Text>();
	}

	public string SelectionText {
		get => SelectionTMP_Text.text;
		set => SelectionTMP_Text.text = value;
	}

	public Sprite Sprite {
		get => icon.sprite;
		set => icon.sprite = value;
	}

	public void SelectThis() {
		PieceSelection selectionUi = GetComponentInParent<PieceSelection>();
		selectionUi.UnselectAll();
		AddThisToSelection();
	}

	public void AddThisToSelection() {
		//Debug.Log("Select " + name);
		selectionMarker.SetActive(true);
	}

	public void Unselect() {
		selectionMarker.SetActive(false);
	}
}

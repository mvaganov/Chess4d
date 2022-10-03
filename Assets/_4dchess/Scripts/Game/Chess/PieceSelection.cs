using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceSelection : MonoBehaviour {
	public ChessGame chessGame;
	public SelectionButtonUi prefabSelectionButton;
	public RectTransform content;
	public UnityEngine.UI.Button selectButton;
	public TMPro.TMP_Text title;
	private Action<int> callback;

	public void UnselectAll() {
		//Debug.Log("unselect");
		for (int i = 0; i < content.childCount; i++) {
			SelectionButtonUi sbutton = content.GetChild(i).GetComponent<SelectionButtonUi>();
			sbutton.Unselect();
		}
	}

	public void MakeSelection() {
		int index = GetSelectedIndex();
		if (index < 0) { return; }
		callback?.Invoke(index);
		gameObject.SetActive(false);
	}

	public int GetSelectedIndex() {
		return FindButtonSelectedIndex(b => b.IsSelected);
	}

	public int FindButtonSelectedIndex(Func<SelectionButtonUi, bool> predicate) {
		int index = 0;
		for (int i = 0; i < content.childCount; i++) {
			SelectionButtonUi sbutton = content.GetChild(i).GetComponent<SelectionButtonUi>();
			if (sbutton == prefabSelectionButton) { continue; }
			if (predicate(sbutton)) { return index; }
			++index;
		}
		return -1;
	}

	public SelectionButtonUi GetButton(int selectionIndex) {
		int i = 0;
		int foundIndex = FindButtonSelectedIndex(b => {
			bool isTheOne = i++ == selectionIndex;
			return isTheOne;
		});
		if (foundIndex < 0) return null;
		return content.GetChild(foundIndex).GetComponent<SelectionButtonUi>();
	}

	public void SelectPiece(string options, Team team, int defaultSelection, Action<int> selectionCallback) {
		for(int i = content.childCount-1; i >= 0; --i) {
			Transform t = content.GetChild(i);
			SelectionButtonUi btn = t.GetComponent<SelectionButtonUi>();
			if (btn != prefabSelectionButton) { Destroy(btn.gameObject); }
		}
		for (int i = 0; i < options.Length; i++) {
			string option = options[i].ToString();
			ChessGame.PieceCode info = chessGame.GetPieceInfo(option);
			GameObject newButtonObject = Instantiate(prefabSelectionButton.gameObject);
			SelectionButtonUi newButton = newButtonObject.GetComponent<SelectionButtonUi>();
			if (info != null) {
				newButton.name = info.name;
				newButton.SelectionText = info.name;
				newButton.Sprite = info.icons[team.TeamIndex];
				newButton.transform.SetParent(content, false);
				newButton.gameObject.SetActive(true);
			}
			if (i == defaultSelection) {
				newButton.AddThisToSelection();
			}
		}
		callback = selectionCallback;
		gameObject.SetActive(true);
	}

	//private void Start() {
	//	Debug.Log(name);
	//	Team t = chessGame.teams[0];
	//	SelectPiece("NBRQ", t, 1, i => Debug.Log(i));
	//}
}

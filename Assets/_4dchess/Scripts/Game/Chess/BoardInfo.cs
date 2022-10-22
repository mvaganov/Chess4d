using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class BoardInfo {
	public string name = "standard";
	[TextArea(1, 6)]
	public string xfen = "rnbqkbnr/pppppppp/////PPPPPPPP/RNBQKBNR W AHah - 0 0";
}

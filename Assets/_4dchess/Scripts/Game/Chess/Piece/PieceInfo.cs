using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class PieceInfo {
	public string name;
	public string code;
	public Piece prefab;
	public Sprite[] icons;
	public PieceInfo(string name, string code, Piece prefab) {
		this.name = name;
		this.code = code;
		this.prefab = prefab;
		icons = new Sprite[2];
	}
	public PieceInfo(string name, string code) : this(name, code, null) { }
}

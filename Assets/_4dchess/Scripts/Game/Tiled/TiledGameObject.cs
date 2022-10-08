using System.Collections;
using UnityEngine;

public class TiledGameObject : MonoBehaviour {
	private Color originalColor;
	private Gradient ColorCycling;
	private int colorCyclePriority;
	public virtual Material Material {
		get => GetComponentInChildren<Renderer>().material;
		set => GetComponentInChildren<Renderer>().material = value;
	}
	public TMPro.TMP_Text Label;
	public virtual Color Color {
		get => Material.color;
		set => Material.color = value;
	}
	public bool IsColorCycling => ColorCycling != null;
	public void ResetColor() {
		Color = originalColor;
		ColorCycling = null;
	}

	public void ColorCycle(Gradient colors, int priority) {
		bool alreadyCycling = ColorCycling != null;
		if (!alreadyCycling || priority > colorCyclePriority) {
			ColorCycling = colors;
			colorCyclePriority = priority;
		}
		if (alreadyCycling || colors == null) { return; }
		StartCoroutine(ColorCycleCoroutine());
	}

	private IEnumerator ColorCycleCoroutine() {
		//const float colorFrameRateDelay = 60f / 1000;
		long then = System.Environment.TickCount;
		float t = 0;
		while(ColorCycling != null) {
			long now = System.Environment.TickCount;
			long passed = now - then;
			then = now;
			t += passed / 1000f;
			if (t > 1) {
				t -= (int)t;
			}
			Color c = ColorCycling.Evaluate(t);
			if (c.a != 1) {
				float a = c.a;
				c.a = 1;
				c = Color.Lerp(originalColor, c, a);
			}
			Color = c;
			yield return null;// new WaitForSeconds(colorFrameRateDelay);
		}
	}

	public Board GetBoard() {
		Tile tile = GetComponentInParent<Tile>();
		if (tile == null) { return null; }
		return tile.GetComponentInParent<Board>();
	}

	public Coord GetCoord() {
		Tile tile = GetComponentInParent<Tile>();
		if (tile == null) { return Coord.zero; }
		Board board = tile.GetComponentInParent<Board>();
		if (board == null) { return Coord.zero; }
		return board.GetCoord(tile);
	}

	protected virtual void Start() {
		originalColor = Material.color;
	}
}

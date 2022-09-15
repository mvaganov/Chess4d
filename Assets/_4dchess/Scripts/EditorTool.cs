using UnityEngine;

public class EditorTool
{
	public static void MarkSceneDirty() {
#if UNITY_EDITOR
		if (Application.isPlaying) { return; }
		var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
		UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
#endif
	}
}

using UnityEngine;
using UnityEngine.EventSystems;

namespace NonStandard.Inputs {
	public class EventSystemSelector : MonoBehaviour {
#if USE_EVENTSYSTEM || UNITY_EDITOR
		public GameObject inputSystemEventSystem;
#endif
		public GameObject regularEventSystem;
		public void Awake() {
			if (EventSystem.current == null) {
				GameObject prefab =
#if USE_EVENTSYSTEM
					inputSystemEventSystem;
#else
					regularEventSystem;
#endif
				GameObject eventSystem = Instantiate(prefab);
				eventSystem.transform.SetParent(transform.parent, false);
			}
			Destroy(gameObject);
		}
	}
}

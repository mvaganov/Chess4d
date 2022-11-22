using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.Inputs {
	public class OnScreenButton :
#if USE_INPUTSYSTEM
		UnityEngine.InputSystem.OnScreen.OnScreenButton
#else
		MonoBehaviour
#endif
	{
	}
}

using UnityEngine;
using System.Collections;

public class RandomTrigger : MonoBehaviour, ITrigger {

	public void Activate() {
		GameObject.Find ("DebugDisplay").GetComponent<DebugDisplay>().AddText("Test Trigger Activated");
	}

}

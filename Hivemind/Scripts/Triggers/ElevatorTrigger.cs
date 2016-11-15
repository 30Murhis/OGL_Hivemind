using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ElevatorTrigger : MonoBehaviour, Trigger {

    public string requiredAuthorization = "Guard";
    public bool requirementMet = false;

	public void Activate() {
		if (!requirementMet)
        {
            FindObjectOfType<DebugDisplay>().SetText("You have no authorization to use this elevator. Find someone who has.");
        }
        else
        {
			Destroy(FindObjectOfType<AdvancedHivemind>().gameObject);
            Application.LoadLevel(2);
        }
	}

}

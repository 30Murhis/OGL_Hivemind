using UnityEngine;
using System.Collections;

public class InfectionTimer : MonoBehaviour {

    float delayBetweenStages = 15f;
    float stageCount = 5f;
	int infectionState = 0;

    /// <summary>
    /// Advances in infection levels, can be made public if some things can force advance.
    /// </summary>
    void AdvanceInfectionState()
    {
        infectionState++;
        if (infectionState > stageCount)
            FindObjectOfType<AdvancedHivemind>().RemoveCharacter(gameObject);
        else if (infectionState == stageCount && FindObjectOfType<AdvancedHivemind>().GetCurrentlyActiveCharacter() == gameObject)
            FindObjectOfType<DebugDisplay>().SetText("The infection kills this character soon.");
        else if (FindObjectOfType<AdvancedHivemind>().GetCurrentlyActiveCharacter() == gameObject)
            FindObjectOfType<DebugDisplay>().SetText("Your infection is advancing.");

        Debug.Log(gameObject.name + "'s level of infection is now " + infectionState);
    }

    /// <summary>
    /// Starts the infection timer.
    /// </summary>
    /// <param name="delayBetweenStages">If this>0, sets this as the delay between stages. Otherwise uses default (15s).</param>
    public void BeginTimer(float delayBetweenStages = -1)
    {
        if (delayBetweenStages > 0) this.delayBetweenStages = delayBetweenStages;
        InvokeRepeating("AdvanceInfectionState", this.delayBetweenStages, this.delayBetweenStages);
    }
}
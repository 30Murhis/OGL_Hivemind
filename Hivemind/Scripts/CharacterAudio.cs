using UnityEngine;
using System.Collections;

public class CharacterAudio : MonoBehaviour {

    [FMODUnity.EventRef]
    public string footStepSound = "event:/SFX/walk";

    public void FootStep()
    {
        FMODUnity.RuntimeManager.PlayOneShot(footStepSound, transform.position);
    }
}

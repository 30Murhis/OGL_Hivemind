using UnityEngine;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

public class DoorTrigger : MonoBehaviour, ITrigger {

    [Tooltip("Level index to load.")]
    public int loadLevel = 0;

    /// <summary>
    /// Activation of the trigger.
    /// <para>Loads the set level.</para>
    /// </summary>
    public void Activate()
    {
        LoadSceneNoPrefix(loadLevel);
    }

    /// <summary>
    /// Instantly loads another scene.
    /// </summary>
    public void LoadSceneNoPrefix(int number)
    {
        CharacterManager.SetCurrentFloorOfCurrentCharacter(number - 1);

#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene(number);
#else
        Application.LoadLevel(number);
#endif
    }
}

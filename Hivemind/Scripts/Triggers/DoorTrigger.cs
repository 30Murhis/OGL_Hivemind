using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

public class DoorTrigger : MonoBehaviour, Trigger {

    public int loadLevel = 0;
    public bool useLevelNamePrefixes = false;
    //public string loadLevelName = "LevelName";
    public bool smoothTransition = true;

    AsyncOperation async;

    void Start()
    {
        SetSmoothTransition();
    }

    void SetSmoothTransition()
    {
        // Smooth transition moves player character downwards,
        // so it is only allowed when going to lower levels, for now at least
        int currentLevelNumber = int.Parse(GetLevelNameNumber());
        smoothTransition = (currentLevelNumber > loadLevel) ? true : false;
    }

    /// <summary>
    /// Gets the current scene name prefix. E.g. if scene name is "Scene001", returns "Scene".
    /// </summary>
    /// <returns></returns>
    string GetLevelNamePrefix()
    {
        string levelNamePrefix;
#if UNITY_5_3_OR_NEWER
        levelNamePrefix = SceneManager.GetActiveScene().name;
        levelNamePrefix = Regex.Replace(levelNamePrefix, @"[\d-]", string.Empty);
#else
        levelNamePrefix = Application.loadedLevelName;
        levelNamePrefix = Regex.Replace(levelNamePrefix, @"[\d-]", string.Empty);
#endif
        return levelNamePrefix;
    }

    /// <summary>
    /// Gets the current scene number. E.g. if scene name is "Scene001", returns "001".
    /// <para>Returns the number as a string so that 0's do not disappear.</para>
    /// </summary>
    /// <returns></returns>
    string GetLevelNameNumber()
    {
        string currentLevel;
        string currentLevelNumber;

#if UNITY_5_3_OR_NEWER
        currentLevel = SceneManager.GetActiveScene().name;
#else
        currentLevel = Application.loadedLevelName;
#endif
        currentLevelNumber = Regex.Match(currentLevel, @"[\d-]").Value;
        return currentLevelNumber;
    }

    /// <summary>
    /// Loads a level with given name.
    /// </summary>
    /// <param name="name"></param>
    void LoadLevel(string name)
    {
#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene(name);
#else
        Application.LoadLevel(name);
#endif
    }

    /// <summary>
    /// Loads the scene asynchronously in the background, allowing the scene to be activated later.
    /// </summary>
    /// <param name="namePrefix"></param>
    /// <returns></returns>
    IEnumerator SmoothTransition(string name)
    {
        Debug.LogWarning("Asynchronous scene loading started. Do not exit play mode until scene has loaded or Unity might crash.");

#if UNITY_5_3_OR_NEWER
        async = SceneManager.LoadSceneAsync(name);
#else
        async = Application.LoadLevelAsync(name);
#endif

        async.allowSceneActivation = false;
        yield return async;
    }

    /// <summary>
    /// Activation of the trigger.
    /// <para>Loads the set level immediately if going forward in scenes and asynchronously if going backwards in scenes.</para>
    /// </summary>
    public void Activate()
    {
        if (useLevelNamePrefixes)
        {
            string name = GetLevelNamePrefix() + loadLevel;

            if (!smoothTransition)
                LoadLevel(name);
            else
                StartCoroutine(SmoothTransition(name));
        }
        else
        {
            LoadSceneNoPrefix(loadLevel);
        }

        /*
#if UNITY_5_3_OR_NEWER
        string levelNamePrefix = SceneManager.GetActiveScene().name;
        levelNamePrefix = Regex.Replace(levelNamePrefix, @"[\d-]", string.Empty);
        if (!smoothTransition)
            SceneManager.LoadScene(levelNamePrefix + loadLevel);
        else
            StartCoroutine(SmoothTransition(levelNamePrefix));
#else
        string levelNamePrefix = Application.loadedLevelName;
        levelNamePrefix = Regex.Replace(levelNamePrefix, @"[\d-]", string.Empty);
        if (!smoothTransition)
            Application.LoadLevel(levelNamePrefix + loadLevel);
        else
            StartCoroutine(SmoothTransition(levelNamePrefix));
#endif
        */
    }

    /// <summary>
    /// Activates the scene that was loaded asynchronously.
    /// </summary>
    public void ActivateScene()
    {
        async.allowSceneActivation = true;
    }

    /// <summary>
    /// Instantly loads another scene with the same prefix name followed by a set number.
    /// </summary>
    public void LoadScene(int number = -1)
    {
        if (number > -1) loadLevel = number;
        else return;

        string name = GetLevelNamePrefix() + loadLevel;

        LoadLevel(name);
    }

    /// <summary>
    /// Instantly loads another scene without using name prefixes
    /// </summary>
    public void LoadSceneNoPrefix(int number)
    {
        // Changes current character's current floor to the one to be loaded
        //CharacterManager.instance.allCharacters.Find(e => e == CharacterManager.GetCurrentCharacterEntity()).currentFloor = number - 1;

        CharacterManager.SetCurrentFloorOfCurrentCharacter(number - 1);

#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene(number);
#else
        Application.LoadLevel(number);
#endif
    }
}

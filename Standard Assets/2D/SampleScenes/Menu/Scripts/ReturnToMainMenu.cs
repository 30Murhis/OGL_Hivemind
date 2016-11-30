using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainMenu : MonoBehaviour
{
    private bool m_Levelloaded;

    void Awake()
    {
        SceneManager.activeSceneChanged += LevelLoaded;
    }

    void LevelLoaded(Scene arg0, Scene arg1)
    {
        m_Levelloaded = true;
    }

    public void Start()
    {
        DontDestroyOnLoad(this);
    }


    //private void OnLevelWasLoaded(int level)
    //{
    //    m_Levelloaded = true;
    //}


    private void Update()
    {
        if (m_Levelloaded)
        {
            Canvas component = gameObject.GetComponent<Canvas>();
            component.enabled = false;
            component.enabled = true;
            m_Levelloaded = false;
        }
    }


    public void GoBackToMainMenu()
    {
        Debug.Log("going back to main menu");
        Application.LoadLevel("MainMenu");
    }
}

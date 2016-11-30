using UnityEngine;
using System.Collections;

public class GameUI : MonoBehaviour {

    public static GameUI Instance;

	// Use this for initialization
	void Start () {
	    if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

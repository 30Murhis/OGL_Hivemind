using UnityEngine;
using System.Collections;

public class Startover : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine (Startagain ());
	
	}
	
	// Update is called once per frame
	IEnumerator Startagain()
	{
		yield return new WaitForSeconds(6);
		Application.LoadLevel (0);
	}
}

using UnityEngine;
using System.Collections;

public class Stealth : MonoBehaviour {

	public int stealthlevel;
	public bool neutral = false;
	public bool concern = false;
	public bool suspicion = false;
	public bool awareness = false;
	public bool fear = false;
	public bool panic = false;
	public bool alert = false;
	public bool intervension = false;
	public string stealthtext;

	// Use this for initialization
	public void Start () {
		stealthtext = "Neutral";
		neutral = true;
	
	}
	
	// Update is called once per frame
	public void Update () {
		if (stealthlevel == 1) {
			neutral = false;
			concern = true;
			stealthtext = "Concerned";
			suspicion = false;

		}
		if (stealthlevel == 4) {
			concern = false;
			suspicion = true;
			stealthtext = "Suspicious";
			awareness = false;

		}
		if (stealthlevel == 7) {
			suspicion = false;
			awareness = true;
			stealthtext = "Aware";
			fear = false;
		}	
		if (stealthlevel == 10) {
			awareness = false;
			fear = true;
			stealthtext = "Fearful";
			panic = false;
		}
		if (stealthlevel == 13) {
			fear = false;
			panic = true;
			stealthtext = "Panicked";
			alert = false;
		}
		if (stealthlevel == 15) {
			panic = false;
			alert = true;
			stealthtext = "Alerted";
			intervension = false;
		}
		if (stealthlevel == 17) {
			alert = false;
			intervension = true;
			stealthtext = "Fearful as hell";

		}

		//For testing purposes only
		if (Input.GetKeyDown (KeyCode.M)) {
			stealthlevel++;
		}

		if (Input.GetKeyDown (KeyCode.N)) {
			stealthlevel--;
		}
	
	}
}



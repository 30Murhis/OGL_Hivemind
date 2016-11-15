using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour {

    [Tooltip("Character asset data for this entity.")]
    public Character character;
    
    [Tooltip("The level this character currently is located.")]
    public int currentFloor = 0;
    
    //[HideInInspector]
    [Tooltip("Current duration of the current infection stage. Used to keep track of time between level changes.")]
    public int currentInfectionStageDuration = 0;

    [Tooltip("Is this character alive.")]
    public bool isAlive = true;

    [Tooltip("Is this character at start a non-playable character.")]
    public bool isNPC = true;
    
    [Tooltip("Is this character infected.")]
    public bool isInfected = false;

    [Tooltip("Current state of infection on this character.")]
    public CharacterEnums.InfectionState currentStateOfInfection = CharacterEnums.InfectionState.None;

    [Tooltip("Current state of suspicion of this character.")]
    public CharacterEnums.SuspicionState currentStateOfSuspicion = CharacterEnums.SuspicionState.None;

    //public delegate void Die();
    //public event Die OnDeath;

    public Entity(Character character)
    {
        this.character = character;
        this.currentFloor = character.spawnFloor;
        this.currentInfectionStageDuration = character.infectionStageDuration;
        this.isNPC = isInfected ? false : character.isOriginallyNPC;
    }

    public void CopyStats(Entity entity)
    {
        Debug.Log("Copy attempt.");

        if (!entity) return;

        Debug.Log("Copied");

        this.character = entity.character;
        this.currentFloor = entity.currentFloor;
        this.currentInfectionStageDuration = entity.currentInfectionStageDuration;
        this.isAlive = entity.isAlive;
        this.isNPC = isInfected ? false : entity.character.isOriginallyNPC;
        this.isInfected = entity.isInfected;
        this.currentStateOfInfection = entity.currentStateOfInfection;
        this.currentStateOfSuspicion = entity.currentStateOfSuspicion;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    void Start () {
	    
	}
	
	void Update () {
	
	}

    public void Die()
    {
        //if (OnDeath != null)
        //    OnDeath();

        // Death initiated.
        FindObjectOfType<CameraController>().transform.SetParent(null);
        Destroy(gameObject);
    }
}

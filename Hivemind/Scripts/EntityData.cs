using UnityEngine;
using System.Collections;

/// <summary>
/// Data class containing all information about an entity.
/// </summary>
[System.Serializable]
public class EntityData
{
    [HideInInspector]
    public string Name;

    [Tooltip("Entity to which this data is linked.")]
    public Entity entity;
    [Tooltip("Character asset containing unchangeable initialization data.")]
    public Character character;
    [Tooltip("The floor/level the entity is currently located.")]
    public int currentFloor;
    [Tooltip("How many seconds are left in this infection stage.")]
    public int currentInfectionStageDuration;
    [Tooltip("Is this entity still alive.")]
    public bool isAlive;
    [Tooltip("Is this entity a non-playable character.")]
    public bool isNPC;
    [Tooltip("Is this entity infected.")]
    public bool isInfected;
    [Tooltip("Has this entity been built and spawned.")]
    public bool hasSpawned;
    [Tooltip("Current stage of infection.")]
    public CharacterEnums.InfectionStage currentStateOfInfection;
    [Tooltip("Current stage of suspicion.")]
    public CharacterEnums.SuspicionState currentStateOfSuspicion;
    [Tooltip("Last position this character was on a level.")]
    public Vector3 lastPosition;
    [Tooltip("Character's calculated height.")]
    public float height;
    [Tooltip("Character's calculated height.")]
    public float width;

    /// <summary>
    /// Creates EntityData from character scriptable object data.
    /// <para>Uses character's data to initialize entity's info to default values.</para>
    /// <para>Note: EntityData.entity needs to be set at some point.</para>
    /// </summary>
    /// <param name="character">Character scriptable object.</param>
    public EntityData(Character character)
    {
        this.entity = null;
        this.character = character;
        this.currentFloor = ((currentFloor != character.spawnFloor) && hasSpawned) ? currentFloor : character.spawnFloor;
        this.currentInfectionStageDuration = character.infectionStageDuration;
        this.isAlive = true;
        this.isNPC = isInfected ? false : character.isOriginallyNPC;
        this.isInfected = !character.isOriginallyNPC;
        this.hasSpawned = false;
        this.lastPosition = Vector3.zero;
        this.height = 0;
        this.width = 0;

        this.Name = character.characterName;
    }

    /// <summary>
    /// Returns gameobject this entity is attached to, if one is found.
    /// </summary>
    public GameObject GetGameObject()
    {
        try
        {
            return entity.gameObject;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Sets entity's info to match EntityData's info.
    /// </summary>
    //public void DataToEntity()
    //{
    //    entity.entityData = this;
    //    entity.character = this.character;
    //    entity.currentFloor = this.currentFloor;
    //    entity.currentInfectionStageDuration = this.currentInfectionStageDuration;
    //    entity.isAlive = this.isAlive;
    //    entity.isNPC = this.isNPC;
    //    entity.isInfected = this.isInfected;
    //    entity.currentStateOfInfection = this.currentStateOfInfection;
    //    entity.currentStateOfSuspicion = this.currentStateOfSuspicion;
    //}

    /// <summary>
    /// Sets EntityData's info to match entity's info.
    /// </summary>
    //public void EntityToData()
    //{
    //    this.character = entity.character;
    //    this.currentFloor = entity.currentFloor;
    //    this.currentInfectionStageDuration = entity.currentInfectionStageDuration;
    //    this.isAlive = entity.isAlive;
    //    this.isNPC = entity.isNPC;
    //    this.isInfected = entity.isInfected;
    //    this.currentStateOfInfection = entity.currentStateOfInfection;
    //    this.currentStateOfSuspicion = entity.currentStateOfSuspicion;
    //}
}

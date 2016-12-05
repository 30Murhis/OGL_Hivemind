using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

/// <summary>
/// Character manager class.
/// <para>Handles controlling, updating, spawning, despawning, infecting and changing characters.</para>
/// <para>Has a global timer for infection progress for all infected characters, so that they update even on different levels.</para>
/// </summary>
[System.Serializable]
public class CharacterManager : MonoBehaviour {

    public static CharacterManager Instance = null;

    [Header("References")]
    [Tooltip("Characters asset that contains all characters.")]
    public Characters characterList;
    [Tooltip("Character prefab that is used as a blueprint for building and spawning all characters.")]
    public GameObject characterPrefab;

    [Header("Variables")]
    [Tooltip("Enable player input on currently controller character.")]
    public bool enablePlayerInput = true;
    [Tooltip("Delay between each infection timer tick in seconds.")]
    public float infectionTick = 1f;

    [Header("Lists of characters")]
    [Tooltip("List of all character entities.")]
    public List<EntityData> allCharacters = new List<EntityData>();

    [Space]
    [Tooltip("List of all character entities on the current level.")]
    public List<EntityData> charactersOnLevel = new List<EntityData>();

    [Space]
    [Tooltip("List of all infected character entities.")]
    public List<EntityData> infectedCharacters = new List<EntityData>();

    // Character that is currently controlled by player
    GameObject currentCharacter = null;

    // Checks if allCharacters has been initialized
    bool initialized;

    // Checks if first timer tick has passed
    bool firstTimerTickPassed;

    // Action put to memory and ready to be called when needed
    Action actionOnHold;

    // Events
    public delegate void CurrentCharacterChange();
    public static event CurrentCharacterChange OnCharacterChange;

    public delegate void InfectionAdvance();
    public static event InfectionAdvance OnInfectionAdvance;

    public delegate void NewInfectedCharacter(EntityData entityData);
    public static event NewInfectedCharacter OnNewInfectedCharacter;

    public delegate void CharacterDeath(EntityData entityData);
    public static event CharacterDeath OnCharacterDeath;

    /////////////////////////////
    /// MonoBehaviour Methods ///
    /////////////////////////////

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.activeSceneChanged += LevelLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Begins infection timer coroutine
        if (infectedCharacters.Count > 0)
            StartCoroutine(InfectionTimer());
    }

    ///////////////////////
    /// Private Methods ///
    ///////////////////////

    void LevelLoaded(Scene arg0, Scene arg1)
    {
        Reset();
    }

    /// <summary>
    /// Initializes the characters, spawns them on the current level and sets the current player.
    /// </summary>
    void Reset()
    {
        if (Instance != this) return;

        // Initializes the list of characters.
        if (!initialized)
            InitializeCharacterList();

        // Spawns all characters for this level
#if UNITY_5_3_OR_NEWER
        SpawnCharacters(SceneManager.GetActiveScene().buildIndex - 1);
        Debug.Log("Level " + (SceneManager.GetActiveScene().buildIndex - 1) + " loaded, spawning " + charactersOnLevel.Count + " characters.");
#else
        SpawnCharacters(Application.loadedLevel - 1);
#endif
        
        if (actionOnHold != null)
        {
            actionOnHold();
            actionOnHold = null;
        }
        else
        {
            // Sets the currently controlled player
            SetCurrentCharacter(GetCurrentCharacterEntityData());
        }
    }

    /// <summary>
    /// Creates an entity data class from every character scriptable object and adds it to the list of all characters.
    /// </summary>
    void InitializeCharacterList()
    {
        allCharacters.Clear();

        for (int i = 0; i < characterList.allCharacters.Count; i++)
        {
            EntityData entityData = new EntityData(characterList.allCharacters[i]);
            allCharacters.Add(entityData);
        }

        if (allCharacters != null && allCharacters.Count > 0)
            initialized = true;
    }

    /// <summary>
    /// Spawns all characters that are on this level.
    /// </summary>
    /// <param name="level">Level to spawn characters to.</param>
    void SpawnCharacters(int level)
    {
        charactersOnLevel.Clear();
        for (int i = 0; i < allCharacters.Count; i++)
        {
            EntityData entityData = allCharacters[i];

            //Debug.Log("Entity #" + i + ": " + ed.character.characterName);

            if (entityData.isAlive && ((entityData.character.spawnFloor == level && entityData.character.spawnFloor == entityData.currentFloor) || entityData.currentFloor == level))
            {
                if (entityData.hasSpawned)
                {
                    RespawnEntity(entityData);
                }
                else
                {
                    SpawnEntity(entityData);
                }

                // Adds this entity to the list of characters on this level
                charactersOnLevel.Add(entityData);
            }
            else
            {
                if (entityData != null && entityData.GetGameObject() != null)
                    DespawnEntity(entityData);
            }
        }
    }

    /// <summary>
    /// Global infection timer for all infected characters.
    /// <para>Every second decreases infection stage duration by 1 for each character.</para>
    /// <para>Every stage duration (default: 15) seconds increases infection stage by 1 for each character.</para>
    /// <para>Kills all characters that reach the end of last possible infection stage.</para>
    /// </summary>
    IEnumerator InfectionTimer()
    {
        while (true)
        {
            // If first tick, which happens too early because of no delay, does not advance timers
            if (!firstTimerTickPassed)
            {
                firstTimerTickPassed = true;
                yield return new WaitForSeconds(infectionTick);
            }

            // If no infected characters, skip this tick
            if (infectedCharacters.Count <= 0) //StopCoroutine(InfectionTimer());
                yield return new WaitForSeconds(infectionTick);
            
            // Advances all timers on infected characters and infection stages if needed, and also kills everyone who reached their limits
            for (int i = 0; i < infectedCharacters.Count; i++)
            {
                EntityData entityData = infectedCharacters[i];
                entityData.currentInfectionStageDuration--;

                // Checks if timer goes below 0
                if (entityData.currentInfectionStageDuration < 0)
                {
                    // Checks if character is not on the last infection stage
                    if (entityData.currentStateOfInfection != CharacterEnums.InfectionStage.Final)
                    {
                        entityData.currentInfectionStageDuration = entityData.character.infectionStageDuration;
                        entityData.currentStateOfInfection++;

                        if (entityData == GetCurrentCharacterEntityData()) // currentCharacter.GetComponent<Entity>().entityData
                            FindObjectOfType<DebugDisplay>().SetText(entityData.character.characterName + "'s infection is advancing...");
                    }
                    else
                    {
                        if (entityData == GetCurrentCharacterEntityData())
                            FindObjectOfType<DebugDisplay>().SetText(entityData.character.characterName + "'s infection got the best of " + ((entityData.character.gender == CharacterEnums.Gender.Male) ? "him." : "her."));

                        KillCharacter(entityData);
                    }
                }
            }

            // Calls for OnInfectionAdvance event
            if (OnInfectionAdvance != null)
            {
                OnInfectionAdvance();
            }

            yield return new WaitForSeconds(infectionTick);
        }
    }

    /// <summary>
    /// Spawns a character based on information from EntityData class.
    /// </summary>
    /// <param name="entityData">Entity's data, which is used to construct the entity with its information.</param>
    /// <param name="position">Position to spawn the character to.</param>
    /// <returns>Returns the whole character gameobject.</returns>
    GameObject SpawnEntity(EntityData entityData, Vector3 position = default(Vector3))
    {
        // If character's infection is in its final stage and no time is left, does not spawn this character
        if (entityData.currentStateOfInfection == CharacterEnums.InfectionStage.Final && entityData.currentInfectionStageDuration <= 0)
        {
            entityData.isAlive = false;
            return null;
        }

        // Sets default spawn position
        Vector2 spawnPosition = Vector2.zero;

        // Changes position if it is set in parameters
        if (position != default(Vector3))
        {
            spawnPosition = position;
        }
        else
        {
            // Changes position based on character's spawn point type
            switch (entityData.character.spawnPositionSetter)
            {
                case CharacterEnums.SpawnPosition.FromVector:
                    spawnPosition = entityData.character.originalSpawnPosition;
                    break;
                case CharacterEnums.SpawnPosition.RandomFromLevel:
                    if (FindObjectOfType<BackgroundGenerator>())
                    {
                        float mapWidth = FindObjectOfType<BackgroundGenerator>().GetBackgroundWidth();
                        spawnPosition.x = UnityEngine.Random.Range(-mapWidth / 2, mapWidth / 2);
                    }
                    else
                    {
                        Debug.LogWarning("BackgroundGenerator was not found on the scene. Character " + entityData.character.characterName + " was unable to spawn at random spot. It was spawned on the default spot instead.");
                    }
                    break;
            }
        }

        // Tries to find a ground below from Ground layer
        int groundMask = 1 << LayerMask.NameToLayer("Ground");
        RaycastHit2D hit = Physics2D.Raycast(spawnPosition, Vector2.down, 50f, groundMask);

        // If ground is found, moves spawn position's y value to be on top of the ground
        if (hit)
        {
            spawnPosition.y = hit.collider.transform.position.y + hit.collider.bounds.size.y / 2;
        }

        // Creates the gameobject to the spawn position from prefab with character name
        GameObject go = (GameObject)Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
        go.name = entityData.character.characterName;

        // If conversation is set, gives it to the spawned character
        if (entityData.character.VideConversation != null)
        {
            go.GetComponent<VIDE_Assign>().assignedDialogue = entityData.character.VideConversation;
            go.GetComponent<VIDE_Assign>().assignedIndex = entityData.character.VideConversationIndex;
            go.GetComponent<VIDE_Assign>().dialogueName = entityData.character.VideConversation.ToString();
            go.GetComponent<VIDE_Assign>().dialogueName = entityData.character.VideConversation.ToString();
        }

        // If animator is set, gives it to the spawned character
        if (entityData.character.animator != null)
        {
            go.GetComponentInChildren<Animator>().runtimeAnimatorController = entityData.character.animator;
        }

        // Gets components to memory for easy access
        Entity e = go.GetComponent<Entity>();
        RayNPC rnpc = go.GetComponent<RayNPC>();
        //RayPlayerInput rpi = go.GetComponent<RayPlayerInput>();

        // Checks if character is currently NPC and not infected
        if (entityData.isNPC && !entityData.isInfected)
        {
            // Enables/sets NPC stuff and disables player stuff
            go.tag = "NPC";
            rnpc.enabled = true;
            rnpc.SetAIBehaviourActive(true);
            //rpi.enabled = false;
        }
        else
        {
            // Enables/sets player stuff and disables NPC stuff
            go.tag = "Player";
            rnpc.SetAIBehaviourActive(false);
            rnpc.enabled = false;
            //rpi.enabled = true;

            // Adds this entity to the list of infected characters
            infectedCharacters.Add(entityData);

            // Sets infection state to 1 if it is at none
            if (entityData.currentStateOfInfection == CharacterEnums.InfectionStage.None)
            {
                entityData.currentStateOfInfection = CharacterEnums.InfectionStage.Stage1;
            }

            // Launches the new infected character event
            if (OnNewInfectedCharacter != null)
            {
                OnNewInfectedCharacter(entityData);
            }

            // Moves the camera to the position of the first infected character
            if (infectedCharacters.Count == 1)
            {
                CameraController.Instance.transform.position = spawnPosition;
            }
        }

        // Sets hasSpawned to true and sets entity and entity data to reference each other's
        entityData.hasSpawned = true;
        entityData.entity = e;
        e.entityData = entityData;

        //Debug.Log("Character " + entityData.character.characterName + " spawned to " + spawnPosition);

        // Hides comment box
        go.transform.GetComponentInChildren<RandomComment>().transform.parent.gameObject.SetActive(false);

        return go;
    }

    /// <summary>
    /// Respawns an entity and locates it to the given location or its previous location on the level.
    /// </summary>
    /// <param name="entityData"></param>
    /// <param name="position"></param>
    void RespawnEntity(EntityData entityData, Vector3 position = default(Vector3))
    {
        GameObject go = entityData.GetGameObject();
        go.SetActive(true);

        if (position != default(Vector3))
        {
            go.transform.position = position;
        }
        else
        {
            go.transform.position = entityData.lastPosition;
        }

        //Debug.Log("Character " + entityData.character.characterName + " respawned to " + go.transform.position);
    }

    /// <summary>
    /// Despawns an entity saving its previous last position.
    /// </summary>
    /// <param name="entityData"></param>
    void DespawnEntity(EntityData entityData)
    {
        entityData.lastPosition = entityData.GetGameObject().transform.position;
        entityData.GetGameObject().SetActive(false);
    }

    //////////////////////
    /// Static Methods ///
    //////////////////////

    /// <summary>
    /// Spawns a character to the map with given entity information.
    /// </summary>
    /// <param name="entity">Entity to spawn.</param>
    /// <param name="position">Position to spawn to.</param>
    /// <returns>Returns spawned character object.</returns>
    public static GameObject SpawnCharacter(Entity entity, Vector3 position = default(Vector3))
    {
        try
        {
            return Instance.SpawnEntity(Instance.allCharacters.Find(ed => ed.entity == entity), position);
        }
        catch
        {
            Debug.LogError("Spawning from entity failed.");
            return null;
        }
    }

    /// <summary>
    /// Spawns a character to the map with given character information.
    /// </summary>
    /// <param name="character">Character to spawn.</param>
    /// <param name="position">Position to spawn to.</param>
    /// <returns>Returns spawned character object.</returns>
    public static GameObject SpawnCharacter(Character character, Vector3 position = default(Vector3))
    {
        try
        {
            return Instance.SpawnEntity(Instance.allCharacters.Find(ed => ed.character = character), position);
        }
        catch
        {
            Debug.LogError("Spawning from character \"" + character.characterName + "\" failed.");
            return null;
        }
    }

    /// <summary>
    /// Spawns a character to the map with given game object information.
    /// <para>Basically dublicates a character.</para>
    /// </summary>
    /// <param name="entity">Character to spawn.</param>
    /// <param name="position">Position to spawn to.</param>
    /// <returns>Returns spawned character object.</returns>
    public static GameObject SpawnCharacter(GameObject gameObject, Vector3 position = default(Vector3))
    {
        try
        {
            return Instance.SpawnEntity(Instance.allCharacters.Find(ed => ed.GetGameObject() == gameObject), position);
        }
        catch
        {
            Debug.LogError("Spawning from gameObject \"" + gameObject.name + "\" failed.");
            return null;
        }
    }

    /// <summary>
    /// Sets the character to be infected.
    /// <para>Adds the character to the list of infected characters, which allows infection timer to progress on this character.</para>
    /// </summary>
    /// <param name="character">Character to list as infected.</param>
    public static void InfectCharacter(GameObject character)
    {
        EntityData ed = character.GetComponent<Entity>().entityData;
        if (ed != null)
        {
            Instance.infectedCharacters.Add(ed);
            ed.isInfected = true;
            ed.currentStateOfInfection = CharacterEnums.InfectionStage.Stage1;
            ed.isNPC = false;

            // If list of infected characters was empty, starts a new infection timer
            if (Instance.infectedCharacters.Count == 1)
            {
                Instance.StartCoroutine(Instance.InfectionTimer());
            }
            
            //character.transform.parent = FindObjectOfType<AdvancedHivemind>().transform;
            character.tag = "Player";
            //character.GetComponent<RayNPC>().SetAIBehaviourActive(false);
            character.GetComponent<RayNPC>().enabled = false;
            Destroy(character.GetComponent<RayNPC>());
            character.GetComponent<RayPlayerInput>().enabled = false;
            //character.GetComponent<CharacterInteraction>().enabled = false;
            
            if (OnNewInfectedCharacter != null)
            {
                OnNewInfectedCharacter(ed);
            }
        }
        else
        {
            Debug.LogError("Character to infect did not have an Entity script attached to it. Infection failed.");
        }
    }

    /// <summary>
    /// Enables/disables player control on certain entity.
    /// </summary>
    /// <param name="entityData">Entity to change.</param>
    /// <param name="enabled">Set player control enabled.</param>
    public static void SetPlayerControl(EntityData entityData, bool enabled)
    {

        GameObject go = entityData.GetGameObject();

        if (enabled)
        {
            go.layer = LayerMask.NameToLayer("Player");
            //go.AddComponent<DontDestroy>();
        }
        else
        {
            go.layer = LayerMask.NameToLayer("Character");
            //Destroy(go.GetComponent<DontDestroy>());
        }
			

        // Stop movement so that character does not stay running forever in case it was running
        go.GetComponent<RayMovement>().Run = false;
        go.GetComponent<RayMovement>().CharacterInput = Vector2.zero;

        // Set enabled state of player input and interaction script based on enabled boolean
        go.GetComponent<RayPlayerInput>().enabled = enabled;

        if (go.GetComponent<CharacterInteraction>())
            go.GetComponent<CharacterInteraction>().enabled = enabled;

        // Hide commentbox just in case it's active
        if (go.GetComponentInChildren<RandomComment>())
            go.GetComponentInChildren<RandomComment>().transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets currently controlled character to the chosen entity.
    /// <para>If entity is not given, sets it to first infected character found in the level.</para>
    /// </summary>
    public static void SetCurrentCharacter(EntityData entityData = null)
    {
        // If character to set is the same as the current character, return here
        if (entityData != null && GetCurrentCharacterEntityData() == entityData)
        {
            return;
        }

        // Disable the player controls from the possible previous character
        if (Instance.currentCharacter)
        {
            SetPlayerControl(Instance.currentCharacter.GetComponent<Entity>().entityData, false);
            Instance.currentCharacter = null;
        }

        EntityData entityDataOfNewCharacter = null;

        // If entity is given, finds the entity from the list of infected characters.
        // Otherwise gets the first one of the infected characters.
        if (entityData != null)
        {
            entityDataOfNewCharacter = Instance.infectedCharacters.Find(x => x == entityData);
        }
        else
        {
            if (Instance.infectedCharacters.Count > 0)
                entityDataOfNewCharacter = Instance.infectedCharacters[0];
        }

        // If entitydata of the new character has been retrieved successfully, sets current character to be the new entity
        if (entityDataOfNewCharacter != null)
        {
            Instance.currentCharacter = entityDataOfNewCharacter.GetGameObject();
            SetPlayerControl(entityDataOfNewCharacter, true);
        }
        else
        {
            //Debug.LogError("SetCurrentCharacter() failed; no available EntityData found.");
        }
        
        if (OnCharacterChange != null)
        {
            OnCharacterChange();
        }
    }

    /// <summary>
    /// Changes currently controllable character, if there are others.
    /// <para>If index is given, changes to the character that has the index of 'index % infectedCharacters.Count'.</para>
    /// <para>If index is not given, changes to the character next on the list.</para>
    /// <para>Changes levels if the next character is on a different level.</para>
    /// </summary>
    /// <param name="index">Index of entitydata to change to.</param>
    public static void ChangeCurrentCharacter(int index = -1)
    {
        if (Instance.infectedCharacters.Count <= 1) return;

        if (index > -1)
        {
            SetCurrentCharacter(Instance.infectedCharacters[index % Instance.infectedCharacters.Count]);
        }
        else
        {
            // Gets next index for infected characters from current character, and with the index, gets the entitydata of the next character
            int newIndex = Instance.infectedCharacters.IndexOf(GetCurrentCharacterEntityData()) + 1;
            if (newIndex >= Instance.infectedCharacters.Count) newIndex = 0;
            EntityData entityData = Instance.infectedCharacters[newIndex];

            // Checks if the next character is on another level than the current one
            if (entityData.currentFloor != GetCurrentCharacterEntityData().currentFloor)
            {
                // If on another level, changes level and sets SetCurrentCharacter() to activate after level has loaded by storing it in action
                Instance.actionOnHold = () =>
                {
                    SetCurrentCharacter(entityData);
                };
                SceneManager.LoadScene(entityData.currentFloor + 1);
            }
            else
            {
                // If on the same level, just sets the character normally
                SetCurrentCharacter(entityData);
            }
        }
    }

    /// <summary>
    /// Gets currently controlled character's height, if it can be retrieved.
    /// </summary>
    /// <returns>Returns currently controlled character's height.</returns>
    public static float GetCurrentCharacterHeight()
    {
        return GetCurrentCharacterEntity().entityData.height;
    }

    /// <summary>
    /// Gets currently controlled character, if one is set.
    /// </summary>
    /// <returns>Returns currently controlled character.</returns>
    public static GameObject GetCurrentCharacterObject()
    {
        return Instance.currentCharacter;
    }

    /// <summary>
    /// Gets currently controlled character's entity, if one is found.
    /// </summary>
    /// <returns>Returns currently controlled character entity.</returns>
    public static Entity GetCurrentCharacterEntity()
    {
        if (Instance.currentCharacter == null || Instance.currentCharacter.GetComponent<Entity>() == null)
        {
            //Debug.LogWarning("Could not retrieve entity.");
            return null;
        }

        return Instance.currentCharacter.GetComponent<Entity>();
    }

    /// <summary>
    /// Gets currently controlled character's entity data, if one is found.
    /// </summary>
    /// <returns>Returns currently controlled character's entity data.</returns>
    public static EntityData GetCurrentCharacterEntityData()
    {
        if (Instance.currentCharacter == null || Instance.currentCharacter.GetComponent<Entity>() == null || Instance.currentCharacter.GetComponent<Entity>().entityData == null)
        {
            //Debug.LogWarning("Could not retrieve entity data.");
            return null;
        }

        return Instance.currentCharacter.GetComponent<Entity>().entityData;
    }

    /// <summary>
    /// Sets currently controlled character's current floor.
    /// </summary>
    /// <param name="number">Floor number to be set.</param>
    public static void SetCurrentFloorOfCurrentCharacter(int number)
    {
        if (Instance.currentCharacter == null || Instance.currentCharacter.GetComponent<Entity>() == null || Instance.currentCharacter.GetComponent<Entity>().entityData == null)
        {
            Debug.LogError("Could not set the current floor of current character.");
            return;
        }

        GetCurrentCharacterEntityData().currentFloor = number;
    }

    /// <summary>
    /// Kills a character.
    /// </summary>
    /// <param name="entityData">Character to kill.</param>
    public static void KillCharacter(EntityData entityData)
    {
        Debug.Log("Character " + entityData.character.characterName + " has died.");
        if (entityData != null && Instance.charactersOnLevel.Contains(entityData))
        {
            entityData.isAlive = false;
            Instance.charactersOnLevel.Remove(entityData);
            if (Instance.infectedCharacters.Contains(entityData))
            {
                if (OnCharacterDeath != null)
                {
                    OnCharacterDeath(entityData);
                }
                Instance.infectedCharacters.Remove(entityData);
            }
            SetCurrentCharacter();
            Instance.DespawnEntity(entityData);
        }
    }

    /////////////////////////////////////
    /// Obsolete Methods (for memory) ///
    /////////////////////////////////////

    /// <summary>
    /// Spawns a character based on information from Entity class.
    /// </summary>
    /// <param name="entity">Character to spawn.</param>
    /// <param name="position">Position to spawn the character to.</param>
    /// <returns>Returns the whole character gameobject.</returns>
    [System.Obsolete("Use SpawnEntity(EntityData) instead, this is commented to do nothing.", true)]
    GameObject SpawnEntity(Entity entity, Vector3 position = default(Vector3))
    {
        return null;
        //// Initiates default spawn position (-5y because floor is about -6y)
        //Vector3 spawnPosition = new Vector3(0f, -5f, 0f);

        //// Changes position if it is set in parameters
        //if (position != default(Vector3))
        //{
        //    spawnPosition = position;
        //}

        //// Creates the gameobject from prefab with character name
        //GameObject go = (GameObject)Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
        //go.name = entity.character.characterName;

        //// If conversation is set, gives it to the spawned character
        //if (entity.character.VideConversation != null)
        //{
        //    go.GetComponent<VIDE_Assign>().assignedDialogue = entity.character.VideConversation;
        //    go.GetComponent<VIDE_Assign>().assignedIndex = entity.character.VideConversationIndex;
        //}

        //// If animator is set, gives it to the spawned character
        //if (entity.character.animator != null)
        //{
        //    go.GetComponentInChildren<Animator>().runtimeAnimatorController = entity.character.animator;
        //}

        //// If pose sprite is set, changes the sprite to that
        //// Not probably needed
        ////if (character.characterPoseSprite != null)
        ////{
        ////    Debug.Log(character.name + " had pose sprite.");
        ////    Debug.Log("Changing sprite to " + character.characterPoseSprite.name);
        ////    go.GetComponentInChildren<SpriteRenderer>().sprite = character.characterPoseSprite;
        ////}

        //// Gets components to memory for easy access
        //RayNPC rnpc = go.GetComponent<RayNPC>();
        //RayPlayerInput rpi = go.GetComponent<RayPlayerInput>();
        //CharacterInteraction ci = go.GetComponent<CharacterInteraction>();

        //// Checks if character is currently NPC
        //if (entity.isNPC)
        //{
        //    // Enables/sets NPC stuff and disables player stuff
        //    go.transform.SetParent(GameObject.FindGameObjectWithTag("NPC_Container").transform);
        //    go.tag = "NPC";
        //    rnpc.enabled = true;
        //    rnpc.SetAIBehaviourActive(true);
        //    rpi.enabled = false;
        //    ci.enabled = false;
        //}
        //else
        //{
        //    // Enables/sets player stuff and disables NPC stuff
        //    //go.transform.parent = FindObjectOfType<AdvancedHivemind>().transform;
        //    go.tag = "Player";
        //    rnpc.SetAIBehaviourActive(false);
        //    rnpc.enabled = false;
        //    rpi.enabled = true;
        //    ci.enabled = true;
        //}

        ////go.AddComponent<Entity>().CopyStats(entity);
        ////allCharacters[allCharacters.IndexOf(entity)] = go.GetComponent<Entity>();
        ////charactersOnLevel.Add(go.GetComponent<Entity>());

        ////Debug.Log("Entity: " + go.GetComponent<Entity>().character.characterName);

        ////if (go.GetComponent<Entity>().isInfected) infectedCharacters.Add(go.GetComponent<Entity>());

        ////go.AddComponent<Entity>().character = entity.character;
        ////Debug.Log(go.GetComponent<Entity>().character.characterName);

        //Entity e = go.GetComponent<Entity>();
        ////e.character = entity.character;
        //CopyEntities(ref e, entity);
        //Debug.Log(allCharacters[allCharacters.IndexOf(entity.entityData)]);

        //allCharacters[allCharacters.IndexOf(entity.entityData)] = go.GetComponent<Entity>().entityData;
        //charactersOnLevel.Add(go.GetComponent<Entity>());
        //if (go.GetComponent<Entity>().isInfected) infectedCharacters.Add(go.GetComponent<Entity>());

        //// Hides comment box
        //go.transform.GetComponentInChildren<RandomComment>().transform.parent.gameObject.SetActive(false);

        //return go;
    }

    /// <summary>
    /// Spawns a character based on information from Entity class.
    /// </summary>
    /// <param name="indexOfEntity">Index of entity to spawn.</param>
    /// <param name="position">Position to spawn the character to.</param>
    /// <returns>Returns the whole character gameobject.</returns>
    [System.Obsolete("Use SpawnEntity(EntityData) instead.", true)]
    GameObject SpawnEntity(int indexOfEntity, Vector3 position = default(Vector3))
    {
        // If character's infection is in its final stage and no time is left, does not spawn this character
        if (allCharacters[indexOfEntity].currentStateOfInfection == CharacterEnums.InfectionStage.Final && allCharacters[indexOfEntity].currentInfectionStageDuration <= 0)
        {
            EntityData en = allCharacters[indexOfEntity];
            en.isAlive = false;
            return null;
        }

        // Gets map's width and randomizes x position to be somewhere in the map
        float mapWidth = FindObjectOfType<BackgroundGenerator>().GetBackgroundWidth();
        float xPos = UnityEngine.Random.Range(-mapWidth / 2, mapWidth / 2);

        // Initiates default spawn position with random x position and -5.8y, because floor is about -6y
        Vector3 spawnPosition = new Vector3(xPos, -5.8f, 0f);

        // Changes position if it is set in parameters
        if (position != default(Vector3))
        {
            spawnPosition = position;
        }

        // Creates the gameobject from prefab with character name
        GameObject go = (GameObject)Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
        go.name = allCharacters[indexOfEntity].character.characterName;

        // If conversation is set, gives it to the spawned character
        if (allCharacters[indexOfEntity].character.VideConversation != null)
        {
            go.GetComponent<VIDE_Assign>().assignedDialogue = allCharacters[indexOfEntity].character.VideConversation;
            go.GetComponent<VIDE_Assign>().assignedIndex = allCharacters[indexOfEntity].character.VideConversationIndex;
            go.GetComponent<VIDE_Assign>().dialogueName = allCharacters[indexOfEntity].character.VideConversation.ToString();
            go.GetComponent<VIDE_Assign>().dialogueName = allCharacters[indexOfEntity].character.VideConversation.ToString();
        }

        // If animator is set, gives it to the spawned character
        if (allCharacters[indexOfEntity].character.animator != null)
        {
            go.GetComponentInChildren<Animator>().runtimeAnimatorController = allCharacters[indexOfEntity].character.animator;
        }

        // If pose sprite is set, changes the sprite to that
        // Not probably needed
        //if (character.characterPoseSprite != null)
        //{
        //    go.GetComponentInChildren<SpriteRenderer>().sprite = character.characterPoseSprite;
        //}

        // Gets components to memory for easy access
        Entity e = go.GetComponent<Entity>();
        RayNPC rnpc = go.GetComponent<RayNPC>();
        RayPlayerInput rpi = go.GetComponent<RayPlayerInput>();
        //CharacterInteraction ci = go.GetComponent<CharacterInteraction>();

        // Checks if character is currently NPC
        if (allCharacters[indexOfEntity].isNPC && !allCharacters[indexOfEntity].isInfected)
        {
            // Enables/sets NPC stuff and disables player stuff
            //go.transform.SetParent(GameObject.FindGameObjectWithTag("NPC_Container").transform);
            go.tag = "NPC";
            rnpc.enabled = true;
            rnpc.SetAIBehaviourActive(true);
            rpi.enabled = false;
            //ci.enabled = false;
        }
        else
        {
            // Enables/sets player stuff and disables NPC stuff
            //go.transform.parent = FindObjectOfType<AdvancedHivemind>().transform;
            go.tag = "Player";
            rnpc.SetAIBehaviourActive(false);
            rnpc.enabled = false;
            rpi.enabled = true;
            //ci.enabled = true;
            infectedCharacters.Add(go.GetComponent<Entity>().entityData);
            if (allCharacters[indexOfEntity].currentStateOfInfection == CharacterEnums.InfectionStage.None)
            {
                allCharacters[indexOfEntity].currentStateOfInfection = CharacterEnums.InfectionStage.Stage1;
            }
            if (OnNewInfectedCharacter != null)
            {
                OnNewInfectedCharacter(allCharacters[indexOfEntity]);
            }
        }

        // Updates character's entity class
        CopyEntities(ref e, allCharacters[indexOfEntity].entity);

        // Adds the entity the lists of entities
        allCharacters[indexOfEntity].entity = go.GetComponent<Entity>();
        charactersOnLevel.Add(go.GetComponent<Entity>().entityData);

        // Hides comment box
        go.transform.GetComponentInChildren<RandomComment>().transform.parent.gameObject.SetActive(false);

        return go;
    }

    /// <summary>
    /// Spawns a character based on information from Entity class.
    /// <para>Version 2</para>
    /// </summary>
    /// <param name="indexOfEntity">Index of entity to spawn.</param>
    /// <param name="position">Position to spawn the character to.</param>
    /// <returns>Returns the whole character gameobject.</returns>
    [System.Obsolete("Use SpawnEntity(EntityData) instead, this is commented to do nothing.", true)]
    GameObject SpawnEntityV2(int indexOfEntity, Vector3 position = default(Vector3))
    {
        return null;
        ////Entity entityToSpawn = new Entity();
        ////CopyEntities(ref entityToSpawn, allCharacters[indexOfEntity]);

        ////Debug.Log("Index:" + indexOfEntity + ", Name:" + entityToSpawn.character.characterName);
        ////Debug.Log("Index:" + indexOfEntity + ", NPC state:" + entityToSpawn.isNPC);
        ////Debug.Log("Index:" + indexOfEntity + ", Is infected:" + entityToSpawn.isInfected);

        //EntityData entityToSpawn = allCharacters[indexOfEntity];

        //// If character's infection is in its final stage and no time is left, does not spawn this character
        //if (entityToSpawn.currentStateOfInfection == CharacterEnums.InfectionState.Final && entityToSpawn.currentInfectionStageDuration <= 0)
        //{
        //    entityToSpawn.isAlive = false;
        //    return null;
        //}

        //// Gets map's width and randomizes x position to be somewhere in the map
        //float mapWidth = FindObjectOfType<BackgroundGenerator>().GetBackgroundWidth();
        //float xPos = UnityEngine.Random.Range(-mapWidth / 2, mapWidth / 2);

        //// Initiates default spawn position with random x position and -5.8y, because floor is about -6y
        //Vector3 spawnPosition = new Vector3(xPos, -5.8f, 0f);

        //// Changes position if it is set in parameters
        //if (position != default(Vector3))
        //{
        //    spawnPosition = position;
        //}

        //// Creates the gameobject from prefab with character name
        //GameObject go = (GameObject)Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
        //go.name = entityToSpawn.character.characterName;

        //// If conversation is set, gives it to the spawned character
        //if (entityToSpawn.character.VideConversation != null)
        //{
        //    go.GetComponent<VIDE_Assign>().assignedDialogue = entityToSpawn.character.VideConversation;
        //    go.GetComponent<VIDE_Assign>().assignedIndex = entityToSpawn.character.VideConversationIndex;
        //    go.GetComponent<VIDE_Assign>().dialogueName = entityToSpawn.character.VideConversation.ToString();
        //    go.GetComponent<VIDE_Assign>().dialogueName = entityToSpawn.character.VideConversation.ToString();
        //}

        //// If animator is set, gives it to the spawned character
        //if (entityToSpawn.character.animator != null)
        //{
        //    go.GetComponentInChildren<Animator>().runtimeAnimatorController = entityToSpawn.character.animator;
        //}

        //// If pose sprite is set, changes the sprite to that
        //// Not probably needed
        ////if (character.characterPoseSprite != null)
        ////{
        ////    go.GetComponentInChildren<SpriteRenderer>().sprite = character.characterPoseSprite;
        ////}

        //// Gets components to memory for easy access
        //Entity e = go.GetComponent<Entity>();
        //e.entityData = entityToSpawn;
        //entityToSpawn.entity = e;
        //RayNPC rnpc = go.GetComponent<RayNPC>();
        //RayPlayerInput rpi = go.GetComponent<RayPlayerInput>();
        ////CharacterInteraction ci = go.GetComponent<CharacterInteraction>();

        //// Checks if character is currently NPC
        //if (entityToSpawn.isNPC && !entityToSpawn.isInfected)
        //{
        //    // Enables/sets NPC stuff and disables player stuff
        //    //go.transform.SetParent(GameObject.FindGameObjectWithTag("NPC_Container").transform);
        //    go.tag = "NPC";
        //    rnpc.enabled = true;
        //    rnpc.SetAIBehaviourActive(true);
        //    rpi.enabled = false;
        //    //ci.enabled = false;
        //}
        //else
        //{
        //    // Enables/sets player stuff and disables NPC stuff
        //    //go.transform.parent = FindObjectOfType<AdvancedHivemind>().transform;
        //    go.tag = "Player";
        //    rnpc.SetAIBehaviourActive(false);
        //    rnpc.enabled = false;
        //    rpi.enabled = true;
        //    //ci.enabled = true;
        //    infectedCharacters.Add(go.GetComponent<Entity>());
        //    if (entityToSpawn.currentStateOfInfection == CharacterEnums.InfectionState.None)
        //    {
        //        entityToSpawn.currentStateOfInfection = CharacterEnums.InfectionState.State1;
        //    }
        //    if (OnNewInfectedCharacter != null)
        //    {
        //        OnNewInfectedCharacter(entityToSpawn.entity);
        //    }
        //}

        //// Updates character's entity class
        ////CopyEntities(ref e, entityToSpawn.entity);
        //entityToSpawn.hasSpawned = true;
        //e.hasSpawned = true;
        //entityToSpawn.DataToEntity();

        //// Adds the entity the lists of entities
        ////entityToSpawn = go.GetComponent<Entity>();
        //charactersOnLevel.Add(go.GetComponent<Entity>());

        //// Hides comment box
        //go.transform.GetComponentInChildren<RandomComment>().transform.parent.gameObject.SetActive(false);

        //return go;
    }

    /// <summary>
    /// Copies entity values from an entity class to a referenced one.
    /// </summary>
    /// <param name="copyTo">Entity to copy values to.</param>
    /// <param name="copyFrom">Entity to copy values from.</param>
    [System.Obsolete("Unused, not working & commented to do nothing.", true)]
    void CopyEntities(ref Entity copyTo, Entity copyFrom)
    {
        //copyTo.character = copyFrom.character;
        //copyTo.currentFloor = copyFrom.currentFloor;
        //copyTo.currentInfectionStageDuration = copyFrom.currentInfectionStageDuration;
        //copyTo.currentStateOfInfection = copyFrom.currentStateOfInfection;
        //copyTo.currentStateOfSuspicion = copyFrom.currentStateOfSuspicion;
        //copyTo.isAlive = copyFrom.isAlive;
        //copyTo.isInfected = copyFrom.isInfected;
        //copyTo.isNPC = copyFrom.isNPC;
    }

    //public static void UpdateEntityInfo()
    //{
    //    Entity e = instance.allCharacters.Find(x => x == entity);
    //    instance.CopyEntities(ref e, entity);

    //    for (int i = 0; i < instance.allCharacters.Count; i++)
    //    {
    //        //instance.allCharacters[i] = new Entity(instance.allCharacters[i]);
    //        if (instance.allCharacters[i].hasSpawned)
    //            instance.allCharacters[i].EntityToData();
    //    }
    //}
}

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// Character manager class.
/// <para>Handles controlling, updating, spawning, despawning, infecting and changing characters.</para>
/// <para>Has a global timer for infection progress for all infected characters, so that they update even on different levels.</para>
/// </summary>
public class CharacterManager : MonoBehaviour {

    public static CharacterManager instance = null;

    [Tooltip("Characters asset that contains all characters.")]
    public Characters characterList;
    [Tooltip("Character prefab that is used as a blueprint for building and spawning all characters.")]
    public GameObject characterPrefab;

    [SerializeField]
    [Tooltip("List of all character entities.")]
    public List<Entity> allCharacters = new List<Entity>();
    [SerializeField]
    [Tooltip("List of all character entities on the current level.")]
    public List<Entity> charactersOnLevel = new List<Entity>();
    [SerializeField]
    [Tooltip("List of all infected character entities.")]
    public List<Entity> infectedCharacters = new List<Entity>();

    // Character that is currently controlled by player
    public GameObject currentCharacter = null;

    // Checks if allCharacters has been initialized
    bool initialized;

    // Checks if first timer tick has passed
    bool firstTimerTickPassed;

    public delegate void CurrentCharacterChange();
    public static event CurrentCharacterChange OnCharacterChange;

    public delegate void InfectionAdvance();
    public static event InfectionAdvance OnInfectionAdvance;

    public delegate void NewInfectedCharacter(Entity e);
    public static event NewInfectedCharacter OnNewInfectedCharacter;

    public delegate void CharacterDeath(int i);
    public static event CharacterDeath OnCharacterDeath;

    /////////////////////////////
    /// MonoBehaviour Methods ///
    /////////////////////////////

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void OnEnable() // OnLevelWasLoaded()
    {
        // Initializes the list of characters.
        if (!initialized) InitializeCharacterList();

        Debug.Log("Level loaded. Spawning characters.");

        // Clears the previous list
        charactersOnLevel.Clear();

        // Spawns all characters for this level
        SpawnCharacters(Application.loadedLevel - 1);

        // Sets the currently controlled player
        SetCurrentCharacter();
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

    void InitializeCharacterList()
    {
        foreach (Character c in characterList.allCharacters)
        {
            Entity entity = new Entity(c);
            entity.character = c;
            allCharacters.Add(entity);
        }
        if (allCharacters != null && allCharacters.Count > 0) initialized = true;
    }

    /// <summary>
    /// Spawns all characters that are on this level.
    /// </summary>
    /// <param name="level">Level to spawn characters to.</param>
    void SpawnCharacters(int level)
    {
        for (int i = 0; i < allCharacters.Count; i++)
        {
            Entity e = allCharacters[i];
            if (e.isAlive)
            {
                if ((e.character.spawnFloor == level && e.character.spawnFloor == e.currentFloor) || e.currentFloor == level)
                {
                    SpawnEntity(i);
                }
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
        while (infectedCharacters.Count > 0)
        {
            if (infectedCharacters.Count <= 0) StopCoroutine(InfectionTimer());

            // If first tick, which happens too early because of no delay, does not advance timers
            if (!firstTimerTickPassed)
            {
                firstTimerTickPassed = true;
                yield return new WaitForSeconds(1f);
            }
            
            for (int i = 0; i < infectedCharacters.Count; i++)
            {
                Entity e = infectedCharacters[i];
                e.currentInfectionStageDuration--;

                if (e.currentInfectionStageDuration <= 0)
                {
                    if (e.currentStateOfInfection != CharacterEnums.InfectionState.Final)
                    {
                        e.currentInfectionStageDuration = e.character.infectionStageDuration;
                        e.currentStateOfInfection++;

                        if (e == currentCharacter.GetComponent<Entity>())
                            FindObjectOfType<DebugDisplay>().SetText(e.character.characterName + "'s infection is advancing...");
                    }
                    else
                    {
                        if (e == currentCharacter.GetComponent<Entity>())
                            FindObjectOfType<DebugDisplay>().SetText(e.character.characterName + "'s infection got the best of " + ((e.character.gender.ToString() == "Male") ? "him." : "her."));

                        KillCharacter(e);
                    }
                }
            }

            if (OnInfectionAdvance != null)
            {
                OnInfectionAdvance();
            }

            yield return new WaitForSeconds(1f);
        }
    }
    
    /// <summary>
    /// Spawns a character based on information from Entity class.
    /// </summary>
    /// <param name="entity">Character to spawn.</param>
    /// <param name="position">Position to spawn the character to.</param>
    /// <returns>Returns the whole character gameobject.</returns>
    GameObject SpawnEntity(int indexOfEntity, Vector3 position = default(Vector3))
    {
        // If character's infection is in its final stage and no time is left, does not spawn this character
        if (allCharacters[indexOfEntity].currentStateOfInfection == CharacterEnums.InfectionState.Final && allCharacters[indexOfEntity].currentInfectionStageDuration <= 0)
        {
            Entity en = allCharacters[indexOfEntity];
            en.isAlive = false;
            return null;
        }

        // Gets map's width and randomizes x position to be somewhere in the map
        float mapWidth = FindObjectOfType<BackgroundGenerator>().GetBackgroundWidth();
        float xPos = UnityEngine.Random.Range(-mapWidth / 2, mapWidth / 2);

        // Initiates default spawn position with random x position and -5.9y, because floor is about -6y
        Vector3 spawnPosition = new Vector3(xPos, -5.9f, 0f);

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
        if (allCharacters[indexOfEntity].isNPC)
        {
            // Enables/sets NPC stuff and disables player stuff
            go.transform.SetParent(GameObject.FindGameObjectWithTag("NPC_Container").transform);
            go.tag = "NPC";
            rnpc.enabled = true;
            rnpc.SetAIBehaviourActive(true);
            rpi.enabled = false;
            //ci.enabled = false;
        }
        else
        {
            // Enables/sets player stuff and disables NPC stuff
            go.transform.parent = FindObjectOfType<AdvancedHivemind>().transform;
            go.tag = "Player";
            rnpc.SetAIBehaviourActive(false);
            rnpc.enabled = false;
            rpi.enabled = true;
            //ci.enabled = true;
            infectedCharacters.Add(go.GetComponent<Entity>());
            if (allCharacters[indexOfEntity].currentStateOfInfection == CharacterEnums.InfectionState.None)
            {
                allCharacters[indexOfEntity].currentStateOfInfection = CharacterEnums.InfectionState.State1;
            }
            if (OnNewInfectedCharacter != null)
            {
                OnNewInfectedCharacter(allCharacters[indexOfEntity]);
            }
        }

        // Updates character's entity class
        CopyEntities(ref e, allCharacters[indexOfEntity]);

        // Adds the entity the lists of entities
        allCharacters[indexOfEntity] = go.GetComponent<Entity>();
        charactersOnLevel.Add(go.GetComponent<Entity>());

        // Hides comment box
        go.transform.GetComponentInChildren<RandomComment>().transform.parent.gameObject.SetActive(false);

        return go;
    }

    /// <summary>
    /// Copies entity values from an entity class to a referenced one.
    /// </summary>
    /// <param name="copyTo">Entity to copy values to.</param>
    /// <param name="copyFrom">Entity to copy values from.</param>
    void CopyEntities(ref Entity copyTo, Entity copyFrom)
    {
        copyTo.character = copyFrom.character;
        copyTo.currentFloor = copyFrom.currentFloor;
        copyTo.currentInfectionStageDuration = copyFrom.currentInfectionStageDuration;
        copyTo.currentStateOfInfection = copyFrom.currentStateOfInfection;
        copyTo.currentStateOfSuspicion = copyFrom.currentStateOfSuspicion;
        copyTo.isAlive = copyFrom.isAlive;
        copyTo.isInfected = copyFrom.isAlive;
        copyTo.isNPC = copyFrom.isNPC;
    }

    //////////////////////
    /// Static Methods ///
    //////////////////////

    /// <summary>
    /// Spawns a character to the map with given entity information.
    /// </summary>
    /// <param name="entity">Character to spawn.</param>
    /// <param name="position">Position to spawn to.</param>
    /// <returns>Returns spawned character object.</returns>
    public static GameObject SpawnCharacter(Entity entity, Vector3 position = default(Vector3))
    {
        int i = instance.allCharacters.IndexOf(entity);
        return instance.SpawnEntity(i, position);
    }

    /// <summary>
    /// Spawns a character to the map with given character information.
    /// </summary>
    /// <param name="entity">Character to spawn.</param>
    /// <param name="position">Position to spawn to.</param>
    /// <returns>Returns spawned character object.</returns>
    public static GameObject SpawnCharacter(Character character, Vector3 position = default(Vector3))
    {
        foreach (Entity e in instance.allCharacters)
        {
            if (e.character == character)
            {
                int i = instance.allCharacters.IndexOf(e);
                return instance.SpawnEntity(i, position);
            }
        }

        return null;
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
        Entity entity = gameObject.GetComponent<Entity>();

        if (entity)
        {
            foreach (Entity e in instance.allCharacters)
            {
                if (e == entity)
                {
                    int i = instance.allCharacters.IndexOf(e);
                    return instance.SpawnEntity(i, position);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Sets the character to be infected.
    /// <para>Adds the character to the list of infected characters, which allows infection timer to progress on this character.</para>
    /// </summary>
    /// <param name="character">Character to list as infected.</param>
    public static void InfectCharacter(GameObject character)
    {
        Entity e = character.GetComponent<Entity>();
        if (e)
        {
            instance.infectedCharacters.Add(e);
            e.isInfected = true;
            e.currentStateOfInfection = CharacterEnums.InfectionState.State1;
            e.isNPC = false;

            // If list of infected characters was empty, starts a new infection timer
            if (instance.infectedCharacters.Count == 1)
            {
                instance.StartCoroutine(instance.InfectionTimer());
            }
            
            character.transform.parent = FindObjectOfType<AdvancedHivemind>().transform;
            character.tag = "Player";
            character.GetComponent<RayNPC>().SetAIBehaviourActive(false);
            character.GetComponent<RayNPC>().enabled = false;
            character.GetComponent<RayPlayerInput>().enabled = false;
            character.GetComponent<CharacterInteraction>().enabled = false;
            
            if (OnNewInfectedCharacter != null)
            {
                OnNewInfectedCharacter(e);
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
    /// <param name="entity">Entity to change.</param>
    /// <param name="enabled">Set player control enabled.</param>
    public static void SetPlayerControl(Entity entity, bool enabled)
    {
		if (enabled) {
			entity.gameObject.layer = LayerMask.NameToLayer("Player");

		} else {
			entity.gameObject.layer = LayerMask.NameToLayer("Character");
		}

        GameObject go = entity.GetGameObject();

        // Stop movement so that character does not stay running forever in case it was running
        go.GetComponent<RayMovement>().Run = false;
        go.GetComponent<RayMovement>().CharacterInput = Vector2.zero;

        // Set enabled state of player input and interaction script based on enabled boolean
        go.GetComponent<RayPlayerInput>().enabled = enabled;
        go.GetComponent<CharacterInteraction>().enabled = enabled;

        // Hide commentbox just in case it's active
        if (go.GetComponentInChildren<RandomComment>())
            go.GetComponentInChildren<RandomComment>().transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets currently controlled character to the chosen entity.
    /// <para>If entity is not given, sets it to first infected character found in the level.</para>
    /// </summary>
    public static void SetCurrentCharacter(Entity entity = null)
    {
        // Disable the player controls from the possible previous character
        if (instance.currentCharacter)
            SetPlayerControl(instance.currentCharacter.GetComponent<Entity>(), false);

        instance.currentCharacter = null;

        for (int i = 0; i < instance.infectedCharacters.Count; i++)
        {
            // If no entity is given, chooses the first infected character and gives it player control
            if (!entity)
            {
                instance.currentCharacter = instance.infectedCharacters[i].GetGameObject();
                SetPlayerControl(instance.infectedCharacters[i], true);
                break;
            }
            // If entity is given, finds the entity from the list of infected character and sets it as current character
            if (instance.infectedCharacters[i] == entity)
            {
                instance.currentCharacter = instance.infectedCharacters[i].GetGameObject();
                SetPlayerControl(instance.infectedCharacters[i], true);
                break;
            }
        }
    }

    /// <summary>
    /// Changes currently controllable character.
    /// <para>If index is given, changes to the character that has the index of 'index % infectedCharacters.Count'.</para>
    /// <para>If index is not given, changes to the character next on the list.</para>
    /// </summary>
    /// <param name="index"></param>
    public static void ChangeCurrentCharacter(int index = -1)
    {
        if (instance.infectedCharacters.Count <= 0) return;

        if (index > -1)
        {
            SetCurrentCharacter(instance.infectedCharacters[index % instance.infectedCharacters.Count]);
        }
        else
        {
            SetCurrentCharacter(instance.infectedCharacters[(instance.infectedCharacters.IndexOf(instance.currentCharacter.GetComponent<Entity>()) + 1) == instance.infectedCharacters.Count ? 0 : (instance.infectedCharacters.IndexOf(instance.currentCharacter.GetComponent<Entity>()) + 1)]);
        }
        if (OnCharacterChange != null)
        {
            OnCharacterChange();
        }
    }

    /// <summary>
    /// Gets currently controlled character, if one is set.
    /// </summary>
    /// <returns>Returns currently controlled character.</returns>
    public static Entity GetCurrentCharacterEntity()
    {
        return instance.currentCharacter.GetComponent<Entity>();
    }

    /// <summary>
    /// Gets currently controlled character, if one is set.
    /// </summary>
    /// <returns>Returns currently controlled character.</returns>
    public static GameObject GetCurrentCharacterObject()
    {
        return instance.currentCharacter;
    }

    /// <summary>
    /// Kills a character.
    /// </summary>
    /// <param name="entity">Character to kill.</param>
    public static void KillCharacter(Entity entity)
    {
        if (entity && instance.charactersOnLevel.Contains(entity))
        {
            instance.charactersOnLevel.Remove(entity);
            if (instance.infectedCharacters.Contains(entity))
            {
                if (OnCharacterDeath != null)
                {
                    OnCharacterDeath(instance.infectedCharacters.IndexOf(entity));
                }
                instance.infectedCharacters.Remove(entity);
            }
            SetCurrentCharacter();
            entity.Die();
            if (instance.infectedCharacters.Count <= 0) instance.StopCoroutine(instance.InfectionTimer());
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
    [System.Obsolete("Use SpawnEntity(index) instead.", true)]
    GameObject SpawnEntity(Entity entity, Vector3 position = default(Vector3))
    {
        // Initiates default spawn position (-5y because floor is about -6y)
        Vector3 spawnPosition = new Vector3(0f, -5f, 0f);

        // Changes position if it is set in parameters
        if (position != default(Vector3))
        {
            spawnPosition = position;
        }

        // Creates the gameobject from prefab with character name
        GameObject go = (GameObject)Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
        go.name = entity.character.characterName;

        // If conversation is set, gives it to the spawned character
        if (entity.character.VideConversation != null)
        {
            go.GetComponent<VIDE_Assign>().assignedDialogue = entity.character.VideConversation;
            go.GetComponent<VIDE_Assign>().assignedIndex = entity.character.VideConversationIndex;
        }

        // If animator is set, gives it to the spawned character
        if (entity.character.animator != null)
        {
            go.GetComponentInChildren<Animator>().runtimeAnimatorController = entity.character.animator;
        }

        // If pose sprite is set, changes the sprite to that
        // Not probably needed
        //if (character.characterPoseSprite != null)
        //{
        //    Debug.Log(character.name + " had pose sprite.");
        //    Debug.Log("Changing sprite to " + character.characterPoseSprite.name);
        //    go.GetComponentInChildren<SpriteRenderer>().sprite = character.characterPoseSprite;
        //}

        // Gets components to memory for easy access
        RayNPC rnpc = go.GetComponent<RayNPC>();
        RayPlayerInput rpi = go.GetComponent<RayPlayerInput>();
        CharacterInteraction ci = go.GetComponent<CharacterInteraction>();

        // Checks if character is currently NPC
        if (entity.isNPC)
        {
            // Enables/sets NPC stuff and disables player stuff
            go.transform.SetParent(GameObject.FindGameObjectWithTag("NPC_Container").transform);
            go.tag = "NPC";
            rnpc.enabled = true;
            rnpc.SetAIBehaviourActive(true);
            rpi.enabled = false;
            ci.enabled = false;
        }
        else
        {
            // Enables/sets player stuff and disables NPC stuff
            go.transform.parent = FindObjectOfType<AdvancedHivemind>().transform;
            go.tag = "Player";
            rnpc.SetAIBehaviourActive(false);
            rnpc.enabled = false;
            rpi.enabled = true;
            ci.enabled = true;
        }

        //go.AddComponent<Entity>().CopyStats(entity);
        //allCharacters[allCharacters.IndexOf(entity)] = go.GetComponent<Entity>();
        //charactersOnLevel.Add(go.GetComponent<Entity>());

        //Debug.Log("Entity: " + go.GetComponent<Entity>().character.characterName);

        //if (go.GetComponent<Entity>().isInfected) infectedCharacters.Add(go.GetComponent<Entity>());

        //go.AddComponent<Entity>().character = entity.character;
        //Debug.Log(go.GetComponent<Entity>().character.characterName);

        Entity e = go.GetComponent<Entity>();
        //e.character = entity.character;
        CopyEntities(ref e, entity);
        Debug.Log(allCharacters[allCharacters.IndexOf(entity)]);

        allCharacters[allCharacters.IndexOf(entity)] = go.GetComponent<Entity>();
        charactersOnLevel.Add(go.GetComponent<Entity>());
        if (go.GetComponent<Entity>().isInfected) infectedCharacters.Add(go.GetComponent<Entity>());

        // Hides comment box
        go.transform.GetComponentInChildren<RandomComment>().transform.parent.gameObject.SetActive(false);

        return go;
    }
}

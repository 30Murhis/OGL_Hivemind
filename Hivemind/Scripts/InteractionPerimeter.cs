using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

[RequireComponent(typeof(CircleCollider2D))]
public class InteractionPerimeter : MonoBehaviour
{
    [Tooltip("Interaction perimeter radius.")]
    public float perimeterRadius = 10f;
    [Tooltip("Display glow around the edges of currently chosen interactable object.")]
    public bool glowTarget = false;
    [Tooltip("Switch sides of participants in the dialogue when current talker changes.")]
    public bool switchSidesOfTalkers = false;

    [Tooltip("Reference to dialogueUI script.")]
    public DialogueUI dialogueUI;
    [Tooltip("Material used for glow effect if glowInteractables is set to true.")]
    public Material glowMaterial;

    [Tooltip("Currently chosen interactable object.")]
    public GameObject currentlyChosenObject;
    [Tooltip("Index of currently chosen interactable object.")]
    public int currentlyChosen;

    [Tooltip("List of possible interactable objects.")]
    public List<GameObject> interactables = new List<GameObject>();

    // Original material of the currently chosen object
    Material originalMaterial;

    // Because pivot is bottom center, this is added to transform.position to center the perimeter to character
    float perimeterCenterY = 3.5f;

    // Discussion partner used in dialogues
    GameObject discussionPartner;

    // Static instance to keep only one existing at the same time
    public static InteractionPerimeter Instance;

    void Awake()
    {
        SceneManager.activeSceneChanged += LevelLoaded;
    }

    void LevelLoaded(Scene arg0, Scene arg1)
    {
        interactables.Clear();
    }

    void Start () {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CircleCollider2D cc = GetComponent<CircleCollider2D>();
        cc.radius = perimeterRadius;
        cc.offset = Vector2.up * perimeterCenterY;

        CharacterManager.OnCharacterChange += CharacterManager_OnCharacterChange;
        CharacterManager.OnCharacterDeath += CharacterManager_OnCharacterDeath;
        CharacterManager.OnNewInfectedCharacter += CharacterManager_OnNewInfectedCharacter;

        TryFindNewParent();
    }

    /// <summary>
    /// When a character gets infected, removes it from the list of interactable objects if it is found there.
    /// </summary>
    /// <param name="ed"></param>
    void CharacterManager_OnNewInfectedCharacter(EntityData ed)
    {
        interactables.RemoveAll(go => go.GetComponent<Entity>() && go.GetComponent<Entity>().entityData == ed);
    }

    /// <summary>
    /// When character dies, sets parent to null so that this object will not be destroyed too.
    /// <para>Also calls for TryFindNewParent().</para>
    /// </summary>
    /// <param name="i"></param>
    void CharacterManager_OnCharacterDeath(EntityData entityData)
    {
        transform.SetParent(null);
        TryFindNewParent();
    }

    /// <summary>
    /// When character is changed, sets parent to new character.
    /// </summary>
    void CharacterManager_OnCharacterChange()
    {
        TryFindNewParent();
    }
    
    void Update () {
        if (dialogueUI == null)
        {
            dialogueUI = FindObjectOfType<DialogueUI>();
        }

        if (dialogueUI == null) return;

        if (!dialogueUI.dialogue.isLoaded && discussionPartner)
        {
            SetDialogueModeActive(false);
        }

        //if (transform.parent == null)
        //{
        //    TryFindNewParent();
        //}
    }

    /// <summary>
    /// Tries to get new parent from CharacterManager.
    /// </summary>
    void TryFindNewParent()
    {
        if (CharacterManager.GetCurrentCharacterObject() != null)
        {
            transform.SetParent(CharacterManager.GetCurrentCharacterObject().transform);
            transform.localPosition = Vector3.zero;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position + Vector3.up * perimeterCenterY, perimeterRadius);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // Adds collided object to list of interactables if it meets with at least 1 of the following conditions on top of the first one:
        // - If it is not already listed in the list of interactables
        // - If it has ITrigger interface or its tag is NPC
        // - If it is interactable and not on CharacterManager's list of infected characters when an Entity class is found attached to the NPC tagged object
        // - If it is a ghost object
        if (!interactables.Contains(col.gameObject))
        {
            if (col.GetComponent<ITrigger>() != null)
            {
                interactables.Add(col.gameObject);
            }
            if (col.tag == "NPC")
            {
                Entity e = col.gameObject.GetComponent<Entity>();
                if (e && !CharacterManager.Instance.infectedCharacters.Contains(e.entityData) && e.entityData.character.isInteractable)
                {
                    interactables.Add(col.gameObject);
                }
            }
            if (col.name.StartsWith("Ghost"))
            {
                interactables.Add(col.gameObject);
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        //if (col.tag == "NPC" || col.GetComponent<ITrigger>() != null)
        if (interactables.Contains(col.gameObject))
        {
            interactables.Remove(col.gameObject);

            if (originalMaterial != null)
            {
                RemoveGlow();
            }

            if (currentlyChosenObject == col.gameObject)
            {
                //TryGetPreviousInteractionTarget();
                RemoveTarget();
                FindObjectOfType<DebugDisplay>().ClearText();
            }
        }
    }

    /// <summary>
    /// Sets current target to null and its index to 0
    /// </summary>
    void RemoveTarget()
    {
        currentlyChosen = 0;
        currentlyChosenObject = null;
    }

    /// <summary>
    /// Changes current interaction target to the next one, if one exists.
    /// </summary>
    public void TryGetNextInteractionTarget()
    {
        if (interactables.Count == 0)
        {
            RemoveTarget();
        }
        else
        {
            currentlyChosen++;
            if (currentlyChosen >= interactables.Count)
                currentlyChosen = 0;

            ChangeInteractionTarget();
        }
    }

    /// <summary>
    /// Changes current interaction target to the previous one, if one exists.
    /// </summary>
    public void TryGetPreviousInteractionTarget()
    {
        if (interactables.Count == 0)
        {
            RemoveTarget();
        }
        else
        {
            currentlyChosen--;
            if (currentlyChosen < 0)
                currentlyChosen = interactables.Count - 1;

            ChangeInteractionTarget();
        }
    }

    /// <summary>
    /// Changes interaction target based on currentlyChosen variable.
    /// </summary>
    void ChangeInteractionTarget()
    {
        if (originalMaterial != null)
        {
            RemoveGlow();
        }

        GameObject newTarget = interactables[currentlyChosen % interactables.Count];
        Entity e = newTarget.GetComponent<Entity>();

        // If the next target would end up being an infected or non-interactable character for some reason, removes it and tries to get another target
        if (e && (CharacterManager.Instance.infectedCharacters.Contains(e.entityData) || !e.entityData.character.isInteractable))
        {
            interactables.RemoveAt(currentlyChosen);
            TryGetNextInteractionTarget();
            return;
        }

        currentlyChosenObject = interactables[currentlyChosen % interactables.Count];

        string nameOfTarget = currentlyChosenObject.name;
        nameOfTarget = nameOfTarget.Contains("Ghost") ? nameOfTarget.Substring("Ghost ".Length) : nameOfTarget;

        FindObjectOfType<DebugDisplay>().SetText("Currently chosen interaction target: " + nameOfTarget);

        if (glowTarget && glowMaterial != null)
        {
            originalMaterial = currentlyChosenObject.GetComponentInChildren<SpriteRenderer>().material;

            if (currentlyChosenObject.GetComponentInChildren<SpriteRenderer>())
                currentlyChosenObject.GetComponentInChildren<SpriteRenderer>().material = glowMaterial;
        }
    }

    /// <summary>
    /// Removes glow of an object.
    /// </summary>
    void RemoveGlow()
    {
        if (currentlyChosenObject != null && currentlyChosenObject.GetComponentInChildren<SpriteRenderer>())
            currentlyChosenObject.GetComponentInChildren<SpriteRenderer>().material = originalMaterial;

        originalMaterial = null;
    }

    /// <summary>
    /// Interacts with currently chosen targeted interactable object.
    /// </summary>
    public void InteractWithCurrentTarget()
    {
        if (currentlyChosenObject != null && currentlyChosen >= 0)
            InteractWith(currentlyChosenObject);
    }

    /// <summary>
    /// Interacts with gameobject, if it is possible to do so.
    /// </summary>
    /// <param name="obj"></param>
    public void InteractWith(GameObject obj)
    {
        // Check for interactable NPC
        if (obj.tag == "NPC") // && obj.GetComponent<RayNPC>()
        {
            if (obj.GetComponentInParent<Entity>().entityData.character.isInteractable)
            {
                // Check for a ghost object
                if (obj.name.StartsWith("Ghost"))
                {
                    discussionPartner = obj.transform.parent.gameObject;
                }
                else
                {
                    discussionPartner = obj;
                }
            }

            InitializeDialogue();
        }

        // Calls for a method from ITrigger interface
        if (obj.GetComponent<ITrigger>() != null)
        {
            obj.GetComponent<ITrigger>().Activate();
        }
    }

    /// <summary>
    /// Activates/deactivates dialogue mode.
    /// </summary>
    /// <param name="active">Active state wanted.</param>
    public void SetDialogueModeActive(bool active)
    {
        CharacterManager.GetCurrentCharacterObject().GetComponent<RayMovement>().allowMovement = !active;
        CharacterManager.GetCurrentCharacterObject().GetComponent<RayPlayerInput>().enablePlayerInput = !active;

        RayMovement partnerRM = discussionPartner.GetComponent<RayMovement>();
        RayNPC partnerRNPC = discussionPartner.GetComponent<RayNPC>();

        partnerRM.allowMovement = !active;

        if (partnerRNPC)
            partnerRNPC.SetAIBehaviourActive(!active);

        if (active)
        {
            partnerRM.FaceTarget(CharacterManager.GetCurrentCharacterObject().GetComponentInChildren<SpriteRenderer>());
        }
        if (!active && dialogueUI.dialogue.isLoaded)
        {
            dialogueUI.dialogue.EndDialogue();
            discussionPartner = null;
        }
    }

    /// <summary>
    /// Sets activates dialogue mode and processes the dialogue.
    /// </summary>
    void InitializeDialogue()
    {
        if (!discussionPartner.GetComponent<VIDE_Assign>()) return;

        SetDialogueModeActive(true);

        ProcessDialogue();
    }

    /// <summary>
    /// Processes the dialogue and updates the dialogue UI accordingly.
    /// </summary>
    void ProcessDialogue()
    {
        VIDE_Assign assigned = discussionPartner.GetComponent<VIDE_Assign>();

        Sprite NPC = discussionPartner.GetComponent<Entity>().entityData.character.characterDialogSprite;
        Sprite player = CharacterManager.GetCurrentCharacterEntity().entityData.character.characterDialogSprite;

        if (!dialogueUI.dialogue.isLoaded)
        {
            dialogueUI.Begin(assigned);
        }
        else
        {
            dialogueUI.NextNode();
        }

        if (!dialogueUI.dialogue.isLoaded) return;

        if (dialogueUI.dialogue.nodeData.currentIsPlayer)
        {
            dialogueUI.dialogImage.sprite = player;
            if (switchSidesOfTalkers)
            {
                dialogueUI.dialogImage.transform.SetAsFirstSibling();
                dialogueUI.dialogImage.rectTransform.localScale = Vector3.one;
            }
        }
        else
        {
            dialogueUI.dialogImage.sprite = NPC;
            if (switchSidesOfTalkers)
            {
                dialogueUI.dialogImage.transform.SetAsLastSibling();
                dialogueUI.dialogImage.rectTransform.localScale = new Vector3(-1, 1, 1);
            }
        }

        dialogueUI.npcName.text = discussionPartner.name;
    }
}

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public class InteractionPerimeter : MonoBehaviour
{
    [Tooltip("Interaction perimeter radius.")]
    public float perimeterRadius = 10f;
    [Tooltip("Display glow around the edges of currently chosen interactable object.")]
    public bool glowTarget = false;

    [Tooltip("List of possible interactable objects.")]
    public List<GameObject> interactables = new List<GameObject>();
    [Tooltip("Currently chosen interactable object.")]
    public GameObject currentlyChosenObject;
    [Tooltip("Index of currently chosen interactable object.")]
    public int currentlyChosen;

    [Tooltip("Reference to dialogueUI script.")]
    public DialogueUI dialogueUI;

    [Tooltip("Material used for glow effect if glowInteractables is set to true.")]
    public Material glowMaterial;

    // Original material of the currently chosen object
    Material originalMaterial;

    // Because pivot is bottom center, this is added to transform.position to center the perimeter to character
    float perimeterCenterY = 3.5f;

    // Discussion partner used in dialogues
    GameObject discussionPartner;
    
    void Start () {
        CircleCollider2D cc = GetComponent<CircleCollider2D>();
        cc.radius = perimeterRadius;
        cc.offset = Vector2.up * perimeterCenterY;

        CharacterManager.OnCharacterChange += CharacterManager_OnCharacterChange;
        CharacterManager.OnCharacterDeath += CharacterManager_OnCharacterDeath;

        TryFindNewParent();
    }

    /// <summary>
    /// When character dies, sets parent to null so that this object will not be destroyed too.
    /// </summary>
    /// <param name="i"></param>
    void CharacterManager_OnCharacterDeath(int i)
    {
        transform.SetParent(null);
    }

    /// <summary>
    /// When character is changed, sets parent to new character.
    /// </summary>
    void CharacterManager_OnCharacterChange()
    {
        TryFindNewParent();
    }
    
    void Update () {
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
        if (col.tag == "NPC" || col.GetComponent<Trigger>() != null)
        {
            if (!interactables.Contains(col.gameObject))
                interactables.Add(col.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "NPC" || col.GetComponent<Trigger>() != null)
        {
            interactables.Remove(col.gameObject);

            if (originalMaterial != null)
            {
                RemoveGlow();
            }

            if (currentlyChosenObject == col.gameObject)
            {
                TryGetPreviousInteractionTarget();
            }
        }
    }

    /// <summary>
    /// Changes current interaction target to the next one, if one exists.
    /// </summary>
    public void TryGetNextInteractionTarget()
    {
        if (interactables.Count == 0)
        {
            currentlyChosen = 0;
            currentlyChosenObject = null;
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
            currentlyChosen = 0;
            currentlyChosenObject = null;
        }
        else
        {
            currentlyChosen--;
            if (currentlyChosen < 0)
                currentlyChosen = interactables.Count - 1;

            ChangeInteractionTarget();
        }
    }

    void ChangeInteractionTarget()
    {
        if (originalMaterial != null)
        {
            RemoveGlow();
        }

        currentlyChosenObject = interactables[currentlyChosen % interactables.Count];

        FindObjectOfType<DebugDisplay>().SetText("Currently chosen interaction target: " + currentlyChosenObject.name);

        if (glowTarget && glowMaterial != null)
        {
            originalMaterial = currentlyChosenObject.GetComponentInChildren<SpriteRenderer>().material;

            if (currentlyChosenObject.GetComponentInChildren<SpriteRenderer>())
                currentlyChosenObject.GetComponentInChildren<SpriteRenderer>().material = glowMaterial;
        }
    }

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
            if (obj.name.StartsWith("Ghost")) {
                if (obj.GetComponentInParent<Entity>().character.isInteractable)
                {
                    discussionPartner = obj.transform.parent.gameObject;
                }
            }
            else
            {
                if (obj.GetComponent<Entity>().character.isInteractable)
                {
                    discussionPartner = obj;
                }
            }

            InitializeDialogue();
        }

        if (obj.GetComponent<Trigger>() != null)
        {
            obj.GetComponent<Trigger>().Activate();
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

    void InitializeDialogue()
    {
        if (!discussionPartner.GetComponent<VIDE_Assign>()) return;

        SetDialogueModeActive(true);

        ProcessDialogue();
    }

    void ProcessDialogue()
    {
        VIDE_Assign assigned = discussionPartner.GetComponent<VIDE_Assign>();

        Sprite NPC = discussionPartner.GetComponent<Entity>().character.characterDialogSprite;
        Sprite player = CharacterManager.GetCurrentCharacterEntity().character.characterDialogSprite;

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
            dialogueUI.dialogImage.transform.SetAsFirstSibling();
            dialogueUI.dialogImage.rectTransform.localScale = Vector3.one;
        }
        else
        {
            dialogueUI.dialogImage.sprite = NPC;
            dialogueUI.dialogImage.transform.SetAsLastSibling();
            dialogueUI.dialogImage.rectTransform.localScale = new Vector3(-1, 1, 1);
        }

        dialogueUI.npcName.text = discussionPartner.name;
    }
}

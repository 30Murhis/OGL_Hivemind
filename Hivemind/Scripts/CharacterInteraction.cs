using UnityEngine;

/// <summary>
/// Handles interaction with other NPC game objects
/// </summary>
public class CharacterInteraction : MonoBehaviour
{
    //Reference to our diagUI script for quick access
    [Tooltip("Reference to dialogueUI script.")]
    public DialogueUI dialogueUI;

    [Tooltip("Interaction perimeter radius.")]
    public float perimeterRadius = 10f;

    //public Material glowMaterial;

    //// Because pivot is bottom center, this is added to transform.position to center the perimeter to character
    //float perimeterCenterY = 3.5f;
    
    RayMovement rm;
    RayPlayerInput rpi;

    GameObject discussionPartner;
    RayMovement discussionPartnerRM;
    RayNPC discussionPartnerRNPC;

    [HideInInspector] public bool TryInteraction = false;

    // Remote controlled NPC check for testing purposes
    bool isControlledNPC;

    void Start()
    {
        //cm = GetComponent<CharacterMovement>();
        rpi = GetComponent<RayPlayerInput>();
        rm = GetComponent<RayMovement>();
        dialogueUI = FindObjectOfType<DialogueUI>();
    }

    void Update()
    {

        // If dialog is on, disable movement
        if (dialogueUI.dialogue.isLoaded)
        {
            //cm.AllowCharacterMovement(false);
            rm.allowMovement = false;
            rpi.enablePlayerInput = false;

            if (Input.GetAxisRaw("Horizontal") != 0)
            {
                dialogueUI.dialogue.EndDialogue();
            }
        }

        // Otherwise allow movement to characters involved in discussion
        if (!dialogueUI.dialogue.isLoaded && discussionPartner)
        {
            discussionPartner.GetComponent<RayNPC>().SetAIBehaviourActive(!isControlledNPC);
            discussionPartner.GetComponent<RayMovement>().allowMovement = true;
            discussionPartner = null;

            //cm.AllowCharacterMovement(true);
            rm.allowMovement = true;
            rpi.enablePlayerInput = true;
        }
        
        if (TryInteraction)
        {
            TryInteract();
        }

        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y + 3), transform.right * perimeterRadius * rm.facingDirection, Color.red);

        //Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position + Vector3.up * perimeterCenterY, perimeterRadius);
        //for (int i = 0; i < colliders.Length; i++)
        //{
        //    if (colliders[i].transform.parent == transform) continue;

        //    // Checks for triggers & colliders
        //    if (colliders[i].GetComponent<Trigger>() != null || colliders[i].tag == "NPC")
        //    {
        //        //colliders[i].GetComponentInChildren<SpriteRenderer>().material = glowMaterial;
        //    }
        //}

    }

    //void OnDrawGizmosSelected()
    //{
    //    Gizmos.DrawWireSphere(transform.position + Vector3.up * perimeterCenterY, perimeterRadius);
    //}

    /// <summary>
    /// Casts a ray to see if we hit an NPC and, if so, we interact
    /// </summary>
    void TryInteract()
    {
        if (discussionPartner)
        {
            DoInteraction();
            return;
        }

        // Multi ray
        RaycastHit2D[] rHit = Physics2D.RaycastAll(new Vector2(transform.position.x, transform.position.y + 3), Vector2.right * rm.facingDirection, perimeterRadius, -1);
        if (rHit.Length > 0)
        {
            foreach (RaycastHit2D hit in rHit)
            {
                if (hit.collider.tag == "NPC")
                {
                    // Check for a ghost object
                    if (hit.collider.name.StartsWith("Ghost"))
                    {
                        // Get the ghost's original from the character pair list - not needed if ghost is original's child
                        //CharacterPair cp = FindObjectOfType<GhostManager>().characters.Find(c => c.Ghost == hit.collider.gameObject);

                        discussionPartner = hit.collider.transform.parent.gameObject;
                    }
                    else
                    {
                        discussionPartner = hit.collider.gameObject;
                    }

                    if (!discussionPartner.GetComponent<VIDE_Assign>()) return;

                    discussionPartnerRM = discussionPartner.GetComponent<RayMovement>();
                    discussionPartnerRNPC = discussionPartner.GetComponent<RayNPC>();
                    isControlledNPC = discussionPartner.GetComponent<NPCControl>();

                    discussionPartnerRM.allowMovement = false;
                    discussionPartnerRM.FaceTarget(GetComponentInChildren<SpriteRenderer>());
                    discussionPartnerRNPC.SetAIBehaviourActive(false);

                    DoInteraction();

                    // Break the loop so that only one conversation is active in case many NPC's got raycasted
                    break;
                }
            }
        }
    }

    void DoInteraction()
    {
        //Lets grab the NPC's DialogueAssign script...
        VIDE_Assign assigned = discussionPartner.GetComponent<VIDE_Assign>();

        Sprite player = discussionPartner.GetComponent<Entity>().entityData.character.characterDialogSprite;
        Sprite NPC = GetComponent<Entity>().entityData.character.characterDialogSprite;

        if (!dialogueUI.dialogue.isLoaded)
        {
            //... and use it to begin the conversation
            dialogueUI.Begin(assigned);
        }
        else
        {
            //If conversation already began, let's just progress through it
            dialogueUI.NextNode();
        }

        if (dialogueUI.dialogue.nodeData.currentIsPlayer)
        {
            dialogueUI.dialogImage.sprite = NPC;
            dialogueUI.dialogImage.transform.SetAsFirstSibling();
            dialogueUI.dialogImage.rectTransform.localScale = Vector3.one;
        }
        else
        {
            dialogueUI.dialogImage.sprite = player;
            dialogueUI.dialogImage.transform.SetAsLastSibling();
            dialogueUI.dialogImage.rectTransform.localScale = new Vector3(-1, 1, 1);
        }
        

        dialogueUI.npcName.text = discussionPartner.name;
    }

    void OnDestroy()
    {
        if (dialogueUI && dialogueUI.dialogue)
            dialogueUI.dialogue.EndDialogue();
    }
}

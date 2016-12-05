using UnityEngine;

public class DebugInput : MonoBehaviour {

    [HideInInspector]
    public bool allowCameraZoom = false;

    [HideInInspector]
    public string keyKillCurrentCharacter = "0";
    [HideInInspector]
    public string keyWarpTargetToCurrent = "1";
    [HideInInspector]
    public string keyChangeCommentOfTargetChar = "2";
    [HideInInspector]
    public string keyMoveTargetCharLeft = "3";
    [HideInInspector]
    public string keyMoveTargetCharRight = "4";
    [HideInInspector]
    public string keyRunWithTargetChar = "5";
    [HideInInspector]
    public string keySpawnRandomCharacter = "6";
    [HideInInspector]
    public string keySpawnRandomCharacterSomewhere = "7";

    [HideInInspector]
    public GameObject targetChar;
    [HideInInspector]
    public Characters listOfCharacters;

    RandomComment rc;
    RayMovement rm;
    //AdvancedHivemind hivemind;
    int commentNum = 0;
    int dir;
        
	void Start () {
        //hivemind = FindObjectOfType<AdvancedHivemind>();
        if (targetChar)
        {
            rc = targetChar.GetComponent<RandomComment>();
            rm = targetChar.GetComponent<RayMovement>();
            //targetChar.GetComponent<RayNPC>().enableSimpleAI = false;
        }
    }

#if UNITY_EDITOR
    void Update () {
        
        if (Input.GetKeyDown(keyKillCurrentCharacter))
        {
            //hivemind.RemoveCharacter(hivemind.GetCurrentlyActiveCharacter());
            CharacterManager.KillCharacter(CharacterManager.GetCurrentCharacterEntity().entityData);
        }

        if (targetChar)
        {
            if (Input.GetKeyDown(keyWarpTargetToCurrent))
            {
                WarpTargetToCurrent();
            }

            if (Input.GetKeyDown(keyChangeCommentOfTargetChar))
            {
                if (!rc) rc = targetChar.GetComponentInChildren<RandomComment>(true);

                commentNum++;
                if (commentNum >= rc.GetCommentCount()) commentNum = 0;
                rc.GetComment(commentNum);
            }

            if (Input.GetKey(keyMoveTargetCharLeft)) dir = -1;
            else if (Input.GetKey(keyMoveTargetCharRight)) dir = 1;
            else dir = 0;
            
            if (dir != 0)
            {
                if (targetChar.GetComponent<RayNPC>().enableSimpleAI)
                    targetChar.GetComponent<RayNPC>().enableSimpleAI = false;
            }

            if (targetChar.GetComponent<RayNPC>() && !targetChar.GetComponent<RayNPC>().enableSimpleAI)
            {
                rm.CharacterInput = new Vector2(dir, 0);
                rm.Run = Input.GetKey(keyRunWithTargetChar);
            }
        }
        else
        {
            targetChar = CharacterManager.Instance.allCharacters[Random.Range(1, CharacterManager.Instance.allCharacters.Count)].GetGameObject();
            if (targetChar)
            {
                rc = targetChar.GetComponent<RandomComment>();
                rm = targetChar.GetComponent<RayMovement>();
            }
        }

        if (Input.GetKeyDown(keySpawnRandomCharacter))
        {
            if (listOfCharacters.allCharacters.Count > 0)
            {
                SpawnRandomCharacter(true);
            }
        }

        if (Input.GetKeyDown(keySpawnRandomCharacterSomewhere))
        {
            if (listOfCharacters.allCharacters.Count > 0)
            {
                SpawnRandomCharacter(false);
            }
        }

        float delta = Input.GetAxisRaw("Mouse ScrollWheel");

        if (delta != 0 && allowCameraZoom)
        {
            FindObjectOfType<CameraController>().ChangeZoomLevelToDirection(-delta);
        }
    }
#endif
    
    public void SpawnRandomCharacter(bool onCurrentCharacter)
    {
        float bgWidth = FindObjectOfType<BackgroundGenerator>().GetBackgroundWidth();
        CharacterManager.SpawnCharacter(
            listOfCharacters.allCharacters[Random.Range(1, listOfCharacters.allCharacters.Count)],
            onCurrentCharacter ? CharacterManager.GetCurrentCharacterObject().transform.position : new Vector3(Random.Range(-bgWidth / 2, bgWidth / 2), 0, 0)
        );

        //Character character = listOfCharacters.allCharacters[Random.Range(0, listOfCharacters.allCharacters.Count)];
        //GameObject go = new GameObject(character.characterName);

        //if (onCurrentCharacter) go.transform.position = hivemind.GetCurrentlyActiveCharacter().transform.position;
        //else
        //{
        //    float bgWidth = FindObjectOfType<BackgroundGenerator>().GetBackgroundWidth();
        //    go.transform.position = new Vector2( Random.Range(-bgWidth/2,bgWidth/2), 5);
        //}

        //go.tag = "NPC";

        //GameObject spriteChild = Instantiate(targetChar.transform.GetChild(0).gameObject);
        //spriteChild.transform.SetParent(go.transform);
        //spriteChild.transform.localPosition = Vector3.zero;
        //spriteChild.GetComponent<Animator>().runtimeAnimatorController = character.animator;

        //BoxCollider2D bx = go.AddComponent<BoxCollider2D>();
        //bx.offset = new Vector2(0, 2.554656f);
        //bx.size = new Vector2(1.793282f, 4.76397f);

        //go.AddComponent<Rigidbody2D>().isKinematic = true;
        //go.AddComponent<RayController2D>().collisionMask = 1024;
        //go.AddComponent<RayMovement>();
        //go.AddComponent<CharacterInteraction>();
        //go.AddComponent<RayNPC>();

        //GameObject sporeShotChild = new GameObject("SporeShotSource");
        //sporeShotChild.transform.SetParent(go.transform);
        //sporeShotChild.transform.localPosition = new Vector2(0, 3.27f);

        //RayPlayerInput rpi = go.AddComponent<RayPlayerInput>();
        //RayPlayerInput copy = hivemind.GetCurrentlyActiveCharacter().GetComponent<RayPlayerInput>();
        ////rpi.sporeShotSource = copy.sporeShotSource;
        //rpi.projectile = copy.projectile;
        //rpi.enabled = false;

        //GameObject commentChild = Instantiate(targetChar.transform.GetChild(2).gameObject);
        //commentChild.transform.SetParent(go.transform);
        //commentChild.transform.localPosition = targetChar.transform.GetChild(2).localPosition;

        //if (character.isInteractable && character.VideConversation != null)
        //{
        //    VIDE_Assign va = go.AddComponent<VIDE_Assign>();
        //    va.assignedDialogue = character.VideConversation;
        //    va.assignedIndex = character.VideConversationIndex;
        //    Debug.Log(va.assignedIndex);
        //    va.dialogueName = go.name + " Dialogue";
        //}
    }

    public void WarpTargetToCurrent()
    {
        //targetChar.transform.position = hivemind.GetCurrentlyActiveCharacter().transform.position;
        targetChar.transform.position = CharacterManager.GetCurrentCharacterObject().transform.position;
    }
}
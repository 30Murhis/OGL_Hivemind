using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

[System.Serializable]
public class CharacterPair
{
    [HideInInspector]
    public string Name;
    public GameObject Original;
    public GameObject Ghost;
    public SpriteRenderer OriginalSR;
    public SpriteRenderer GhostSR;
    public GameObject OriginalCommentBox;
    public GameObject GhostCommentBox;
}

/// <summary>
/// Manager class for handling 'ghost' partners of characters.
/// <para>Creates ghost objects from all characters on map and makes them to mimick original's sprites and comments.</para>
/// <para>Keeps the ghosts one map width away from the originals, and on the other side of the map than the originals.</para>
/// <para>Used to create the illusion of infinite looping 2D world by visualizing characters outside the map where cameras still can reach.</para>
/// </summary>
public class GhostManager : MonoBehaviour
{
    public GameObject commentBoxPrefab;
    public List<CharacterPair> characterPairs = new List<CharacterPair>();

    float bgWidth;

    public static GhostManager Instance;

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

    void LevelLoaded(Scene arg0, Scene arg1)
    {
        Initialize();
    }

    void Initialize()
    {
        //for (int i = 0; i < characterPairs.Count; i++)
        //{
        //    Destroy(characterPairs[i].Ghost);
        //}

        //characterPairs.Clear();

        // Get width of the level's background
        bgWidth = FindObjectOfType<BackgroundGenerator>().GetBackgroundWidth();

        // Create ghosts from NPC's and player characters
        CreateGhost("NPC");
        CreateGhost("Player");
    }

    void CreateGhost(string tag)
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(tag))
        {
            // Skip if one of the child's of the character object
            if (go.transform.parent != null) continue;
            //    if (go.transform.root.tag == tag) continue;

            // Skip if object has a ghost already
            if (characterPairs.Exists(cp => cp.Original == go)) continue;

            GameObject ghost = new GameObject("Ghost " + go.name);
            ghost.tag = tag;

            ghost.transform.parent = go.transform;
            ghost.transform.localPosition = new Vector2(bgWidth, go.transform.position.y);

            SpriteRenderer ghostSR = ghost.AddComponent<SpriteRenderer>();
            BoxCollider2D ghostBC = ghost.AddComponent<BoxCollider2D>();

            SpriteRenderer originalSR = go.GetComponentInChildren<SpriteRenderer>();
            BoxCollider2D originalBC = go.GetComponent<BoxCollider2D>();

            ghostSR.sortingOrder = originalSR.sortingOrder;
            ghostBC.size = originalBC.size;
            ghostBC.offset = originalBC.offset;

            GameObject ghostComment = (GameObject)Instantiate(commentBoxPrefab, ghost.transform, false);

            // Add the original-ghost pair to list
            characterPairs.Add(
                new CharacterPair
                {
                    Name = go.name,
                    Original = go,
                    Ghost = ghost,
                    OriginalSR = originalSR,
                    GhostSR = ghostSR,
                    OriginalCommentBox = go.transform.FindChild("CommentBox").gameObject,
                    GhostCommentBox = ghostComment
                }
            );
        }
    }

    void Update()
    {
        // foreach (CharacterPair character in characterPairs)
        for (int i = 0; i < characterPairs.Count; i++)
        {
            CharacterPair cp = characterPairs[i];
            if (cp.Original == null)
            {
                characterPairs.Remove(cp);
                return;
            }

            // Sets the x position of the ghost object to the opposite side of the map from the original depending on which side of the x-axis the original currently is
            if (Mathf.Sign(cp.Original.transform.position.x) > 0)
            {
                cp.Ghost.transform.position = new Vector2(cp.Original.transform.position.x - bgWidth, cp.Original.transform.position.y);
            }
            else
            {
                cp.Ghost.transform.position = new Vector2(cp.Original.transform.position.x + bgWidth, cp.Original.transform.position.y);
            }

            // Update the ghost's sprite to match the original's sprite
            cp.GhostSR.sprite = cp.OriginalSR.sprite;

            // Update the ghost's look direction to match the original's
            cp.GhostSR.flipX = cp.OriginalSR.flipX;

            // Update the ghost's commentbox to mimick the original's commentbox
            cp.GhostCommentBox.SetActive(cp.OriginalCommentBox.activeInHierarchy);
            cp.GhostCommentBox.GetComponentInChildren<Text>().text = cp.OriginalCommentBox.GetComponentInChildren<Text>().text;
        }

    }
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

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

public class GhostManager : MonoBehaviour
{

    public GameObject commentBoxPrefab;
    public List<CharacterPair> characters = new List<CharacterPair>();

    float bgWidth;

    void Start()
    {
        // Get width of the level's background
        bgWidth = FindObjectOfType<BackgroundGenerator>().GetBackgroundWidth();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("NPC"))
        {
            if (go.transform.parent.tag == "NPC") continue;

            GameObject ghost = new GameObject("Ghost " + go.name);
            ghost.tag = "NPC";

            ghost.transform.position = new Vector2(bgWidth, go.transform.position.y);
            ghost.transform.parent = go.transform;

            SpriteRenderer ghostSR = ghost.AddComponent<SpriteRenderer>();
            BoxCollider2D ghostBC = ghost.AddComponent<BoxCollider2D>();

            BoxCollider2D originalBC = go.GetComponent<BoxCollider2D>();

            ghostBC.size = originalBC.size;
            ghostBC.offset = originalBC.offset;

            GameObject ghostComment = (GameObject)Instantiate(commentBoxPrefab, ghost.transform, false);

            // Add the original-ghost pair to list
            characters.Add(
                new CharacterPair
                {
                    Name = go.name,
                    Original = go,
                    Ghost = ghost,
                    OriginalSR = go.GetComponentInChildren<SpriteRenderer>(),
                    GhostSR = ghostSR,
                    OriginalCommentBox = go.GetComponentInChildren<RandomComment>().transform.parent.gameObject,
                    GhostCommentBox = ghostComment
                }
            );
        }

    }

    void Update()
    {

        foreach (CharacterPair character in characters)
        {

            if (character.Original == null)
            {
                characters.Remove(character);
                return;
            }

            // Sets the x position of the ghost object to the opposite side of the map from the original depending on which side of the x-axis the original currently is

            if (character.Original == null)
            {
                characters.Remove(character);
                return;
            }
            // Sets the x position of the ghost object to the opposite side of the map from the original depending on which side of the x-axis the original currently is refs/remotes/origin/master
            if (Mathf.Sign(character.Original.transform.position.x) > 0)
            {
                character.Ghost.transform.position = new Vector2(character.Original.transform.position.x - bgWidth, character.Original.transform.position.y);
            }
            else
            {
                character.Ghost.transform.position = new Vector2(character.Original.transform.position.x + bgWidth, character.Original.transform.position.y);
            }

            // Update the ghost's sprite to match the original's sprite
            character.GhostSR.sprite = character.OriginalSR.sprite;

            // Update the ghost's look direction to match the original's
            character.GhostSR.flipX = character.OriginalSR.flipX;

            character.GhostCommentBox.SetActive(character.OriginalCommentBox.activeInHierarchy);
            character.GhostCommentBox.GetComponentInChildren<Text>().text = character.OriginalCommentBox.GetComponentInChildren<Text>().text;
        }

    }
}

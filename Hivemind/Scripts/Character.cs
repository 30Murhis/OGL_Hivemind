using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Character scriptable object class containing unchangeable initialization data used to construct all characters.
/// </summary>
[System.Serializable]
public class Character : ScriptableObject
{
    [Tooltip("Character name.")]
    public string characterName = "Character";

    [Tooltip("Original spawn level of this character.")]
    public int spawnFloor = 0;

    [Tooltip("Authorization state/access rights.")]
    public string authorization = "";
    
    [Tooltip("Priority.")]
    public int priority = 0;

    [Tooltip("The duration of each infection stage before stage advances to the next level.")]
    public int infectionStageDuration = 15;

    [Tooltip("Animator of the character.")]
    public RuntimeAnimatorController animator = null;

    [Tooltip("Is this character a non-playable character at the start.")]
    public bool isOriginallyNPC = true;

    //[Tooltip("Is this character infected at the start.")]
    //public bool isOriginallyInfected = false;

    [Tooltip("Is this character interactable by player.")]
    public bool isInteractable = true;
    
    [Tooltip("Is this character infectable by player.")]
    public bool isInfectable = true;
    
    [Tooltip("Is this character an inanimate object that cannot move.")]
    public bool isInanimateObject = false;

    [Tooltip("Gender of the character.")]
    public CharacterEnums.Gender gender = CharacterEnums.Gender.Unknown;

    [Tooltip("VIDE conversation that this character uses.")]
    public string VideConversation = null;

    [Tooltip("Index of the VIDE conversation for this character.")]
    public int VideConversationIndex = 0;

    [Tooltip("General standing pose sprite of the character.")]
    public Sprite characterPoseSprite = null;

    [Tooltip("Dialog sprite of the character.")]
    public Sprite characterDialogSprite = null;

    //public List<CommentList> comments = null; // or List<string>
}

public class CharacterEnums
{

    /// <summary>
    /// Gender of the character.
    /// </summary>
    public enum Gender
    {
        Unknown, Male, Female
    }

    /// <summary>
    /// Infection stage of the character.
    /// </summary>
    public enum InfectionStage
    {
        None, Stage1, Stage2, Stage3, Stage4, Final
    }

    /// <summary>
    /// Suspicion state of the character.
    /// </summary>
    public enum SuspicionState
    {
        None, Concern, Suspicion, Awareness, Fear, Panic, Alert, Intervention
    }
}
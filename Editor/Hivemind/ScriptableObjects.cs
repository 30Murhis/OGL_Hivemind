using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

/*
public class Comment : ScriptableObject
{
    public List<string> WorkPositive = null;
    public List<string> WorkNegative = null;
    public List<string> PersonalPositive = null;
    public List<string> PersonalNegative = null;
    public List<string> OutsideWorldPositive = null;
    public List<string> OutsideWorldNegative = null;

    public Comment()
    {
        WorkPositive = new List<string>();
        WorkNegative = new List<string>();
        PersonalPositive = new List<string>();
        PersonalNegative = new List<string>();
        OutsideWorldPositive = new List<string>();
        OutsideWorldNegative = new List<string>();
    }
}

// This can be used to list characters into categories
public class CommentList : ScriptableObject
{
    public List<Comment> commentList = new List<Comment>();
}
*/

public class Comment : ScriptableObject
{
    public string comment = "text";
}

public class CommentList : ScriptableObject
{
    public List<string> WorkPositive = new List<string>();
    public List<string> WorkNegative = new List<string>();
    public List<string> PersonalPositive = new List<string>();
    public List<string> PersonalNegative = new List<string>();
    public List<string> OutsideWorldPositive = new List<string>();
    public List<string> OutsideWorldNegative = new List<string>();
}

public class CreateCharacter
{
    [MenuItem("Assets/Create/New Character")]
    public static Character Create()
    {
        Character asset = ScriptableObject.CreateInstance<Character>();

        //double timeSinceUNIXEpoch = System.DateTime.Now.Subtract(System.DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
        double timeSinceUNIXEpoch = System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalMilliseconds;

        asset.characterName = "Character" + timeSinceUNIXEpoch;

        AssetDatabase.CreateAsset(asset, "Assets/ScriptableObjects/Characters/Character" + timeSinceUNIXEpoch.ToString() + ".asset");
        AssetDatabase.SaveAssets();
        return asset;
    }
}

public class CreateCharacters
{
    [MenuItem("Assets/Create/New Character List")]
    public static Characters Create()
    {
        Characters asset = ScriptableObject.CreateInstance<Characters>();

        AssetDatabase.CreateAsset(asset, "Assets/Characters.asset");
        AssetDatabase.SaveAssets();
        return asset;
    }
}

public class CreateComments
{
    [MenuItem("Assets/Create/New List Of Comments")]
    public static CommentList Create()
    {
        CommentList asset = ScriptableObject.CreateInstance<CommentList>();

        AssetDatabase.CreateAsset(asset, "Assets/Comments.asset");
        AssetDatabase.SaveAssets();
        return asset;
    }
}

public class CharacterEditor : EditorWindow
{
    Characters characters;
    int viewIndex = 1;
    string findCharacter = "";
    Vector2 scrollPosition = Vector2.zero;
    List<string> dialogues;

    [MenuItem("Window/Character Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(CharacterEditor));
    }

    void OnEnable()
    {
        if (EditorPrefs.HasKey("CharacterListObjectPath"))
        {
            string objectPath = EditorPrefs.GetString("CharacterListObjectPath");
            characters = AssetDatabase.LoadAssetAtPath(objectPath, typeof(Characters)) as Characters;
        }
        else
        {
            characters = AssetDatabase.LoadAssetAtPath<Characters>("Assets/Characters.asset") as Characters;
        }

        LoadVIDEDialogues();
    }

    void OnDestroy()
    {
        Save();
    }

    void OnGUI()
    {
        if (Event.current.type == EventType.MouseDown)
        {
            if (characters != null)
                LoadVIDEDialogues();
        }

        //EditorUtility.ClearProgressBar();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Character Editor", EditorStyles.boldLabel);
        if (characters && (characters.allCharacters != null) && characters.allCharacters.Count > 0)
        {
            if (GUILayout.Button("Rename assets", GUILayout.ExpandWidth(false)))
            {
                RenameAllCharacterAssets();
            }
            if (GUILayout.Button("Sort character list", GUILayout.ExpandWidth(false)))
            {
                OrderCharacterList();
            }
            if (GUILayout.Button("Save all", GUILayout.ExpandWidth(false)))
            {
                Save();
            }
        }
        else
        {
            if (GUILayout.Button("New list of characters", GUILayout.ExpandWidth(false)))
            {
                CreateNewCharacterList();
            }
        }
        GUILayout.EndHorizontal();

        if (characters)
        {
            GUILayout.Space(10);
            if (GUILayout.Button("DEBUG: Populate list", GUILayout.ExpandWidth(false)))
            {
                CreateMultipleNewCharacters(20);
            }
            GUILayout.Space(20);
            GUILayout.Label("List of all characters:", EditorStyles.boldLabel);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add a new character", GUILayout.ExpandWidth(false)))
            {
                CreateNewCharacter();
            }
            GUILayout.Space(16);
            if (GUILayout.Button("Find & add a character", GUILayout.ExpandWidth(false)))
            {
                FindCharacters();
            }
            GUILayout.Space(16);
            if (GUILayout.Button("Remove last character", GUILayout.ExpandWidth(false)))
            {
                RemoveCharacter();
            }
            GUIStyle btnR = new GUIStyle(GUI.skin.button);
            btnR.normal.textColor = Color.red;
            GUILayout.Space(16);
            if (GUILayout.Button("Remove all characters", btnR))
            {
                RemoveAllCharacters();
            }
            GUILayout.EndHorizontal();
            ScriptableObject target = characters;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty stringsProperty = so.FindProperty("allCharacters");
            EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
            so.ApplyModifiedProperties(); // Remember to apply modified properties

            if (characters.allCharacters.Count > 0)
            {
                GUILayout.Space(20);

                GUILayout.Label("Single Character Editor:", EditorStyles.boldLabel);

                GUILayout.BeginHorizontal();
                viewIndex = Mathf.Clamp(EditorGUILayout.IntField("Currently editing", viewIndex, GUILayout.ExpandWidth(false)), 1, characters.allCharacters.Count);
                GUILayout.Space(200);
                if (characters.allCharacters[viewIndex - 1])
                {
                    findCharacter = EditorGUILayout.TextField(findCharacter);
                    if (GUILayout.Button("Find (hit enter after typing)", GUILayout.ExpandWidth(false)))
                    {
                        FindCharacter(findCharacter);
                    }
                }
                GUILayout.EndHorizontal();


                //else
                //EditorGUILayout.LabelField("(Unknown - character/name not found/set)");


                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Prev", GUILayout.ExpandWidth(false)))
                {
                    if (viewIndex > 1)
                    {
                        viewIndex--;
                    }
                }
                GUILayout.Space(5);
                if (GUILayout.Button("Next", GUILayout.ExpandWidth(false)))
                {
                    if (viewIndex < characters.allCharacters.Count)
                    {
                        viewIndex++;
                    }
                }
                GUILayout.Space(61);
                if (GUILayout.Button("Delete this character", GUILayout.ExpandWidth(false)))
                {
                    RemoveCharacter(viewIndex - 1);
                    if (viewIndex > 1)
                    {
                        viewIndex--;
                    }
                    else return;
                }
                GUILayout.EndHorizontal();

                if (characters.allCharacters[viewIndex - 1])
                {
                    GUILayout.Space(10);
                    characters.allCharacters[viewIndex - 1].characterName = EditorGUILayout.TextField("Character Name", characters.allCharacters[viewIndex - 1].characterName as string);
                    characters.allCharacters[viewIndex - 1].spawnFloor = EditorGUILayout.IntSlider("Spawn Floor", characters.allCharacters[viewIndex - 1].spawnFloor, 0, 100);
                    //characters.allCharacters[viewIndex - 1].currentFloor = EditorGUILayout.IntSlider("Current Floor", characters.allCharacters[viewIndex - 1].currentFloor, 0, 100);
                    characters.allCharacters[viewIndex - 1].authorization = EditorGUILayout.TextField("Authorization", characters.allCharacters[viewIndex - 1].authorization as string);
                    characters.allCharacters[viewIndex - 1].priority = EditorGUILayout.IntField("Priority", characters.allCharacters[viewIndex - 1].priority);
                    characters.allCharacters[viewIndex - 1].infectionStageDuration = EditorGUILayout.IntField("Infection Stage Duration", characters.allCharacters[viewIndex - 1].infectionStageDuration);
                    characters.allCharacters[viewIndex - 1].animator = EditorGUILayout.ObjectField("Animator", characters.allCharacters[viewIndex - 1].animator, typeof(RuntimeAnimatorController), true) as RuntimeAnimatorController;
                    characters.allCharacters[viewIndex - 1].isOriginallyNPC = EditorGUILayout.Toggle("Is NPC At Start", characters.allCharacters[viewIndex - 1].isOriginallyNPC);
                    //characters.allCharacters[viewIndex - 1].isOriginallyInfected = EditorGUILayout.Toggle("Is Infected At Start", characters.allCharacters[viewIndex - 1].isOriginallyInfected);
                    characters.allCharacters[viewIndex - 1].isInteractable = EditorGUILayout.Toggle("Is Interactable", characters.allCharacters[viewIndex - 1].isInteractable);
                    characters.allCharacters[viewIndex - 1].isInfectable = EditorGUILayout.Toggle("Is Infectable", characters.allCharacters[viewIndex - 1].isInfectable);
                    characters.allCharacters[viewIndex - 1].isInanimateObject = EditorGUILayout.Toggle("Is Inanimate Object", characters.allCharacters[viewIndex - 1].isInanimateObject);
                    characters.allCharacters[viewIndex - 1].gender = (CharacterEnums.Gender)EditorGUILayout.EnumPopup("Gender", characters.allCharacters[viewIndex - 1].gender);
                    //characters.allCharacters[viewIndex - 1].currentStateOfInfection = (Character.InfectionState)EditorGUILayout.EnumPopup("State Of Infection", characters.allCharacters[viewIndex - 1].currentStateOfInfection);
                    //characters.allCharacters[viewIndex - 1].currentStateOfSuspicion = (Character.SuspicionState)EditorGUILayout.EnumPopup("State Of Suspicion", characters.allCharacters[viewIndex - 1].currentStateOfSuspicion);

                    GUILayout.BeginHorizontal();
                    if (dialogues.Count > 0)
                    {
                        EditorGUI.BeginChangeCheck();
                        characters.allCharacters[viewIndex - 1].VideConversationIndex = EditorGUILayout.Popup("Assigned Dialogue",characters.allCharacters[viewIndex - 1].VideConversationIndex, dialogues.ToArray());
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (characters.allCharacters[viewIndex - 1].VideConversation != dialogues[characters.allCharacters[viewIndex - 1].VideConversationIndex])
                            {
                                characters.allCharacters[viewIndex - 1].VideConversation = dialogues[characters.allCharacters[viewIndex - 1].VideConversationIndex];
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label("No saved Dialogues!");

                    }
                    GUILayout.EndHorizontal();

                    characters.allCharacters[viewIndex - 1].characterPoseSprite = EditorGUILayout.ObjectField("Standing pose sprite", characters.allCharacters[viewIndex - 1].characterPoseSprite, typeof(Sprite), false, GUILayout.ExpandWidth(false)) as Sprite;
                    characters.allCharacters[viewIndex - 1].characterDialogSprite = EditorGUILayout.ObjectField("Dialog sprite", characters.allCharacters[viewIndex - 1].characterDialogSprite, typeof(Sprite), false, GUILayout.ExpandWidth(false)) as Sprite;

                    GUILayout.Space(25);
                }
                else
                {
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("This character seems to be uninitialized. You need to initialize it first.");
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Initialize", GUILayout.ExpandWidth(false)))
                    {
                        InitializeCharacter(viewIndex - 1);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUIStyle lr = new GUIStyle(EditorStyles.label);
                lr.normal.textColor = new Color(0.7f, 0, 0);
                GUILayout.Space(10);
                EditorGUILayout.LabelField("List of characters seems to be empty.", lr);
                EditorGUILayout.LabelField("Add new ones by clicking the 'Add a new character' button above.", lr);
                EditorGUILayout.LabelField("Or try to find them by clicking the 'Find & add a character' button.", lr);
            }
            GUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.LabelField("No list of characters found.");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Find list of characters", GUILayout.ExpandWidth(false)))
            {
                FindCharactersList();
            }
            if (GUILayout.Button("Create a new list of characters", GUILayout.ExpandWidth(false)))
            {
                CreateNewCharacterList();
            }
            GUILayout.EndHorizontal();
        }
    }

    void LoadVIDEDialogues()
    {
        TextAsset[] files = Resources.LoadAll<TextAsset>("Dialogues");
        dialogues = new List<string>();
        //assignedIndex = 0;

        if (files.Length < 1) return;

        foreach (TextAsset f in files)
        {
            dialogues.Add(f.name);
        }
    }

    void InitializeCharacter(int listPos)
    {
        Character newCharacter = CreateCharacter.Create();
        if (newCharacter)
        {
            string relPath = AssetDatabase.GetAssetPath(newCharacter);
            //EditorPrefs.SetString("CharacterObjectPath_" + System.DateTime.Now.Millisecond, relPath);
            if (characters && (characters.allCharacters != null))
            {
                characters.allCharacters[listPos] = newCharacter;
            }
        }
    }

    void FindCharacter(string nameToSeek)
    {
        //if (characters.allCharacters.Any(c => c.characterName.Contains(charName)))
        {
            int foundIndex = characters.allCharacters.FindIndex(c => c.characterName.Contains(nameToSeek)) + 1;
            if (foundIndex > 0) viewIndex = foundIndex;
            else Debug.Log("No character found with \"" + nameToSeek + "\" in their name.");
        }
    }

    void FindCharacters()
    {
        string absPath = EditorUtility.OpenFilePanel("Find & add a character asset", "Assets/ScriptableObjects/Characters", "asset");
        if (absPath.StartsWith(Application.dataPath))
        {
            string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
            Character existingCharacter = AssetDatabase.LoadAssetAtPath(relPath, typeof(Character)) as Character;

            if (characters.allCharacters.Contains(existingCharacter))
            {
                EditorUtility.DisplayDialog("Conflicting characters", "Character list already contains this character.", "OK");
                Debug.LogWarning("Character list already contains this character.");
                return;
            }
            //if (comments.commentList == null)
            //    comments.commentList = new List<Comment>();
            if (existingCharacter)
            {
                //EditorPrefs.SetString("CharacterObjectPath", relPath);
                if (characters && (characters.allCharacters != null))
                {
                    characters.allCharacters.Add(existingCharacter);
                    EditorUtility.SetDirty(characters);
                }
            }
        }
    }

    void FindCharactersList()
    {
        string absPath = EditorUtility.OpenFilePanel("Find character list asset", "Assets/", "asset");
        if (absPath.StartsWith(Application.dataPath))
        {
            string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
            Characters characters = AssetDatabase.LoadAssetAtPath(relPath, typeof(Characters)) as Characters;
            EditorPrefs.SetString("CharacterListObjectPath", relPath);
            //if (comments.commentList == null)
            //    comments.commentList = new List<Comment>();
            if (characters)
            {
                this.characters = characters;
            }
        }
    }

    void OrderCharacterList()
    {
        characters.allCharacters = characters.allCharacters.OrderBy(c => c.characterName).ToList();
    }

    void CreateNewCharacterList()
    {
        // There is no overwrite protection here!
        // There is No "Are you sure you want to overwrite your existing object?" if it exists.
        // This should probably get a string from the user to create a new name and pass it ...
        //viewIndex = 1;
        characters = CreateCharacters.Create();
        if (characters)
        {
            string relPath = AssetDatabase.GetAssetPath(characters);
            EditorPrefs.SetString("CharacterListObjectPath", relPath);
        }
    }

    void CreateNewCharacter(int counter = 0)
    {
        //if (counter == 0) counter = System.DateTime.Now.Millisecond;
        Character newCharacter = CreateCharacter.Create();
        if (newCharacter)
        {
            string relPath = AssetDatabase.GetAssetPath(newCharacter);
            //EditorPrefs.SetString("CharacterObjectPath_" + counter, relPath);
            if (characters && (characters.allCharacters != null))
            {
                characters.allCharacters.Add(newCharacter);
                viewIndex++;
                EditorUtility.SetDirty(characters);
            }
        }
    }

    void CreateMultipleNewCharacters(int amount)
    {
        bool cancel;

        for (int i = 0; i < amount; i++)
        {
            cancel = EditorUtility.DisplayCancelableProgressBar(
                    "Progress",
                    "Creating characters",
                    ((float)i / (float)characters.allCharacters.Count));

            if (cancel)
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            CreateNewCharacter();
        }

        EditorUtility.ClearProgressBar();
        return;
    }

    void RemoveCharacter(int index = -1, bool skipConfirm = false)
    {
        if (characters.allCharacters.Count < 1) return;

        int removeIndex = (index > -1) ? index : characters.allCharacters.Count - 1;

        if (!skipConfirm)
        {
            if (!EditorUtility.DisplayDialog("Confirm removal",
               "Are you sure you want to delete " + characters.allCharacters[removeIndex].characterName + "?", "Yes", "No"))
            {
                return;
            }
        }
        
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(characters.allCharacters[removeIndex]));
        characters.allCharacters.RemoveAt(removeIndex);
    }

    void RemoveAllCharacters()
    {
        if (characters.allCharacters.Count < 1) return;

        bool cancel;
        int counter = 0;

        if (EditorUtility.DisplayDialog("Confirm removal",
               "Are you sure you want to delete all " + characters.allCharacters.Count + " characters?", "Yes", "No"))
        {
            if (!EditorUtility.DisplayDialog("Extra confirmation",
               "Are you really really sure you really want to delete ALL " + characters.allCharacters.Count + " characters?", "Yes", "No"))
                return;

                while (characters.allCharacters.Count>0)
            {
                cancel = EditorUtility.DisplayCancelableProgressBar(
                           "Progress",
                           "Deleting characters",
                           ((float)counter / (float)characters.allCharacters.Count));

                if (cancel)
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }

                counter++;
                RemoveCharacter(0, true);
            }
        }

        EditorUtility.ClearProgressBar();
    }

    void RenameAllCharacterAssets()
    {
        int counter = 0;
        bool cancel = false;
        List<string> appearedNames = new List<string>();
        foreach (Character c in characters.allCharacters)
        {
            cancel = EditorUtility.DisplayCancelableProgressBar(
                       "Progress",
                       "Renaming characters",
                       ((float)counter / (float)characters.allCharacters.Count));

            if (cancel)
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            if (c == null) continue;

            string prevName = c.name;
            string newName = c.characterName;
            if (newName == prevName) continue; // Don't update this if name has not changed

            int counterN = 0;
            for (int i = 0; i < appearedNames.Count; i++)
            {
                if (appearedNames[i] == newName)
                {
                    counterN++;
                }
            }

            if (counterN > 0) newName += "_" + (counterN + 1).ToString();

            appearedNames.Add(c.characterName);
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(c), newName);
            counter++;
        }
        if (counter > 0)
            AssetDatabase.SaveAssets();
        
        EditorUtility.ClearProgressBar();
    }

    void Save()
    {
        foreach (Character c in characters.allCharacters)
        {
            EditorUtility.SetDirty(c);
        }
        EditorUtility.SetDirty(characters);
        AssetDatabase.SaveAssets();
        Debug.Log("Assets saved.");
    }
}


public class CommentEditor : EditorWindow
{
    public CommentList comments;
    private int viewIndex = 1;
    public Vector2 scrollPosition = Vector2.zero;

    [MenuItem("Window/Comment editor %#e")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(CommentEditor));
    }

    void OnEnable()
    {
        if (EditorPrefs.HasKey("CommentObjectPath"))
        {
            string objectPath = EditorPrefs.GetString("CommentObjectPath");
            comments = AssetDatabase.LoadAssetAtPath(objectPath, typeof(CommentList)) as CommentList;
        }

    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Comment Editor", EditorStyles.boldLabel);
        if (comments != null)
        {
            if (GUILayout.Button("Show Comment Asset", GUILayout.ExpandWidth(false)))
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = comments;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Open Comment List", GUILayout.ExpandWidth(false)))
        {
            OpenCommentList();
        }
        if (GUILayout.Button("New Comment List", GUILayout.ExpandWidth(false)))
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = comments;
        }
        GUILayout.EndHorizontal();

        if (comments == null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("Create New Comment List", GUILayout.ExpandWidth(false)))
            {
                CreateNewCommentList();
            }
            if (GUILayout.Button("Open Existing Comment List", GUILayout.ExpandWidth(false)))
            {
                OpenCommentList();
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(20);

        if (comments != null)
        {
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Prev", GUILayout.ExpandWidth(false)))
            {
                if (viewIndex > 1)
                    viewIndex--;
                else
                    viewIndex = 3;
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Next", GUILayout.ExpandWidth(false)))
            {
                if (viewIndex < 3)
                    viewIndex++;
                else viewIndex = 1;
            }/*
            GUILayout.Space(60);

            if (GUILayout.Button("Add Item", GUILayout.ExpandWidth(false)))
            {
                AddItem();
            }
            if (GUILayout.Button("Delete Item", GUILayout.ExpandWidth(false)))
            {
                DeleteItem(viewIndex - 1);
            }
            
            */
            GUILayout.EndHorizontal();
            //if (comments.commentList == null)
            //   Debug.Log("wtf");

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            if (viewIndex == 1)
            {
                //////////
                // WORK //
                //////////
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Work", EditorStyles.boldLabel);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Zero list", GUILayout.ExpandWidth(false)))
                {
                    ZeroList(0);
                }
                GUILayout.Space(86);
                if (GUILayout.Button("Add line", GUILayout.ExpandWidth(false)))
                {
                    AddCommentLine(0);
                }
                GUILayout.EndHorizontal();
                if (comments.WorkPositive != null)
                {
                    ScriptableObject target = comments;
                    SerializedObject so = new SerializedObject(target);
                    SerializedProperty stringsProperty = so.FindProperty("WorkPositive");

                    EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
                    so.ApplyModifiedProperties(); // Remember to apply modified properties
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Space(154);
                if (GUILayout.Button("Add line", GUILayout.ExpandWidth(false)))
                {
                    AddCommentLine(1);
                }
                GUILayout.EndHorizontal();
                if (comments.WorkNegative != null)
                {
                    ScriptableObject target = comments;
                    SerializedObject so = new SerializedObject(target);
                    SerializedProperty stringsProperty = so.FindProperty("WorkNegative");

                    EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
                    so.ApplyModifiedProperties(); // Remember to apply modified properties
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            if (viewIndex == 2)
            {
                //////////////
                // PERSONAL //
                //////////////
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Personal", EditorStyles.boldLabel);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Space(154);
                if (GUILayout.Button("Add line", GUILayout.ExpandWidth(false)))
                {
                    AddCommentLine(2);
                }
                GUILayout.EndHorizontal();
                if (comments.WorkPositive != null)
                {
                    ScriptableObject target = comments;
                    SerializedObject so = new SerializedObject(target);
                    SerializedProperty stringsProperty = so.FindProperty("PersonalPositive");

                    EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
                    so.ApplyModifiedProperties(); // Remember to apply modified properties
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Space(154);
                if (GUILayout.Button("Add line", GUILayout.ExpandWidth(false)))
                {
                    AddCommentLine(3);
                }
                GUILayout.EndHorizontal();
                if (comments.WorkNegative != null)
                {
                    ScriptableObject target = comments;
                    SerializedObject so = new SerializedObject(target);
                    SerializedProperty stringsProperty = so.FindProperty("PersonalNegative");

                    EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
                    so.ApplyModifiedProperties(); // Remember to apply modified properties
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            if (viewIndex == 3)
            {
                /////////////
                // OUTSIDE //
                /////////////
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Outside World", EditorStyles.boldLabel);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Space(154);
                if (GUILayout.Button("Add line", GUILayout.ExpandWidth(false)))
                {
                    AddCommentLine(4);
                }
                GUILayout.EndHorizontal();
                if (comments.WorkPositive != null)
                {
                    ScriptableObject target = comments;
                    SerializedObject so = new SerializedObject(target);
                    SerializedProperty stringsProperty = so.FindProperty("OutsideWorldPositive");

                    EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
                    so.ApplyModifiedProperties(); // Remember to apply modified properties
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Space(154);
                if (GUILayout.Button("Add line", GUILayout.ExpandWidth(false)))
                {
                    AddCommentLine(5);
                }
                GUILayout.EndHorizontal();
                if (comments.WorkNegative != null)
                {
                    ScriptableObject target = comments;
                    SerializedObject so = new SerializedObject(target);
                    SerializedProperty stringsProperty = so.FindProperty("OutsideWorldNegative");

                    EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
                    so.ApplyModifiedProperties(); // Remember to apply modified properties
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(comments);
        }
    }

    void ZeroList(int index)
    {
        if (EditorUtility.DisplayDialog("List zeroing.",
            "Are you sure you want to reset this list?", "Yes", "No"))
        {
            if (index == 0) comments.WorkPositive = new List<string>();
            if (index == 1) comments.WorkNegative = new List<string>();
            if (index == 2) comments.PersonalPositive = new List<string>();
            if (index == 3) comments.PersonalPositive = new List<string>();
            if (index == 4) comments.OutsideWorldPositive = new List<string>();
            if (index == 5) comments.OutsideWorldPositive = new List<string>();
        }

    }

    void AddCommentLine(int index)
    {
        if (index == 0) comments.WorkPositive.Add("");
        if (index == 1) comments.WorkNegative.Add("");
        if (index == 2) comments.PersonalPositive.Add("");
        if (index == 3) comments.PersonalPositive.Add("");
        if (index == 4) comments.OutsideWorldPositive.Add("");
        if (index == 5) comments.OutsideWorldPositive.Add("");
    }

    void CreateNewCommentList()
    {
        // There is no overwrite protection here!
        // There is No "Are you sure you want to overwrite your existing object?" if it exists.
        // This should probably get a string from the user to create a new name and pass it ...
        viewIndex = 1;
        comments = CreateComments.Create();
        if (comments)
        {
            //comments.commentList = new List<Comment>();
            string relPath = AssetDatabase.GetAssetPath(comments);
            EditorPrefs.SetString("CommentObjectPath", relPath);
        }
    }

    void OpenCommentList()
    {
        string absPath = EditorUtility.OpenFilePanel("Select Inventory Item List", "", "");
        if (absPath.StartsWith(Application.dataPath))
        {
            string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
            comments = AssetDatabase.LoadAssetAtPath(relPath, typeof(CommentList)) as CommentList;
            //if (comments.commentList == null)
            //    comments.commentList = new List<Comment>();
            if (comments)
            {
                EditorPrefs.SetString("CommentObjectPath", relPath);
            }
        }
    }

    void AddItem()
    {
        Comment newItem = new Comment();
        //newItem.itemName = "New Item";
        //comments.commentList.Add(newItem);
        //viewIndex = comments.commentList.Count;
    }

    void DeleteItem(int index)
    {
        //comments.commentList.RemoveAt(index);
    }
}

#endif
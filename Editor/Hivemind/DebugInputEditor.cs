using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DebugInput))]
public class DebugInputEditor : Editor
{
    int defaultFieldWidth = 20;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DebugInput myTarget = (DebugInput)target;

        EditorGUILayout.LabelField("Debug keys for testing purposes. These will be automatically disabled on builds.", EditorStyles.helpBox);

        /* Not needed anymore
        if (GUILayout.Button("Open DebugInput.cs"))
        {
            MonoScript monoscript = MonoScript.FromMonoBehaviour(myTarget);
            string scriptPath = AssetDatabase.GetAssetPath(monoscript);
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(scriptPath, 0);
        }
        */

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Enable Mouse Wheel Camera Zoom");
        myTarget.allowCameraZoom = EditorGUILayout.Toggle(myTarget.allowCameraZoom, GUILayout.Width(defaultFieldWidth));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Key Bindings", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Kill Current Character");
        myTarget.keyKillCurrentCharacter = EditorGUILayout.TextField(myTarget.keyKillCurrentCharacter, GUILayout.Width(defaultFieldWidth));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Warp Target To Current");
        myTarget.keyWarpTargetToCurrent = EditorGUILayout.TextField(myTarget.keyWarpTargetToCurrent, GUILayout.Width(defaultFieldWidth));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Display Comment On Target");
        myTarget.keyChangeCommentOfTargetChar = EditorGUILayout.TextField(myTarget.keyChangeCommentOfTargetChar, GUILayout.Width(defaultFieldWidth));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Move Target Left");
        myTarget.keyMoveTargetCharLeft = EditorGUILayout.TextField(myTarget.keyMoveTargetCharLeft, GUILayout.Width(defaultFieldWidth));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Move Target Right");
        myTarget.keyMoveTargetCharRight = EditorGUILayout.TextField(myTarget.keyMoveTargetCharRight, GUILayout.Width(defaultFieldWidth));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Run With Target");
        myTarget.keyRunWithTargetChar = EditorGUILayout.TextField(myTarget.keyRunWithTargetChar, GUILayout.Width(defaultFieldWidth));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Spawn Random Character");
        myTarget.keySpawnRandomCharacter = EditorGUILayout.TextField(myTarget.keySpawnRandomCharacter, GUILayout.Width(defaultFieldWidth));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Spawn Random Character Randomly");
        myTarget.keySpawnRandomCharacterSomewhere = EditorGUILayout.TextField(myTarget.keySpawnRandomCharacterSomewhere, GUILayout.Width(defaultFieldWidth));
        EditorGUILayout.EndHorizontal();

        /* Buttons
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Warp Target To Current Position"))
        {
            myTarget.WarpTargetToCurrent();
        }
        if (GUILayout.Button("Spawn Random Character Somewhere"))
        {
            myTarget.SpawnRandomCharacter(false);
        }
        */

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Objects", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("Target Character");
        myTarget.targetChar = EditorGUILayout.ObjectField("Target Character", myTarget.targetChar, typeof(GameObject), true) as GameObject;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("List Of Characters (asset)");
        myTarget.listOfCharacters = EditorGUILayout.ObjectField(myTarget.listOfCharacters, typeof(Characters), false, GUILayout.ExpandWidth(true)) as Characters;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUIStyle redLabel = new GUIStyle(EditorStyles.helpBox);
        redLabel.normal.textColor = new Color(0.5f, 0, 0);
        EditorGUILayout.LabelField("Notice: Interaction with spawned characters will not work perfectly yet.", redLabel);
    }
}
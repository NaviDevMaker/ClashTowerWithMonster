using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;

public class ClipNameRenamer : EditorWindow
{
    [MenuItem("Tools/State Based Clip Renamer")]
    static void OpenWindow()
    {
        GetWindow<ClipNameRenamer>("State Based Renamer");
    }

    List<AnimatorController> controllers = new List<AnimatorController>();
    Dictionary<AnimatorController, string[]> originalClipNames = new Dictionary<AnimatorController, string[]>();

    const string RemoveSolo = "Remove Animator";
    const string RemoveAll = "Remove All Animators";
    const string AddAnimator = "Add Animator";
    const string Rename = "Rename Clips by State Name";

    const string jsonDataName = "ClipNameRenamer_savedata";
    void OnGUI()
    {
        GUILayout.Label("Animators");

        for (int i = 0; i < controllers.Count; i++)
        {
            var current = controllers[i];
            var controllerToAdd = (AnimatorController)EditorGUILayout.ObjectField("Animator", current, typeof(AnimatorController), false);

            if (controllerToAdd == current)
                continue;

            // 割り当てようとしているコントローラが既にリスト内に存在する場合は警告を出す
            if (controllerToAdd != null && controllers.Contains(controllerToAdd))
            {
                EditorGUILayout.HelpBox("This AnimatorController is already added.", MessageType.Warning);
                continue;
            }

            // 以前のコントローラをnullにする場合
            if (controllerToAdd == null)
            {
                if (current != null && originalClipNames.ContainsKey(current))
                {
                    //名前が変更されていた場合、nullがはいっていることはない
                    var stringArray = originalClipNames[current];
                    var hasNullElement = stringArray.All(name => name == null);
                    if (!hasNullElement)
                    {
                        var confirm = DisplayConfirmDialog(RemoveSolo);
                        if (confirm)
                        {
                            Renameback(current);
                            Debug.Log($"Removed AnimatorController: {current.name}");
                            originalClipNames.Remove(current);
                            controllers[i] = null;
                        }
                    }
                    else
                    {
                        originalClipNames.Remove(current);
                        controllers[i] = null;
                    }
                             
                } 
            }
            else
            {
                // 以前のコントローラを置き換える場合
                if (current != null && originalClipNames.ContainsKey(current))
                {

                    //名前が変更されていた場合、nullがはいっていることはない
                    var stringArray = originalClipNames[current];
                    var hasNullElement = stringArray.All(name => name == null);
                    if (!hasNullElement)
                    {
                        var confirm = DisplayConfirmDialog(RemoveSolo);
                        if (confirm)
                        {
                            Renameback(current);
                            Debug.Log($"Swapped AnimatorController: {current.name} -> {controllerToAdd.name}");
                            originalClipNames.Remove(current);
                            controllers[i] = controllerToAdd;
                            originalClipNames.Add(controllerToAdd, new string[0]);
                        }
                    }   
                    else
                    {
                        originalClipNames.Remove(current);
                        controllers[i] = controllerToAdd;
                        originalClipNames.Add(controllerToAdd, new string[0]);
                    }
                }
                else 
                {
                    controllers[i] = controllerToAdd;
                    originalClipNames.Add(controllerToAdd, new string[0]);
                }
            }
        }

        if (GUILayout.Button(AddAnimator))
        {
            controllers.Add(null);
        }

        if (GUILayout.Button(RemoveSolo))
        {
            if (controllers.Count == 0) return;

            int index = controllers.Count - 1;
            var removeController = controllers[index];
            if (removeController != null)
            {
                var confirm = DisplayConfirmDialog(RemoveSolo);
                if (confirm)
                {

                    if (removeController != null && originalClipNames.ContainsKey(removeController))
                    {
                        Renameback(removeController);
                        originalClipNames.Remove(removeController);
                    }
                    controllers.RemoveAt(index);
                }
            }
            else
            {
                controllers.RemoveAt(index);
            }
        }

        if (GUILayout.Button(RemoveAll))
        {
            if (controllers.Count == 0) return;

            var confirm = DisplayConfirmDialog(RemoveAll);
            if (confirm)
            {
                foreach (var c in controllers)
                {
                    if (c != null && originalClipNames.ContainsKey(c))
                    {
                        Renameback(c);
                    }
                }
                controllers.Clear();
                originalClipNames.Clear();
            }
        }

        GUILayout.Space(25);
        if (GUILayout.Button(Rename))
        {
            if (controllers.Count == 0)
            {
                Debug.LogWarning("AnimatorController を指定してください。");
                return;
            }

            var confirm = DisplayConfirmDialog(Rename);
            if (confirm)
            {
                RenameClipsByStateName();
            }
        }
    }

    void RenameClipsByStateName()
    {
        foreach (var controller in controllers)
        {
            if (controller == null) continue;

            var clipNames = new List<string>();
            foreach (var layer in controller.layers)
            {
                var states = layer.stateMachine.states;

               
                for (int i = 0; i < states.Length; i++)
                {
                    var state = states[i].state;
                    if (state.motion is AnimationClip clip)
                    {
                        string targetName = state.name;
                        string path = AssetDatabase.GetAssetPath(clip);

                        if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(path)) continue;
                        clipNames.Add(clip.name);
                        AssetDatabase.RenameAsset(path, targetName);
                        Debug.Log($"Renamed Clip '{clip.name}' to '{targetName}' (from state '{state.name}')");
                    }
                }

            }

            originalClipNames[controller] = clipNames.ToArray();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    void Renameback(AnimatorController controller)
    {
        Debug.Log("名前をもとに戻します");
        if (controller == null || !originalClipNames.ContainsKey(controller)) return;

        int count = 0;
        foreach (var layer in controller.layers)
        {
            var states = layer.stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                var state = states[i].state;
                if (state.motion is AnimationClip clip)
                {
                    string originalName = originalClipNames[controller].Length > count ? originalClipNames[controller][count] : null;
                    string path = AssetDatabase.GetAssetPath(clip);
                    if (!string.IsNullOrEmpty(originalName) && !string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.RenameAsset(path, originalName);
                        Debug.Log($"Reverted Clip '{clip.name}' to original name '{originalName}'");
                    }
                    count++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    bool DisplayConfirmDialog(string processName)
    {
        switch (processName)
        {
            case RemoveSolo:
                return EditorUtility.DisplayDialog(
                    "Confirm",
                    "Are you sure you want to rename the animation clip back to its original name?",
                    "Agree", "Cancel"
                );

            case RemoveAll:
                return EditorUtility.DisplayDialog(
                    "Confirm",
                    "Are you sure you want to rename all animation clips back to their original names?",
                    "Agree", "Cancel"
                );

            case Rename:
                return EditorUtility.DisplayDialog(
                    "Confirm",
                    "Are you sure you want to rename all animation clips to match their state names?",
                    "Agree", "Cancel"
                );

            default:
                return false;
        }
    }

    void SaveData()
    {
        var saveData = new SaveData();
        foreach (var controller in controllers)
        {
           Debug.Log(controller);
            var path = AssetDatabase.GetAssetPath(controller);
            saveData.controllerPathes.Add(path);

            if(controller != null)
            {
                if (originalClipNames.TryGetValue(controller, out var originalNames))
                {
                    var arrayToString = string.Join("|",originalNames);
                    saveData.originalClipnames.Add(arrayToString);
                }
                else
                {
                    saveData.originalClipnames.Add(null);
                }
            }
            else
            {
                saveData.originalClipnames.Add(null);
            }
        }

        Debug.Log($"{saveData.originalClipnames.Count},{saveData.controllerPathes.Count}");
        string json = JsonUtility.ToJson(saveData);
        EditorPrefs.SetString(jsonDataName, json);
    }

    void LoadData()
    {
        if(controllers != null) controllers.Clear();
        if(originalClipNames != null) originalClipNames.Clear();
        
        if (!EditorPrefs.HasKey(jsonDataName)) return;

        string json = EditorPrefs.GetString(jsonDataName);
        var saveData = JsonUtility.FromJson<SaveData>(json);

        Debug.Log($"{saveData.originalClipnames.Count},{saveData.controllerPathes.Count}");
        for (int i = 0; i < saveData.controllerPathes.Count; i++)
        { 
            var path = saveData.controllerPathes[i];
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            var originalNames = saveData.originalClipnames[i];
            var stringToArray = string.IsNullOrEmpty(originalNames)? originalNames.Split("|") : new string[0];
            controllers.Add(controller);
            if(controller != null) originalClipNames[controller] = stringToArray;
        }
    }

    private void OnDisable()
    {
        SaveData();
    }

    private void OnEnable()
    {
        //EditorPrefs.DeleteKey("ClipNameRenamer_savedata");
        LoadData();
    }
}
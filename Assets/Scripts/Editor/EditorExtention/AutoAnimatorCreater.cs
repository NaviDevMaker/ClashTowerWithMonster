using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AutoAnimatorCreater:EditorWindow
{
    [MenuItem("Tools/Auto Animator Creater")]
    
    static void OpenWindow()
    {
        GetWindow<AutoAnimatorCreater>("Animator Creater");
    }

    List<string> unitNames = new List<string>();
    List<string> lastFolderNames = new List<string>();


    private void OnGUI()
    {
        for (int i = 0; i < unitNames.Count; i++) 
        {
            GUILayout.Label($"Field{i + 1}");
            GUI.SetNextControlName($"Field{i}");
            var currentUnitName = unitNames[i];
            var unitNameToAdd = EditorGUILayout.TextField(
                new GUIContent("controllerName","生成したいコントローラーの名前を設定します"),
                currentUnitName);
            var currentLastFolderName = lastFolderNames[i];
            var lastFolderNameToAdd = EditorGUILayout.TextField(
                new GUIContent("lastFolderName", "置きたいFolderを設定します"),
                currentLastFolderName
                );
            unitNames[i] = unitNameToAdd;
            lastFolderNames[i] = lastFolderNameToAdd;
        }

        if(GUILayout.Button("Add Controller Field"))
        {
            unitNames.Add(null);
            lastFolderNames.Add(null);
        }

        GUILayout.Space(10);

        if(GUILayout.Button("Remove Controller Field"))
        {
            var focusedFieldName = GUI.GetNameOfFocusedControl();
            for (int i = 0; i < unitNames.Count; i++)
            {
                var fieldName = $"Field{i}";
                if(focusedFieldName == fieldName)
                {
                    var confirm = EditorUtility.DisplayDialog
                    (
                        "Warning",
                        $"The fields \"{unitNames[i]}\" and \"{lastFolderNames[i]}\" will be removed from the list." +
                        $"Are you sure?(This only removes the fields. The actual controller assets in your project will not be deleted.) ",
                        "Agree","Cancel"
                    );
                    if (!confirm) break;
                    unitNames.RemoveAt(i);
                    lastFolderNames.RemoveAt(i);
                    GUI.FocusControl(null);
                    break;
                }
            }
        }

        if (GUILayout.Button("Create Animation Controllers"))
        {
            CreateAnimatorControllers();
        }

        void CreateAnimatorControllers()
        {
            var animatorParametor = AssetDatabase.LoadAssetAtPath<MonsterAnimatorPar>("Assets/Animations/Monsters/MonsterAnimatorPar.asset");
            if (animatorParametor == null) throw new System.Exception("The asset does't exist in your project!!");
            for (int i = 0;i < unitNames.Count; i++)
            {
                var lastFolderName = lastFolderNames[i];
                var unitName = unitNames[i];
                var folderpath = $"Assets/Animations/Monsters/{unitName}";
                if(!File.Exists(folderpath))
                {
                    System.IO.Directory.CreateDirectory(folderpath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                var controllerPath = System.IO.Path.Combine(folderpath, $"{unitName}AnimatorController.controller");
                var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                var attackPar = animatorParametor.Attack;
                var chasePar = animatorParametor.Chase;
                var deathPar = animatorParametor.Death;

                var idleState = "Idle";
                var attackStateName = animatorParametor.attackAnimClipName;
                var chaseStateName = animatorParametor.chaseAnimClipName;
                var deathStateName = animatorParametor.deathAnimClipName;

                controller.AddParameter(attackPar, AnimatorControllerParameterType.Bool);
                controller.AddParameter(chasePar, AnimatorControllerParameterType.Bool);
                controller.AddParameter(deathPar, AnimatorControllerParameterType.Trigger);

                var stateMachine = controller.layers[0].stateMachine;
                var idle = stateMachine.AddState(idleState);
                var attack = stateMachine.AddState(attackStateName);
                var chase = stateMachine.AddState(chaseStateName);
                var death = stateMachine.AddState(deathStateName);

                var toChase = idle.AddTransition(chase);
                toChase.AddCondition(AnimatorConditionMode.If,0,chasePar);
                toChase.hasExitTime = false;
                toChase.duration = 0f;

                var toAttack = chase.AddTransition(attack);
                toAttack.AddCondition(AnimatorConditionMode.If,0, attackPar);
                toAttack.hasExitTime = false;
                toAttack.duration = 0f;

                var toChaseFromAttack = attack.AddTransition(chase);
                toChaseFromAttack.AddCondition(AnimatorConditionMode.IfNot,0,attackPar);
                toChaseFromAttack.hasExitTime = false;
                toChaseFromAttack.duration = 0f;

                var toDeath = stateMachine.AddAnyStateTransition(death);
                toDeath.AddCondition(AnimatorConditionMode.If,0,deathPar);
                toDeath.hasExitTime = false;
                toDeath.duration = 0f;

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}

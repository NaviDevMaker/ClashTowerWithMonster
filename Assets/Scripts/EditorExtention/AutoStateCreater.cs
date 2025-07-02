using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
public class AutoStateCreater : EditorWindow
{
    [MenuItem("Tools/Auto State Creater")]

    static void OpenWindow()
    {
        GetWindow<AutoStateCreater>("State Scripts Creater");
    }

    string[] stateNames_Monster = { "IdleState", "ChaseState", "AttackState", "DeathState" };
    string[] stateNames_Player = { "IdleState", "MoveState", "AttackState", "DeathState","CheckEnemyState" };

    List<UnitBase> unitBases = new List<UnitBase>();
    List<string> lastFolderNames = new List<string>();
    List<string> lastNameSpaceNames = new List<string>();
    private void OnGUI()
    {
        for (int i = 0; i < unitBases.Count; i++)
        {
            {
                var currentBase = unitBases[i];
                var unitBaseToAdd = (UnitBase)EditorGUILayout.ObjectField(
                    new GUIContent("Unit", "ユニットの参照を設定します（UnitBaseを継承したPrefabなど）"),
                    currentBase,
                    typeof(UnitBase),
                    true);
                var currentFolderName = lastFolderNames[i];
                var lastFolderNameToAdd = EditorGUILayout.TextField(
                    new GUIContent("Last Folder Name", "置きたいFolderを設定します"),
                    currentFolderName);
                var currentNameSpaceName = lastNameSpaceNames[i];
                var nameSpaceToAdd = EditorGUILayout.TextField(
                    new GUIContent("Last Name Space Name", "生成するスクリプトのnamespaceを設定します"),
                    currentNameSpaceName);
                unitBases[i] = unitBaseToAdd;
                lastFolderNames[i] = lastFolderNameToAdd;
                lastNameSpaceNames[i] = nameSpaceToAdd;
            }
            GUILayout.Space(5);

        }
        if (GUILayout.Button("Add Unit Field"))
        {
            unitBases.Add(null);
            lastFolderNames.Add(null);
            lastNameSpaceNames.Add(null);
        }
        GUILayout.Space(10);

        if (GUILayout.Button("Create States"))
        {
            for (int i = 0; i < unitBases.Count; i++)
            {
                var classType = unitBases[i].GetType();
                var classTypeName = classType.Name;
                string nameSpaceName = null;
                if (unitBases[i] is IMonster)
                {
                    foreach (var name in stateNames_Monster)
                    {

                        var className = name;
                        var folderName = lastFolderNames[i];
                        var folderPath = $"Assets/Scripts/Monsters/{folderName}";
                        nameSpaceName = $"Game.Monsters.{lastNameSpaceNames[i]}";
                        if (!System.IO.Directory.Exists(folderPath))
                        {
                            System.IO.Directory.CreateDirectory(folderPath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                        string fullPath = System.IO.Path.Combine(folderPath, $"{className}.cs");
                        if (System.IO.File.Exists(fullPath))
                        {
                            Debug.LogError($"This state class name is {className}.cs is already exists!!");
                            continue;
                        }
                        CreateStates(classTypeName, className, fullPath, nameSpaceName);
                    }
                }
                else if (unitBases[i] is IPlayer)
                {

                    foreach (var name in stateNames_Player)
                    {

                        var className = name;
                        var folderName = lastFolderNames[i];
                        var folderPath = $"Assets/Scripts/Player/{folderName}";
                        nameSpaceName = $"Game.Players.{lastNameSpaceNames[i]}";
                        if (!System.IO.Directory.Exists(folderPath))
                        {
                            System.IO.Directory.CreateDirectory(folderPath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                        string fullPath = System.IO.Path.Combine(folderPath, $"{className}.cs");
                        if (System.IO.File.Exists(fullPath))
                        {
                            Debug.LogError($"This state class name is {className}.cs is already exists!!");
                            continue;
                        }
                        CreateStates(classTypeName, className, fullPath, nameSpaceName);
                    }
                }
            }
        }
    }
    void CreateStates(string classTypeName,string className,string fullPath,string namespaceName)
    {
        //string fullPath = System.IO.Path.Combine(path, $"{className}.cs");
        //if (System.IO.File.Exists(fullPath))
        //{
        //     Debug.LogError($"This state class name is {className}.cs is already exists!!");
        //     return;
        //}
        //var baseClassName = $"{className}Base<{classTypeName}>";
        var content = className switch
        {
            "IdleState" => IdleStateScriptContent(classTypeName,namespaceName),
            "ChaseState" => ChaseStateScriptContent(classTypeName,namespaceName),
            "AttackState" => AttackStateScriptContent(classTypeName,namespaceName),
            "DeathState" => DeathStateScriptContent(classTypeName,namespaceName),
             _ => null
        };//$@"using UnityEngine;
        if (content == null) return;
       
//namespace {nameSpaceName}
//{{
//    public class {className} : {baseClassName}
//    {{
//        public {className}({classTypeName} controller) : base(controller) {{ }}

//        public override void OnEnter()
//        {{
//            base.OnEnter();
//        }}
//        public override void OnUpdate()
//        {{
//            base.OnUpdate();
//        }}
//        public override void OnExit()
//        {{
//            base.OnExit();
//        }}
//    }}

//}}";


            System.IO.File.WriteAllText(fullPath, content);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
    }
    
    string IdleStateScriptContent(string classTypeName,string namespaceName)
    {
        var content = $@"using UnityEngine;
using Cysharp.Threading.Tasks;
namespace {namespaceName}
{{
    public class IdleState : IdleStateBase<{classTypeName}>
    {{
        public IdleState({classTypeName} controller) : base(controller) {{ }}


        public override void OnEnter()
        {{
            OnEnterProcess().Forget();
        }}
        public override void OnUpdate()
        {{
           base.OnUpdate();
        }}
        public override void OnExit()
        {{
            base.OnExit();
        }}
        protected override async UniTask OnEnterProcess()
        {{
            await base.OnEnterProcess();
        }}

    }}

}}";
        return content;
    }

    string AttackStateScriptContent(string classTypeName,string namespaceName)
    {
        
        var content = $@"using UnityEngine;

namespace {namespaceName}
{{
    public class AttackState : AttackStateBase<{classTypeName}>
    {{
        public AttackState({classTypeName} controller) : base(controller) {{ }}

        public override void OnEnter()
        {{
            base.OnEnter();

            //This paremetars are examples,so please change it to your preference!!
            if(attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<{classTypeName}>(controller, this, clipLength, 10, 1.0f);      
        }}
        public override void OnUpdate()
        {{
            base.OnUpdate();
        }}
        public override void OnExit()
        {{
            base.OnExit();
        }}
    }}

}}";

        return content;
    }

    string ChaseStateScriptContent(string classTypeName, string namespaceName)
    {
        var content = $@"using UnityEngine;

namespace {namespaceName}
{{
    public class ChaseState : ChaseStateBase<{classTypeName}>
    {{
        public ChaseState({classTypeName} controller) : base(controller) {{ }}

        public override void OnEnter()
        {{
            base.OnEnter();
        }}
        public override void OnUpdate()
        {{
            base.OnUpdate();
        }}
        public override void OnExit()
        {{
            base.OnExit();
        }}
    }}

}}";
        return content;
    }

    string DeathStateScriptContent(string classTypeName, string namespaceName)
    {
        var content = $@"using UnityEngine;

namespace {namespaceName}
{{
    public class DeathState : DeathStateBase<{classTypeName}>
    {{
        public DeathState({classTypeName} controller) : base(controller) {{ }}

        public override void OnEnter()
        {{
            base.OnEnter();
        }}
        public override void OnUpdate()
        {{
            base.OnUpdate();
        }}
        public override void OnExit()
        {{
            base.OnExit();
        }}
    }}

}}";
        return content;
    }
}

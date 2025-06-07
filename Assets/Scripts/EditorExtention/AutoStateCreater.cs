using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
public class AutoStateCreater : EditorWindow
{
    [MenuItem("Tools/Auto State Creater")]

    static void OpenWindow()
    {
        GetWindow<AutoStateCreater>("State Scripts Creater");
    }

    enum UnitType
    {
       Monster,
       Player,
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
            for (int i = 0; i < unitBases.Count;i++)
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
                        CreateStates(classTypeName, className,folderName,folderPath,nameSpaceName);
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
                        CreateStates(classTypeName, className, folderName, folderPath,nameSpaceName);
                    }
                }
            }
        }

        void CreateStates(string classTypeName, string className,string folderName,string path,string nameSpaceName)
        {
            string fullPath = System.IO.Path.Combine(path, $"{className}.cs");
            if (System.IO.File.Exists(fullPath))
            {
                Debug.LogError($"This state class name is {className}.cs is already exists!!");
                return;
            }
            var baseClassName = $"{className}Base<{classTypeName}>";
            var content = $@"using UnityEngine;

       
namespace {nameSpaceName}
{{
    public class {className} : {baseClassName}
    {{
        public {className}({classTypeName} controller) : base(controller) {{ }}

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


            System.IO.File.WriteAllText(fullPath, content);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

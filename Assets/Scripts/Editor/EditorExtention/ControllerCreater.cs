using Mono.Cecil.Cil;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ControllerCreater : EditorWindow
{
    [MenuItem("Tools/Auto Controller Creater")]

    static void OpenWindow()
    {
        GetWindow<ControllerCreater>("Controller Creater");
    }

    enum UnitType
    {
        Monster,
        Player,
    }

    List<UnitType> unitTypes = new List<UnitType>();
    List<string> controllerNames = new List<string>();
    List<string> lastNamespaceNames = new List<string>();
    List<string> lastFolderNames  = new List<string>();

    private void OnGUI()
    {
        for (int i = 0; i < controllerNames.Count; i++) 
        {
            var type = (UnitType)EditorGUILayout.EnumPopup
                (
                   new GUIContent("Unit Type","生成したいcontrollerのタイプを選択してください"),
                   unitTypes[i]
                );
            unitTypes[i] = type;
            var controller = EditorGUILayout.TextField
                (
                     new GUIContent("Controller Name","生成したいcontrollerの名前を入れてください"),
                     controllerNames[i]
                 );
            controllerNames[i] = controller;
            var lastNamespaceName = EditorGUILayout.TextField
                (
                     new GUIContent("Last Namespace Name", "生成するcontrollerの末尾のnamespaceを入力してください(Game/Monsters/YourNamespace)"),
                     lastNamespaceNames[i]
                 );
            lastNamespaceNames[i] = lastNamespaceName;
            var lastFolderName = EditorGUILayout.TextField
                (
                    new GUIContent("Folder Name","入れたいFolderの名前を入れてください(Assets/Scripts/Monsters/YourFolder)、ただAssets/Scripts/Monsters のように元々あることが前提です\n" +
                    "ない場合、自動で生成されます"),
                    lastFolderNames[i]
                );
            lastFolderNames[i] = lastFolderName;
            GUILayout.Space(10);
        }

        if(GUILayout.Button("Add Controller"))
        {
            unitTypes.Add(default);
            controllerNames.Add(null);
            lastFolderNames.Add(null);
            lastNamespaceNames.Add(null);   
        }
        GUILayout.Space(10);
        if(GUILayout.Button("Create Controller"))
        {
            for(int i = 0; i < controllerNames.Count;i++)
            {
                var type = unitTypes[i];
                var lastFolderName = lastFolderNames[i];
                if(type == UnitType.Monster)
                {
                    var folderPath = $"Assets/Scripts/Monsters/{lastFolderName}";
                    var className = controllerNames[i];
                    var nameSpace = $"Game.Monsters.{lastFolderNames[i]}";
                    if(!System.IO.Directory.Exists(folderPath))
                    {
                        System.IO.Directory.CreateDirectory(folderPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();    
                    }
                    var fullPath = System.IO.Path.Combine(folderPath, $"{className}.cs");
                    if(System.IO.File.Exists(fullPath))
                    {
                        Debug.LogError("This script already exists in your folder");
                        continue;
                    }
                    CreateController(className: className, namespaceName: nameSpace,fullPath:fullPath);
                }
            }
        }
    }

    void CreateController(string className,string namespaceName,string fullPath)
    {
        var content = $@"using UnityEngine;

namespace {namespaceName}
{{
    public class {className} : MonsterControllerBase<{className}>
    {{

        protected override void Awake()
        {{
            base.Awake();
            //isSummoned = true;//テスト用だから消して
        }}
        //public int ID;//テスト用だから消してね
        protected override void Start()
        {{
            Debug.Log(""ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ"");
            base.Start();
        }}

        public override void Initialize(int owner = -1)
        {{
            /*Please select your monster movetype.
            moveType = MoveType.Walk;
            moveType = MoveType.Fly;*/
            base.Initialize(owner);
            /*I recommend to delete comment out after you create state class at Auto State Creater
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);*/
        }}

    }}

}}";

        System.IO.File.WriteAllText(fullPath, content);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

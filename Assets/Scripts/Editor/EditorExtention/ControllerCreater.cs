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
                   new GUIContent("Unit Type","����������controller�̃^�C�v��I�����Ă�������"),
                   unitTypes[i]
                );
            unitTypes[i] = type;
            var controller = EditorGUILayout.TextField
                (
                     new GUIContent("Controller Name","����������controller�̖��O�����Ă�������"),
                     controllerNames[i]
                 );
            controllerNames[i] = controller;
            var lastNamespaceName = EditorGUILayout.TextField
                (
                     new GUIContent("Last Namespace Name", "��������controller�̖�����namespace����͂��Ă�������(Game/Monsters/YourNamespace)"),
                     lastNamespaceNames[i]
                 );
            lastNamespaceNames[i] = lastNamespaceName;
            var lastFolderName = EditorGUILayout.TextField
                (
                    new GUIContent("Folder Name","���ꂽ��Folder�̖��O�����Ă�������(Assets/Scripts/Monsters/YourFolder)�A����Assets/Scripts/Monsters �̂悤�Ɍ��X���邱�Ƃ��O��ł�\n" +
                    "�Ȃ��ꍇ�A�����Ő�������܂�"),
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
            //isSummoned = true;//�e�X�g�p�����������
        }}
        //public int ID;//�e�X�g�p����������Ă�
        protected override void Start()
        {{
            Debug.Log(""��������������������������������������������"");
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

using Cysharp.Threading.Tasks;
using Game.Monsters;
using Game.Monsters.Slime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class LineUpFields
{
    [SerializeField] public float criterioX;
    [SerializeField] public float criterioZ;
    [SerializeField] public float spaceX;
    [SerializeField] public float spaceZ;
}
public class SelectablePrefabManager : MonoBehaviour
{
    public static SelectablePrefabManager Instance;
    public List<SelectableMonster> monsters { get; set; } = new List<SelectableMonster>();
    public List<SelectableSpell> spells { get; set; } = new List<SelectableSpell>();
    public List<PrefabBase> prefabs { get; set; } = new List<PrefabBase>();
    Material stoneMaterial;
    MonsterAnimatorPar monsterAnimatorPar;

    [SerializeField] LineUpFields lineUpFields; 
    int line = 0;
    int columCount = 0;

    GameObject slimePrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    Dictionary<AttackMotionType, UnityAction<SelectableMonster, CancellationTokenSource>> attackMotions = new Dictionary<AttackMotionType, UnityAction<SelectableMonster, CancellationTokenSource>>();
    private void Awake() => Instance = this;
    public async void Initialize(PrefabManagerActions prefabManagerActions) /*Func<SelectableCard,(MonsterStatusData, SelectableMonster)> getMosnterDataAndPrefab,
                                 Func<SelectableCard, (SpellStatus, SelectableSpell prefab)> getSpellDataAndPrefab*/
    {
        await SetAssetsFromAdress();
        SetSelectablePrafabs(prefabManagerActions);
        slimePrefab = await SetFieldFromAssets.SetField<GameObject>("Prefabs/Monsters/Slime");
    }

    async void SetSelectablePrafabs(PrefabManagerActions prefabManagerActions)/*Func<SelectableCard, (MonsterStatusData, SelectableMonster prefab)> getMosnterDataAndPrefab,
                                    Func<SelectableCard,(SpellStatus,SelectableSpell prefab)> getSpellDataAndPrefab*/
    {
        List<GameObject> monsterObjs = new List<GameObject>();
        List<GameObject> spellObjs = new List<GameObject>();    
        try
        {
            monsterObjs = (await SetFieldFromAssets.SetFieldByLabel<GameObject>("DeckChooseMonster")).ToList();
            spellObjs = (await SetFieldFromAssets.SetFieldByLabel<GameObject>("DeckChooseSpell")).ToList();
        }
        catch (OperatorException)
        {
            Debug.LogWarning("このアドレスは存在しません");
            return;
        }

        var mosntersFromAddress = new List<SelectableMonster>();
        var spellFromAddress = new List<SelectableSpell>();

        foreach (GameObject monster in monsterObjs)
        {
            var monsterObj = Instantiate(monster);
            var selectableMonster = monsterObj.GetComponent<SelectableMonster>();
            
            mosntersFromAddress.Add(selectableMonster);
        }
        monsters = mosntersFromAddress.OrderBy(monster => monster.sortOrder).ToList();

        foreach (GameObject spell in spellObjs)
        {
            var spellObj = Instantiate(spell);  
            var selectableSpell = spellObj.GetComponent<SelectableSpell>();
            spellFromAddress.Add(selectableSpell);
        }

        spells = spellFromAddress.OrderBy(spell => spell.sortOrder).ToList();
        prefabs.AddRange(monsters);
        prefabs.AddRange(spells);
        var cardDatas = SelectableCardManager.Instance.allCardDatas;
        for (var i = 0; i < cardDatas.Count;i++)
        {
            var cardType = cardDatas[i].CardType;
            var prefab = prefabs[i];
            prefab.cardType = cardType;
        }
        PrefabLineUp();
        var deck = SelectableCardManager.Instance.GetDeck();
        deck.ForEach(card =>
        {
            if (card == null) return;
            var prefab = card.cardData.CardType switch
            {
                CardType.Monster => (PrefabBase)prefabManagerActions.getMosnterDataAndPrefab(card).prefab,
                CardType.Spell => (PrefabBase)prefabManagerActions.getSpellDataAndPrefab(card).prefab,
                _=> default
            };

            var cls = card.removedButtonCls;
            prefab.SetSelectedEffect(cls);
            if(prefab is ISelectableSpell selectableSpell)
            {
                UnityAction<LineRenderer> unableAction = (line) =>
                {
                    line.enabled = false;
                };
                selectableSpell.OnSelectedDeckFirst = unableAction;
            }
        });
    }

    async UniTask SetAssetsFromAdress()
    {
        try
        {
            stoneMaterial = await SetFieldFromAssets.SetField<Material>("Materiials/StoneShader");
            monsterAnimatorPar = await SetFieldFromAssets.SetField<MonsterAnimatorPar>("Animations/MonsterAnimatorPar");
        }
        catch (OperatorException)
        {
            Debug.LogWarning("このアドレスは存在しません");
            return;
        }
    }
    void PrefabLineUp()
    {
        var motions = new AttackMotion();
        attackMotions[AttackMotionType.Simple] = motions.SimpleAttackMotion;
        attackMotions[AttackMotionType.DestractionMachine] = motions.DestractionMachineAttack;
        Debug.Log($"Lineは{line},コラムは{columCount}");
        var criterioX = lineUpFields.criterioX;
        var criterioZ = lineUpFields.criterioZ;
        var spaceX = lineUpFields.spaceX;//25f
        var spaceZ = lineUpFields.spaceZ;//10f
        Debug.Log($"{criterioX},{criterioZ},{spaceX},{spaceZ}");
        var index = 0;
        var count = 0;
        for (int l = 0; l < line; l++)
        {
            for (int c = 0; c < columCount; c++)
            {
                Debug.Log($"{index}aaaaa");
                count++;
                var selectablePrefab = prefabs[index];
                var adjustedSpaceZ = (selectablePrefab is ISelectableMonster) ? spaceZ :
                    (selectablePrefab is ISelectableSpell) ? spaceZ + 2.5f : spaceZ;
                var pos = new Vector3(criterioX + spaceX * c,0f,criterioZ - adjustedSpaceZ * l);
                pos.y = Terrain.activeTerrain.SampleHeight(pos);
                selectablePrefab.transform.position = pos;
                if(selectablePrefab is ISelectableMonster selectableMonster)
                {
                    selectableMonster.stoneMaterial = stoneMaterial;
                    selectableMonster.monsterAnimatorPar = monsterAnimatorPar;
                    selectablePrefab.Initialize();
                    var attackMotionType = selectableMonster._statusData.AnimaSpeedInfo.AttackMotionType;
                    if (attackMotionType == AttackMotionType.Simple)
                    {
                        motions.AnimationEventSetup(selectableMonster);
                    }
                    selectableMonster.attackMotionPlay = attackMotions[attackMotionType];
                }
                else if(selectablePrefab is ISelectableSpell selectableSpell)
                {
                    selectablePrefab.Initialize();
                }
               
                if (count == prefabs.Count) break;
                index++;
            }
        }
    }
    public void SetLine(int line,int columCount)
    {
        this.columCount = columCount;
        this.line = line;
    }
    public void UnableLineRenderer(PrefabBase currentSelectedSpell)
    {
        if (!(currentSelectedSpell is ISelectableSpell)) return;
        spells.ForEach(spell =>
        {
            if (spell == currentSelectedSpell) return;
            spell.lineRenderer.enabled = false;
        });
    }
    public void EnableLineRenderer()
    {
        var deck = SelectableCardManager.Instance.GetDeck();
        var sortOrders = deck.Where(card => card != null)
            .Select(card => card.sortOrder).ToList();
        spells.ForEach(spell =>
        {
            var sortOrder = spell.sortOrder;
            var inDeck = sortOrders.Contains(sortOrder);
            if(spell.lineRenderer != null) spell.lineRenderer.enabled = !inDeck;
        });
    }
    public SlimeController[] GetSlimeObjects(Vector3 pos,SpellType spellType)
    {
        var spawnCount = 4;
        var offset = 3.0f;
        var poses = new Vector3[]
        {
           pos + Vector3.right * offset,
           pos + Vector3.left * offset,
           pos + Vector3.forward * offset   ,
           pos + Vector3.back * offset
        };

        var slimes = poses
            .Take(spawnCount)
            .Select(p =>
            {
                var obj = Instantiate(slimePrefab, p, Quaternion.identity);
                var slime = obj.GetComponent<SlimeController>();
                slime.isSummonedInDeckChooseScene = true;
                slime.isSummoned = true;
                slime.ownerID = spellType switch
                {
                    SpellType.Damage or SpellType.DamageToEveryThing or SpellType.OtherToEnemyside => 1,
                    SpellType.Heal or SpellType.OtherToPlayerside or SpellType.OtherToEverything => 0,
                    _ => default
                };
                if (spellType == SpellType.Heal) slime.HalfOfHp();
                return slime;
            }).ToArray();
        return slimes;
    }
}

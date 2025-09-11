using Game.Monsters.Spider;
using System.Drawing;
using UnityEngine;
using static Game.Monsters.Spider.SpiderController;

namespace Game.Monsters.SpiderEgg
{
    public class SpiderEggController : MonsterControllerBase<SpiderEggController>
    {

        public SwayState SwayState { get;private set; }
        public SpawnSpiderState SpawnSpiderState { get; private set; }
        public readonly int sway_Hash = Animator.StringToHash("isSwaying");
        public SpiderController spiderPrefab { get; private set;}
        protected override async void Awake()
        {
            base.Awake();
            animator = GetComponentInChildren<Animator>();
            var spiderObj = UnitScale switch
            {
                //こっちは一つの下の大きさの卵
                UnitScale.large => await SetFieldFromAssets.SetField<GameObject>("Monsters/Spider_Middle"),
                UnitScale.middle => await SetFieldFromAssets.SetField<GameObject>("Monsters/Spider_Small"),
                _ => default,
            };
            if (spiderObj == null) return;
            spiderPrefab = spiderObj.GetComponent<SpiderController>();
        }
        protected override void Start()
        {
            Debug.Log("ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ");
            base.Start();
        }
        protected override void Update()
        {
            Debug.Log(transform.position);
            base.Update();
        }
        public override void Initialize(int owner = -1)
        {
            moveType = MoveType.Walk;
            base.Initialize(owner);
            IdleState = new IdleState(this);
            DeathState = new DeathState(this);
            SwayState = new SwayState(this);
            SpawnSpiderState = new SpawnSpiderState(this);
        }   
    }
}
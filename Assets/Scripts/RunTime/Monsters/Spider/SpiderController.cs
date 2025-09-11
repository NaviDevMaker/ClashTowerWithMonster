using Cysharp.Threading.Tasks;
using Game.Monsters.SpiderEgg;
using System;
using System.Data;
using System.Drawing;
using UnityEngine;

namespace Game.Monsters.Spider
{
    public class SpiderController : MonsterControllerBase<SpiderController>
    {
        [SerializeField] Transform spawnEggTra;
        public Transform SpawnEggTra => spawnEggTra;
        LayAnEggState LayAnEggState;
       
        [Flags]
        public enum SpiderSize
        {
            Small = 1 << 0,
            Middle = 1 << 1,
            Large = 1 << 2,
        }
        [SerializeField] SpiderSize spiderSize;
        public HipSmoke hipSmoke { get; private set; }
        public class LayAnEggInfo
        {
            public readonly float spawnInterval= 5.0f;
            public float elapsedTime { get; set;} = 0f;
            public GameObject eggPrefab { get; private set;}
            public ParticleSystem hipSmoke { get; private set; } = null;
            public LayAnEggInfo(SpiderController spider) => SetFields(spider);
            async void SetFields(SpiderController spiderController)
            {
                 var size = spiderController.spiderSize;
                 eggPrefab = size switch
                 {
                     //こっちは同じ大きさの卵
                     SpiderSize.Large => await SetFieldFromAssets.SetField<GameObject>("Monsters/SpiderEgg_Large"),
                     SpiderSize.Middle => await SetFieldFromAssets.SetField<GameObject>("Monsters/EggParent_Middle"),
                     _ => default,
                 };

                if (size == SpiderSize.Small) return;
                hipSmoke = spiderController.GetComponentInChildren<ParticleSystem>();
            }
        }
        public LayAnEggInfo layAnEggInfo { get; private set;}
        protected override void Awake()
        {
            base.Awake();
            isSummoned = true;//テスト用だから消して
        }
        protected override void Start()
        {
            Debug.Log("ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ");
            base.Start();
        }
        protected override void Update()
        {
            base.Update();
            if (isSummonedInDeckChooseScene) return;
            if (spiderSize == SpiderSize.Small) return;
            CheckLayableAnEgg();
        }
        public override void Initialize(int owner = -1)
        {
            layAnEggInfo = new LayAnEggInfo(this);
            moveType = MoveType.Walk;
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
            LayAnEggState = new LayAnEggState(this);

            if (spiderSize == SpiderSize.Small) return;
            GenerateHipSmokeCollider();
        }

        void GenerateHipSmokeCollider()
        {
            var obj = new GameObject("HipSmokeHitCollider");
            obj.transform.SetParent(transform);
            var localPos = new Vector3(-0.1f, 0f, -0.8f);
            obj.transform.localPosition = localPos;
            var boxCollider = obj.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(1.5f, 1.0f, 1.0f);
            hipSmoke = obj.AddComponent<HipSmoke>();
            hipSmoke.attacker = this;
        }
        void CheckLayableAnEgg()
        {
            if (currentState == LayAnEggState || !isSummoned) return;
            layAnEggInfo.elapsedTime += Time.deltaTime;
            if(layAnEggInfo.elapsedTime >= layAnEggInfo.spawnInterval)
            {
                Debug.Log("卵を産みます");
                layAnEggInfo.elapsedTime = 0f;
                ChangeState(LayAnEggState);
            }
        }
    }
}
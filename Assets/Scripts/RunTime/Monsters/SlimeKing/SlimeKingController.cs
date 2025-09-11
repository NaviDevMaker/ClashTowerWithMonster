using Cysharp.Threading.Tasks;
using Game.Monsters;
using UnityEngine;

namespace Game.Monsters.SlimeKing
{
    public class SlimeKingController : MonsterControllerBase<SlimeKingController>
    {
         public GameObject slimeObj { get; private set; }
         public int spawnSlimeCount { get; private set; } = 4;//死亡時に生成するスライムの数
         protected override void Awake()
         {
             base.Awake();
             //isSummoned = true;//テスト用だから消して
         }
            //public int ID;//テスト用だから消してね
         protected override void Start()
         {
            Debug.Log("ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ");
            base.Start();
         }
      
        public override async void Initialize(int owner = -1)
        {
            moveType = MoveType.Walk;
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
            slimeObj = await SetFieldFromAssets.SetField<GameObject>("Prefabs/Monsters/Slime");
        }
    }
}


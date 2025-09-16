using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Monsters.Slime
{
    public class SlimeController : MonsterControllerBase<SlimeController>
    {
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

        protected override void Update()
        {
            base.Update();
            if(isSummonedInDeckChooseScene)
            {
                HPBarProcess();
                if (hPBar != null)
                {
                    var eulerAngle = transform.rotation.eulerAngles;
                    hPBar.transform.rotation = Quaternion.Euler(eulerAngle.x, 0f, eulerAngle.z);
                }
            }
        }

        public override void Initialize(int owner = -1)
        {
            moveType = MoveType.Walk;
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
        }

        public async void HalfOfHp()
        {
            await UniTask.DelayFrame(100);
            currentHP = Mathf.RoundToInt((float)currentHP / 2);
        }
    }
}


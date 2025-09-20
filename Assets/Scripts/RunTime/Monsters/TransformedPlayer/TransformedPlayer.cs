
using Game.Monsters.Werewolf;
using UnityEngine;

public interface IInvincible
{
    //特定条件での無敵の場合のやつ
    bool IsInvincible { get; set; }
}
public interface INonTarget : IInvincible
{ 
    //変身中は呪文も効かないようにするからこれ必要、呪文の判定側でこのインターフェースを参照して
    //statusConditionのほうで呪文側が判定すると呪文も効かない最強になる為
    float shapeShiftDuration { get;}
    void ReflectEachHP(int currentHP);
}

public interface ITransformedForm<TOrigin> : INonTarget where TOrigin:UnitBase
{
    float nonTargetInterval { get; set; }
    float elapsedTime { get; }
    TOrigin originalEntity { get; set; }
}

namespace Game.Monsters.TransformedPlayer
{
    public class TransformedPlayer : MonsterControllerBase<TransformedPlayer>,ITransformedForm<WerewolfController>
    {
        public WerewolfController originalEntity { get; set; }
        public float nonTargetInterval { get; set; } = 10f;
        public float elapsedTime { get; private set;}
        public bool IsInvincible { get; set; } = false;

        public float shapeShiftDuration => 0.5f;

        protected override void Awake()
        {
            IsInvincible = true;
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
            if (!isSummonedInDeckChooseScene)
            {
                if (!IsInvincible)
                {
                    HPBarProcess();
                    CheckNonTargetInterval();
                    Debug.Log($"{statusCondition.Freeze.isActive},{statusCondition.Freeze.isEffectedCount}");
                    this.CheckFreeze_Unit(animator);
                    this.CheckAbsorption();
                    if (isSummoned)
                    {
                        currentState?.OnUpdate();
                    }
                }
                if (isDead && currentState != DeathState)
                {
                    ChangeToDeathState();
                }
                Debug.Log(currentState);
           }
            //これデッキ選択シーンの時に見本用のモンスターをその都度削除するからそのため
           else
           {
               if (isDead && currentState != DeathState)
               {
                   ChangeToDeathState();
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
            statusCondition.NonTarget.isActive = true;
        }
        void CheckNonTargetInterval()
        {
            elapsedTime += Time.deltaTime;
            if(nonTargetInterval <= elapsedTime && !isDead) isDead = true;
        }
        public void ReflectEachHP(int currentHP) => this.currentHP = currentHP;
    }
}
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;


public interface ISpells { }
namespace Game.Spells
{
    public class SpellBase : MonoBehaviour, IPushable,ISpells, ISummonbable
    {
        protected float spellDuration = 0f;
        public float rangeX { get; protected set; }
        public float rangeZ { get; protected set; }

        public float timerOffsetY { get; private set;}
        public float prioritizedRange { get; protected set; }
        protected float scaleAmount;
        public MoveType moveType { get; protected set; }
        public SpellStatus _SpellStatus { get; protected set; }
        public bool isSummoned { get; set; } = false;

        protected AddForceToUnit<SpellBase> addForceToUnit;
        bool isSpellInvoked = false;
        protected ParticleSystem particle;
        protected SpellEffectHelper spellEffectHelper { get;private set;}
        public bool isKnockBacked_Monster { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool isKnockBacked_Spell { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public PushEffectUnit pushEffectUnit { get;protected set; }
        void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space)) Spell().Forget();//テスト用だから消して
            if (isSummoned && !isSpellInvoked)
            {
                Spell().Forget();
                TimerSetter.Instance.StartSpellTimer(spellDuration, this);
                isSpellInvoked = true;
            }
        }
        protected virtual void SetRange()
        {
            IColliderRangeProvider colliderRangeProvider = null;

            if (TryGetComponent<BoxCollider>(out var boxCollider))
            {
                colliderRangeProvider = new BoxColliderrangeProvider { boxCollider = boxCollider };
                rangeX = colliderRangeProvider.GetRangeX();
                rangeZ = colliderRangeProvider.GetRangeZ();
                prioritizedRange = colliderRangeProvider.GetPriorizedRange();
                timerOffsetY = colliderRangeProvider.GetTimerOffsetY();
            }
            else if (TryGetComponent<SphereCollider>(out var sphereCollider))
            {
                colliderRangeProvider = new SphereColliderRangeProvider { sphereCollider = sphereCollider };
                rangeX = colliderRangeProvider.GetRangeX() * scaleAmount;
                rangeZ = colliderRangeProvider.GetRangeZ() * scaleAmount;
                prioritizedRange = colliderRangeProvider.GetPriorizedRange() * scaleAmount;
                timerOffsetY = colliderRangeProvider.GetTimerOffsetY() * scaleAmount;
            }
            else return;
        }
        protected virtual void SetDuration()
        {
            var particle = transform.GetChild(0).GetComponent<ParticleSystem>();
            this.particle = particle;
            var main = this.particle.main;
            spellDuration = main.duration;
        }
        protected virtual async UniTaskVoid Spell()
        {
            await UniTask.CompletedTask;
        }
        protected virtual void Initialize()
        {
            spellEffectHelper = new SpellEffectHelper(this);
            moveType = MoveType.Spell;
            SetRange();
            SetDuration();
        }
        protected virtual async void DestroyAll()
        {
            await Task.CompletedTask;
        }

    }

    [Flags]
    public enum PushEffectUnit
    {
        OnlyEnemyUnit = 1 << 0,
        OnlyPlayerUnit = 1 << 1,
        AllUnit = 1 << 2,
    }
}


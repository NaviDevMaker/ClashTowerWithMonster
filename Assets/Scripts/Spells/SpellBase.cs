using Cysharp.Threading.Tasks;
using UnityEngine;


public interface ISpells { }
namespace Game.Spells
{
    public class SpellBase : MonoBehaviour, IPushable,ISpells,ISummonbable
    {
        [SerializeField] SpellStatus spellStatus;

        protected float spellDuration = 0f;
        public float rangeX { get; protected set; }
        public float rangeZ { get; protected set; }
        public float prioritizedRange { get; protected set; }
       
        public MoveType moveType { get; protected set; }
        public SpellStatus SpellStatus { get => spellStatus;}
        public bool isSummoned { get; set; } = false;

        protected AddForceToUnit<SpellBase> addForceToUnit;
        bool isSpellInvoked = false;
        protected ParticleSystem particle;
        protected SpellDamageHelper spellDamageHelper { get;private set;}
        public bool isKnockBacked { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space)) Spell().Forget();
            if (isSummoned && !isSpellInvoked)
            {
                Spell().Forget();
                isSpellInvoked = true;
            }
        }
        protected virtual void SetRange()
        {
            var collider = GetComponent<Collider>();
            if (collider == null) return;
            var colliderRadius = collider.bounds.extents;
            rangeX = colliderRadius.x;
            rangeZ = colliderRadius.z;
            Debug.Log("スペルのレンジ取得！！！！！！！！！");
            prioritizedRange = rangeX >= rangeZ ? rangeX : rangeZ;
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
            spellDamageHelper = new SpellDamageHelper(this);
            moveType = MoveType.Spell;
            SetRange();
            SetDuration();
        }

    }
}


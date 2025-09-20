using Cysharp.Threading.Tasks;
using Game.Monsters.EvilMage;
using Game.Spells;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Monsters.WormMonster
{
    public class WormMonsterController : MonsterControllerBase<WormMonsterController>,IInvincible,
                                         ILongDistanceAttacker<WormMonsterController>
                                       
    {
        public bool IsInvincible { get; set; }
        public BurrowChaseState BurrowChaseState { get;private set; }
        public List<LongDistanceAttack<WormMonsterController>> movers { get; set; }
               = new List<LongDistanceAttack<WormMonsterController>>();     
        public Transform startTra { get; private set;}
        public int moverCount => 10;
        public class WormEffect : IEffectSetter
        {
            public ParticleSystem wormEffect { get; private set; }
            readonly Transform wormTra;
            public WormEffect(Transform transform)
            {
                wormTra = transform;
                SetEffect();
            }
            public async void SetEffect()
            {
                var wormEffectObj = await SetFieldFromAssets.SetField<GameObject>("Monsters/WormEffect");
                if (wormEffectObj == null) return;
                wormEffect = wormEffectObj.GetComponent<ParticleSystem>();
            }
            public async void GenerateWormEffect()
            {
               
                var pos = wormTra.position;
                var rot = wormEffect.gameObject.transform.rotation;
                var effect = Instantiate(wormEffect, pos, rot);
                effect.Play();
                await RelatedToParticleProcessHelper.WaitUntilParticleDisappear(effect);
                if (effect == null) return;
                Destroy(effect.gameObject);
            }
        }

        public WormEffect wormEffect { get; private set; }
        protected override void Awake()
        {
            base.Awake();
            //isSummoned = true;//テスト用だから消して
        }
        protected override void Start()
        {
            Debug.Log("ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ");
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
            Debug.Log(currentState);
        }
        public override void Initialize(int owner = -1)
        {
            moveType = MoveType.Walk;
            base.Initialize(owner);
            SetMoverToList();
            wormEffect = new WormEffect(transform);
            IdleState = new IdleState(this);
            BurrowChaseState = new BurrowChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
        }
        public async void EndMoveAction(LongDistanceAttack<WormMonsterController> neurotoxinMover)
        {
            var cmp = neurotoxinMover.GetComponent<NeurotoxinMover>();
            cmp.IsReachedTargetPos = false;
            if(cmp != null)
            {
                await cmp.GroundedProcess();
            }
            if (this == null)
            {
                Destroy(neurotoxinMover.gameObject);
                return;
            }
            neurotoxinMover.transform.SetParent(startTra);
            neurotoxinMover.gameObject.transform.localPosition = Vector3.zero; 
            neurotoxinMover.target = null;
        }
        public void SetMoverToList()
        {
            var name = ProjectileAttackMonsterStatus.MoverStartTra.gameObject.name;
            var parentObj = this.gameObject.GetObject(name);
            if (parentObj == null) return;
            startTra = parentObj.transform;
            var localPos = Vector3.zero;
            var rot = Quaternion.identity;
            for (int i = 0; i < moverCount; i++)
            {
                var mover = Instantiate(ProjectileAttackMonsterStatus.Mover, Vector3.zero, Quaternion.identity);
                var moverCmp = mover.GetComponent<NeurotoxinMover>();
                moverCmp.Setup(this, startTra, localPos, rot, movers, EndMoveAction, ProjectileAttackMonsterStatus.ProjectileMoveSpeed);
            }
        }       
    }
}
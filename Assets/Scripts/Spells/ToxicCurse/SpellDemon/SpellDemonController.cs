using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Monsters.SpellDemon
{
    public class SpellDemonController : MonsterControllerBase<SpellDemonController>
    {
        [SerializeField] List<Material> otherMaterials;
        public UnitBase targetUnit { get; set; }

        public float duration { get; set; }
        public List<Material> OtherMaterials { get => otherMaterials;}

        Vector3 local;
        protected override void Start()
        {
            local = transform.localPosition;
            SetMaterialColors();
            animator = GetComponent<Animator>();
            Initialize();
            ChangeState(IdleState);
        }
        protected override void Update()
        {
            if ((duration <= 0f || targetUnit.isDead) && !isDead) { isDead = true; ChangeState(DeathState); }
            duration -= Time.deltaTime;
            currentState?.OnUpdate();
        }

        private void LateUpdate()
        {
           if(!isDead) transform.localPosition = local;
        }
        public override void Initialize(int owner = -1)
        {       
            animator = GetComponent<Animator>();    
            IdleState = new IdleState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
            AttackState.target = targetUnit;
        }
    }

}
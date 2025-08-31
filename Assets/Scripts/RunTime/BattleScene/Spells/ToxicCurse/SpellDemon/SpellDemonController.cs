using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

namespace Game.Monsters.SpellDemon
{
    public class SpellDemonController : MonsterControllerBase<SpellDemonController>
    {
        [SerializeField] List<Material> otherMaterials;
        public UnitBase targetUnit { get; set; }

        public float duration { get; set; }
        public List<Material> OtherMaterials { get => otherMaterials;}
        public List<SkinnedMeshRenderer> MySkinnedMeshes;
        Vector3 localPos;
        Vector3 worldPos;
        Quaternion worldRot;
        public bool EndSetProcess { get; set; } = false;
        protected override void Start()
        {
            var fieldInfo = typeof(UnitBase).GetField("mySkinnedMeshes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            MySkinnedMeshes = fieldInfo.GetValue(this) as List<SkinnedMeshRenderer>;
            localPos = transform.localPosition;
            SetMaterialColors();
            animator = GetComponent<Animator>();
            Initialize();
            ChangeState(IdleState);
        }
        protected override void Update()
        {
            Debug.Log(transform.localScale);
            if ((duration <= 0f || targetUnit.isDead) && !isDead) { isDead = true; ChangeState(DeathState); }
            duration -= Time.deltaTime;
            currentState?.OnUpdate();
        }

        private void LateUpdate()
        {
            if (!isDead)
            {
                transform.localPosition = localPos;
                worldPos = targetUnit.transform.TransformPoint(localPos);
                worldRot = targetUnit.transform.rotation;
            }
            //すまん俺、ここ第二引数trueにしてもポジションだめやったからworldPos無理やりぶっこむことにした
            else
            {
                transform.position = worldPos;
                transform.rotation = worldRot;
            }
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
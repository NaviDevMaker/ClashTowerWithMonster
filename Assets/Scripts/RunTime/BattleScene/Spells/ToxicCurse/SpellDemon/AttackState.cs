using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
namespace Game.Monsters.SpellDemon
{
    public class AttackState : AttackStateBase<SpellDemonController>
    {

        class RendererInfo
        {
             public List<Material[]> meshMaterials = new List<Material[]>();
             public List<SkinnedMeshRenderer> renderes = new List<SkinnedMeshRenderer>();
             public Material originalMaterial;
        }

        float changeTimeCounter = 0f;
        float changeTime = 1.5f;
        int changeCount = 0;
        bool isTheBiggest = false;
        Vector3 originalScale = Vector3.zero;

        RendererInfo rendererInfo;
        public AttackState(SpellDemonController controller) : base(controller) { }

        public override void OnEnter()
        {
            rendererInfo = new RendererInfo { meshMaterials = controller.meshMaterials, renderes = controller.MySkinnedMeshes
            ,originalMaterial = controller.BodyMesh.material};
            originalScale = controller.transform.localScale;
            Debug.Log(originalScale);
            SetUp();
            //base.OnEnter();
            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<SpellDemonController>(controller, this, clipLength, 25, 0.5f,false);
        }
        public override void OnUpdate()
        {
            if (!isTheBiggest)
            {
               if(target != null && target.statusCondition.DemonCurse.isActive)ChangeStatus();
            }
                if (controller.isDead) return;
            if (!isAttacking)
            {
                isAttacking = true;
                Attack();
            }
        }
        public override void OnExit()
        {
            SetMaterial(rendererInfo.originalMaterial);
            controller.transform.localScale = originalScale;
            controller.animator.speed = 1.0f;
            cts?.Cancel();
            cts?.Dispose();
        }
        void ChangeStatus()
        {
            changeTimeCounter += Time.deltaTime;
            if(changeTimeCounter >= changeTime)
            {
                var scaleAmount = 1.25f;
                var duration = 1.0f;
                changeTimeCounter = 0f;
                var originalAttackAmount = controller.MonsterStatus.AttackAmount;
                attackAmount += originalAttackAmount;
                controller.transform.DOScale(controller.transform.localScale * scaleAmount, duration);
                changeCount++;
                ChangeMaterial();
                if (changeCount == 2) isTheBiggest = true;
            }
        }

        void ChangeMaterial()
        {
            var index = changeCount - 1;
         
            var newMaterial = controller.OtherMaterials[index];

            SetMaterial(newMaterial);
        }

        void SetMaterial(Material newMaterial)
        {

            for (int i = 0; i < rendererInfo.renderes.Count; i++)
            {
                var length = rendererInfo.renderes[i].materials.Length;
                var newMaterials = new Material[length];
                for (int j = 0; j < newMaterials.Length; j++)
                {
                    newMaterials[j] = newMaterial;
                }
                rendererInfo.renderes[i].materials = newMaterials;
            }
        }
    }

}
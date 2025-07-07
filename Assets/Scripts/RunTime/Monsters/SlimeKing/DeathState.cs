using UnityEngine;
using Game.Monsters.Slime;
       
namespace Game.Monsters.SlimeKing
{
    public class DeathState : DeathStateBase<SlimeKingController>
    {
        public DeathState(SlimeKingController controller) : base(controller) { }

        public override void OnEnter()
        {
            SpawnSlime();
            base.OnEnter();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        void SpawnSlime()
        {
            var spawnOffset = 3.0f;
            var rayOffsetY = 0.3f;
            var unitPos = controller.transform.position;
            var uniRot = controller.transform.rotation;
            for (int i = 0; i < controller.spawnSlimeCount; i++)
            {              
                var spawnPos = i switch
                {
                    0 => controller.transform.forward * spawnOffset,
                    1 => -controller.transform.forward * spawnOffset,
                    2 => controller.transform.right * spawnOffset,
                    3 => -controller.transform.right * spawnOffset,
                    _ => Vector3.zero
                };

                spawnPos = spawnPos + unitPos;
                spawnPos.y = Terrain.activeTerrain.SampleHeight(spawnPos);
               
                var rayDistance = (unitPos - spawnPos).sqrMagnitude;
                var direction = (unitPos - spawnPos).normalized;
                var origin = unitPos + Vector3.up * rayOffsetY;
                var hits = Physics.RaycastAll(origin, direction, rayDistance, Layers.buildingLayer);
                if(hits.Length > 0)
                {
                    for (int h = 0; h < hits.Length; h++)
                    {
                        var hitCollider = hits[h].collider;
                        var hitLayer = 1 << hitCollider.gameObject.layer;
                        if(hitLayer == Layers.buildingLayer)
                        {
                            var slime = controller.slimeObj.GetComponent<SlimeController>();
                            var attackRange = slime.MonsterStatus.AttackRange;
                            var adjust = -(direction * attackRange);
                            spawnPos = hitCollider.ClosestPoint(unitPos) + adjust;
                        }
                    }
                }

                var spawnedSlime = UnityEngine.Object.Instantiate(controller.slimeObj, spawnPos, uniRot).GetComponent<SlimeController>();
                controller.SetSummonParticle(spawnPos);
                spawnedSlime.isSummoned = true;
                spawnedSlime.ownerID = controller.ownerID;
                spawnedSlime.Side = controller.Side;
            }
        }
        
    }

}
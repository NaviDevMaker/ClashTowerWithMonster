using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using static UnityEngine.ParticleSystem;


namespace Game.Spells.Meteo
{
    public class MeteoMover : MonoBehaviour, IPushable, ISpells, ISide
    {
        public Meteo attacker { get; set; }
        List<GameObject> chunks = new List<GameObject>();
        List<ParticleSystem> particles = new List<ParticleSystem>();
        List<MainModule> mainModules = new List<MainModule>();
        UnitBase targetUnit = null;
        public bool IsEndSpellProcess { get; set; } = false;

        public float rangeX { get; private set; }

        public float rangeZ { get; private set; }

        public float prioritizedRange { get; private set; }

        public bool isKnockBacked_Unit { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool isKnockBacked_Spell { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public MoveType moveType => throw new System.NotImplementedException();

        public UnitScale UnitScale => throw new System.NotImplementedException();

        public Transform spellTra => throw new System.NotImplementedException();

        public float timerOffsetY => throw new System.NotImplementedException();

        public int ownerID { get; set; }

        ParticleSystem parentParticle = null;

        bool isSettedProparty = false;

        AddForceToUnit<MeteoMover> addForceToUnit;
        private void Update()
        {
            var rotateSpeed = 2.0f;
            transform.Rotate(Vector3.up * 360f * rotateSpeed * Time.deltaTime, Space.Self);
        }
        public void StartMove(UnitBase targetUnit)
        {
            if (!isSettedProparty) Initialize();
            if (particles.Count == 0)
            {
                parentParticle = transform.GetChild(0).GetComponent<ParticleSystem>();
                mainModules.Add(parentParticle.main);
                if (parentParticle != null)
                {
                    foreach (Transform child in parentParticle.transform)
                    {
                        var p = child.GetComponent<ParticleSystem>();
                        if (p != null) { particles.Add(p); mainModules.Add(p.main); }
                    }
                }
            }
            else
            {
                mainModules.ForEach(main => main.loop = true);
            }
            this.targetUnit = targetUnit;
            MoveToTarget().Forget();
        }

        public async UniTask MoveToTarget()
        {
            parentParticle.Play();
            var moveSpeed = 15f;
            var height = TargetPositionGetter.GetTargetHeight(targetUnit);
            var offset = targetUnit.transform.forward * 0.5f;
            var targetPos = targetUnit.transform.position + Vector3.up * height + offset;
            while ((targetPos - transform.position).magnitude > Mathf.Epsilon + 0.1f
                && (!targetUnit.isDead && targetUnit != null))
            {
                targetPos = targetUnit.transform.position + Vector3.up * height + offset;
                var move = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed);
                transform.position = move;
                await UniTask.Yield();
            }

            if (targetUnit.isDead && targetUnit == null)
            {
                targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos);
                while ((targetPos - transform.position).magnitude > Mathf.Epsilon + 0.1f)
                {
                    var move = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed);
                    transform.position = move;
                    await UniTask.Yield();
                }
            }
            transform.position = targetPos;
            if (targetUnit != null && !targetUnit.isDead)
            {
                addForceToUnit.CompareEachUnit(targetUnit);
                if (targetUnit.TryGetComponent<IUnitDamagable>(out var unitDamagable))
                {
                    var attackAmount = attacker._SpellStatus.EffectAmont;
                    unitDamagable.Damage(attackAmount);
                }
            }
            EffectManager.Instance.expsionEffect.GenerateExplosionEffect(targetPos);
            gameObject.SetActive(false);
            //attacker = null;
            await ExplositionMeteo();
        }

        async UniTask ExplositionMeteo()
        {

            var step = 24 * 3;
            var meteoMesh = GetComponent<MeshFilter>().mesh;
            var meteoMaterial = GetComponent<MeshRenderer>().material;
            var verticles = meteoMesh.vertices;
            var triangles = meteoMesh.triangles;
            var scale = transform.localScale;
            var pos = transform.position;
            chunks = this.GetDivisionMesh<MeteoMover>(
               step,
               triangles,
               verticles,
               scale,
               pos,
               meteoMaterial
           );
            var name = "HQBigRock_mtl";
            var min = -2.0f;
            var max = 2.0f;
            await this.Scattering<MeteoMover>(chunks, name, min, max);
            IsEndSpellProcess = true;
            chunks.Clear();
        }

        public List<UniTask> PaticleDisapperTasksGeter()
        {
            mainModules.ForEach(main => main.loop = false);
            var tasks = new List<UniTask>();
            var parentTask = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(parentParticle);
            tasks.Add(parentTask);
            foreach (var particle in particles)
            {
                var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(particle);
                tasks.Add(task);
            }

            return tasks;
        }

        void Initialize()
        {
            ownerID = attacker.ownerID;
            IColliderRangeProvider colliderRangeProvider;
            if (TryGetComponent<SphereCollider>(out var sphereCollider))
            {
                var scaleAmount = 2f;
                colliderRangeProvider = new SphereColliderRangeProvider { sphereCollider = sphereCollider };
                rangeX = colliderRangeProvider.GetRangeX() * scaleAmount;
                rangeZ = colliderRangeProvider.GetRangeZ() * scaleAmount;
                prioritizedRange = colliderRangeProvider.GetPriorizedRange() * scaleAmount;

                Debug.Log($"{rangeX},{rangeZ}");
                addForceToUnit = new AddForceToUnit<MeteoMover>(this, attacker._SpellStatus.PushAmount,
                    attacker._SpellStatus.PerPushDurationAndStunTime, attacker.pushEffectUnit);
                isSettedProparty = true;
            }


        }
    }
}



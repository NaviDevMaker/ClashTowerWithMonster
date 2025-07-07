using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;


public interface ISpells 
{
    Transform spellTra { get; }
    float timerOffsetY {  get; }

}

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
        public bool isKnockBacked_Unit { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool isKnockBacked_Spell { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public PushEffectUnit pushEffectUnit { get;protected set; }

        public UnitScale UnitScale => throw new NotImplementedException();

        public Transform spellTra => transform;

        LineRenderer lineRenderer;
        void Start()
        {
            Initialize();
        }

        private void Update()
        {
            //if(Input.GetKeyDown(KeyCode.Space)) Spell().Forget();//テスト用だから消して
            if (isSummoned && !isSpellInvoked)
            {
                DrawSpellRange().Forget();
                LitLineRendererMaterial();
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
        protected virtual  void Initialize()
        {
            SetUpLineRenderer();
            spellEffectHelper = new SpellEffectHelper(this);
            moveType = MoveType.Spell;
            SetRange();
            SetDuration();
        }
        protected virtual async void DestroyAll()
        {
            await Task.CompletedTask;
        }

        async void SetUpLineRenderer()
        {
            lineRenderer = PoolObjectPreserver.LineRendererGetter();
            if(lineRenderer == null)
            {
                var prefab = await SetFieldFromAssets.SetField<GameObject>("Prefabs/LineRenderer");
                var obj = Instantiate(prefab,transform.position,Quaternion.identity);
                var line =  obj.GetComponent<LineRenderer>();
                PoolObjectPreserver.lineRenderers.Add(line);
                lineRenderer = line;
            }
            lineRenderer.SetUpLineRenderer();
            lineRenderer.sortingOrder = 1;
        }
        async UniTask DrawSpellRange()
        {
            if (lineRenderer != null)
            {
                lineRenderer.gameObject.SetActive(true);
                if(!lineRenderer.enabled) lineRenderer.enabled = true;
            }

            lineRenderer.DrawRange(transform.position, rangeX, rangeZ);
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(spellDuration), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException) { return; }
            await lineRenderer.ShurinkRangeLine(transform.position,rangeX,rangeZ);
            lineRenderer.gameObject.SetActive(false);
        }

        void LitLineRendererMaterial()
        {
            Func<float, float, Material, UniTask> waitAction = async (radAdjust, maxIntencity, material) =>
            {
                var r = (float)191 / 255;
                var g = (float)139 / 255;
                var b = 0f;
                var baseColor = new Color(r,g,b) * 8.0f;
                var time = 0f;

                while(time + 0.1f < spellDuration)
                {
                    time += Time.deltaTime;
                    var amount = (Mathf.Cos(Time.time * radAdjust) * 0.5f + 0.5f) * maxIntencity;
                    material.SetColor("_EmissionColor", baseColor * amount);
                    await UniTask.Yield(cancellationToken:lineRenderer.gameObject.GetCancellationTokenOnDestroy());
                }
            };

            lineRenderer.LitLineRendererMaterial(waitAction);
        }
    }
}

[Flags]
public enum PushEffectUnit
{
    OnlyEnemyUnit = 1 << 0,
    OnlyPlayerUnit = 1 << 1,
    AllUnit = 1 << 2,
}


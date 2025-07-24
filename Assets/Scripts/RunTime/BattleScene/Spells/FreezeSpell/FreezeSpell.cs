using Cysharp.Threading.Tasks;
using Game.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.VFX;

namespace Game.Spells.Freeze
{
    public class FreezeSpell : SpellBase
    {
        class FreezeInfo
        {
            public Material freezeMaterial { get; set; }
            public Renderer freezeRenderer { get; set; }
            public float interval { get; set; }

            public float usualDuration { get; set; }
            public float freezerateMax { get; set; }
            public float alphaDuration { get; set; }
            public float groundFreezeStartvalue { get; set; }
        }
        FreezeInfo freezeInfo;
        VisualEffect snowVFX;

        readonly StatusConditionType conditionType = StatusConditionType.Freeze;
        protected override async void Initialize()
        {
            Material freezeMaterial = default;
            try
            {
                freezeMaterial = await SetFieldFromAssets.SetField<Material>("Materials/FreezeShaderMaterial");
            }
            catch (Exception)
            {
                Debug.LogWarning("指定のアドレスにアセットが存在していません");
            }
            freezeInfo = new FreezeInfo();

            snowVFX = transform.GetChild(0).GetComponent<VisualEffect>();
            particle = transform.GetChild(1).GetComponent<ParticleSystem>();
            _SpellStatus = await SetFieldFromAssets.SetField<SpellStatus>("Datas/Spells/Freeze");
            base.Initialize();

            var max = 0f;
            if (freezeMaterial != null)
            {
                if (freezeMaterial.HasProperty("_FreezeRateMax")) max = freezeMaterial.GetFloat("_FreezeRateMax");
                freezeMaterial.renderQueue = 3000;
            }

            Debug.Log(max);
            var renderer = GetComponent<MeshRenderer>();
            var copiedMaterial = new Material(freezeMaterial);
            renderer.material = copiedMaterial;

            var duration = spellDuration / 2;
            var usualDuration = spellDuration / 4;
            freezeInfo = new FreezeInfo
            {
                freezeRenderer = renderer,
                freezeMaterial = freezeMaterial,
                interval = spellDuration / 2,
                usualDuration = usualDuration,
                freezerateMax = max,
                alphaDuration = duration,
                groundFreezeStartvalue = 0f
            };

            var startValue = freezeInfo.groundFreezeStartvalue;
            if (renderer.material.HasProperty("_FreezeRate")) renderer.material.SetFloat("_FreezeRate", startValue);
            if (renderer.material.HasProperty("_Alpha")) renderer.material.SetFloat("_Alpha", 0f);
        }
        protected override void SetRange()
        {
            scaleAmount = 10.0f;
            base.SetRange();
        }

        protected override void SetDuration() => spellDuration = 4.0f;

        protected override async UniTaskVoid Spell()
        {
            ParticlePlay(particle);
            snowVFX.Play();
            var endvalue = 1.0f;
            MaterialFadeSetter(freezeInfo.freezeRenderer.material, endvalue).Forget();
            var units = spellEffectHelper.GetUnitInRange();
            var filteredList = units.Where(unit =>
            {
                var inRange = spellEffectHelper.CompareUnitInRange(unit);
                var isNotTower = !(unit is TowerControlller);
                return inRange && isNotTower;
            }).ToList();

            if (filteredList.Count == 0) return;

            StartGroundFreezeAndMelt(freezeInfo.freezeRenderer.material);
            filteredList.ForEach(unit =>
            {
                CheckCancellPreviousToken(unit);
                var newCls = new CancellationTokenSource();
                unit.statusCondition.Freeze.isEffectedCount++;
                unit.statusCondition.Freeze.isActive = true;
                unit.statusCondition.visualTokens[conditionType] = newCls;
                FreezeAction(unit, newCls);
            });

            await UniTask.Delay(TimeSpan.FromSeconds(spellDuration));
            filteredList.ForEach(unit =>
            {
                if (unit == null || unit.isDead) return;
                unit.statusCondition.Freeze.isEffectedCount--;
                var count = unit.statusCondition.Freeze.isEffectedCount;
                if (count == 0) unit.statusCondition.Freeze.isActive = false;
            });
            endvalue = 0f;
            snowVFX.Stop();
            var main = particle.main;
            main.loop = false;
            await MaterialFadeSetter(freezeInfo.freezeRenderer.material, endvalue);

            DestroyAll();
            //
        }

        void CheckCancellPreviousToken(UnitBase target)
        {
            if (target.statusCondition.visualTokens.TryGetValue(conditionType, out var previousToken))
            {
                previousToken.Cancel();
                previousToken.Dispose();
                target.statusCondition.visualTokens.Remove(conditionType);
            }
        }
        void ParticlePlay(ParticleSystem particle)
        {
            var main = particle.main;
            main.loop = true;
            particle.Play();
        }
        protected override async void DestroyAll()
        {
            Func<bool> vfxWait = (() => snowVFX.HasAnySystemAwake());
            var particleTask = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(particle);
            await UniTask.WhenAll(
                  UniTask.WaitUntil(vfxWait),
                  particleTask
            );
            Destroy(gameObject);
        }
        void FreezeAction(UnitBase target, CancellationTokenSource cls)
        {
            spellEffectHelper.EffectToEachUnit(target);
            List<Renderer> renderers = target.AllMesh;
            if (renderers.Count == 0)
            {
                Debug.LogWarning("meshを取得できませんでした");
                return;
            }
            for (int i = 0; i < renderers.Count; i++)
            {
                //Debug.Log(freezeMaterial.name);
                var newMaterials = renderers[i].materials;
                for (int j = 0; j < renderers[i].materials.Length; j++)
                {
                    var copiedMaterial = new Material(freezeInfo.freezeMaterial);
                    copiedMaterial.renderQueue = 3001;
                    var mainTexture = renderers[i].materials[j].GetTexture("_BaseMap");
                    copiedMaterial.SetTexture("_MainTex", mainTexture);
                    newMaterials[j] = copiedMaterial;
                    Debug.Log(renderers[i].materials[j].name);
                }

                renderers[i].materials = newMaterials;
                for (int m = 0; m < renderers[i].materials.Length; m++) StartFreezeAndMelt(target, renderers[i].materials[m], renderers, cls);
            }
        }

        async void StartFreezeAndMelt(UnitBase target, Material freezeMaterial, List<Renderer> targetRenderers, CancellationTokenSource cls)
        {
            if (freezeMaterial.HasProperty("_FreezeRateMax"))
            {
                var max = freezeInfo.freezerateMax;
                freezeMaterial.SetFloat("_FreezeRateMax", max);
            }

            if (freezeMaterial.HasProperty("_Alpha")) freezeMaterial.SetFloat("_Alpha", 1.0f);

            var startValue = freezeMaterial.GetFloat("_FreezeRate");
            var endValue = 1.0f;
            var duration = freezeInfo.usualDuration;
            if (freezeMaterial.HasProperty("_FreezeRate")) freezeMaterial.SetFloat("_FreezeRate", startValue);
            var freeze = FreezerateSetter(freezeMaterial, startValue, endValue, duration, cls);
            var interval = freezeInfo.interval;

            try
            {
                await freeze();
                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: cls.Token);
                var melt = FreezerateSetter(freezeMaterial, endValue, 0f, duration, cls);
                await melt();
            }
            catch (OperationCanceledException) { return; }
            SetOriginalMaterial(target, targetRenderers,cls);
        }
        async void StartGroundFreezeAndMelt(Material groundMaterial)
        {
            if (groundMaterial.HasProperty("_FreezeRateMax"))
            {
                var max = freezeInfo.freezerateMax;
                groundMaterial.SetFloat("_FreezeRateMax", max);
            }
            var currentValue = groundMaterial.GetFloat("_FreezeRate");
            var usualStartValue = freezeInfo.groundFreezeStartvalue;
            var startValue = currentValue != usualStartValue ? currentValue : usualStartValue;
            var endValue = 0.2f;
            var freezeAndMeltDuration = freezeInfo.alphaDuration;
            var freeze = FreezerateSetter(groundMaterial, startValue, endValue, freezeAndMeltDuration);
            var interval = freezeInfo.interval;
            await freeze();
            await UniTask.Delay(TimeSpan.FromSeconds(interval));
            var meltInterval = freezeInfo.alphaDuration;
            var melt = FreezerateSetter(groundMaterial, endValue, usualStartValue, freezeAndMeltDuration);
            await melt();
        }
        void SetOriginalMaterial(UnitBase target, List<Renderer> renderers,CancellationTokenSource expectedCls)
        {
            if (!target.statusCondition.visualTokens.TryGetValue(conditionType, out var currentToken)) return;
            if (currentToken != expectedCls) return;//仕様変わってスコープ外でも使えるらしい
            for (int i = 0; i < renderers.Count; i++)
            {
                var originalMaterials = target.meshMaterials[i];
                renderers[i].materials = originalMaterials;
            }
        }
        Func<UniTask> FreezerateSetter(Material freezeMaterial, float start, float end, float duration, CancellationTokenSource cls = null)
        {
            if (!freezeMaterial.HasProperty("_FreezeRate"))
            {
                Debug.LogWarning("指定のプロパティが見つかりませんでした");
                return null;
            }
            var freezeTime = 0f;
            var max = freezeInfo.freezerateMax;
            Func<UniTask> action = (async () =>
            {
                try
                {
                    while (freezeTime < duration)
                    {
                        freezeTime += Time.deltaTime;
                        var lerp = freezeTime / duration;
                        var value = Mathf.Clamp01(Mathf.Lerp(start, end, lerp));
                        freezeMaterial.SetFloat("_FreezeRate", value);

                        if (cls != null) await UniTask.Yield(cancellationToken: cls.Token);
                        else await UniTask.Yield();
                    }
                }
                catch (OperationCanceledException) { return; }
                freezeMaterial.SetFloat("_FreezeRate", end);
            });
            return action;
        }
        async UniTask MaterialFadeSetter(Material freezeMaterial, float endValue)
        {
            if (!freezeMaterial.HasProperty("_Alpha"))
            {
                Debug.LogWarning("指定のプロパティが見つかりませんでした");
                return;
            }
            var duration = freezeInfo.alphaDuration;
            var time = 0f;
            var currentValue = freezeMaterial.GetFloat("_Alpha");

            while (time < duration)
            {
                time += Time.deltaTime;
                var lerp = time / duration;
                var value = Mathf.Clamp01(Mathf.Lerp(currentValue, endValue, lerp));
                freezeMaterial.SetFloat("_Alpha", value);
                await UniTask.Yield();
            }
            freezeMaterial.SetFloat("_Alpha", endValue);
        }
    }
}




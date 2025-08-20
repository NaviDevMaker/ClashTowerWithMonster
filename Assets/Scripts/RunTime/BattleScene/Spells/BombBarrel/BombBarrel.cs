using Cysharp.Threading.Tasks;
using Game.Spells;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Spells.BombBarrel
{
    public class BombBarrel : SpellBase
    {
        List<GameObject> chunks = new List<GameObject>();
        Transform barrelMainbody = null;
        protected override async void Initialize()
        {
            _SpellStatus = await SetFieldFromAssets.SetField<SpellStatus>("Datas/Spells/BombBarrel");
            base.Initialize();
            pushEffectUnit = PushEffectUnit.AllUnit;
            particle = transform.GetChild(0).GetComponent<ParticleSystem>();
            addForceToUnit = new AddForceToUnit<SpellBase>(this, _SpellStatus.PushAmount, _SpellStatus.PerPushDurationAndStunTime, pushEffectUnit);
        }
        protected override async UniTaskVoid Spell()
        {
            //UIManager.Instance.StartSpellTimer(spellDuration, this);//Ç±ÇÍÇ†Ç∆Ç≈è¡ÇµÇƒÇÀ
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(spellDuration), cancellationToken: this.GetCancellationTokenOnDestroy());
                addForceToUnit.KeepDistance(moveType);
                spellEffectHelper.EffectToUnit();
                await ExplosionBarrel();
            }
            catch (OperationCanceledException) { return; }
            DestroyAll();
        }
        async UniTask ExplosionBarrel()
        {
            if (particle == null) return;
            particle.Play();
            barrelMainbody = transform.GetChild(1);
            var scale = barrelMainbody.transform.localScale;
            foreach (Transform child in barrelMainbody)
            {
                var barrelMesh = child.GetComponent<MeshFilter>().mesh;
                var barrelMaterial = child.GetComponent<MeshRenderer>().material;
                var verticles = barrelMesh.vertices;
                var triangles = barrelMesh.triangles;
                var uvs = barrelMesh.uv;
                var trianglePos = child.transform.position;
                int step = 6 * 3;
                chunks.AddRange(this.GetDivisionMesh<BombBarrel>(

                    step,
                    triangles,
                    verticles,
                    uvs,
                    scale,
                    trianglePos,
                    barrelMaterial
                ));
                child.gameObject.SetActive(false);
            }

            chunks.ForEach(chunk => chunk.transform.SetParent(transform));//Ç±ÇÍdeckëIëâÊñ ÇÊÇ§Ç…í«â¡ÇµÇΩÇ©ÇÁÉoÉOÇ¡ÇΩÇÁÇ±ÇÍÇÃÇπÇ¢ê‡îZå˙
            await BarrelScattering();
        }

        async UniTask BarrelScattering()
        {
            var name = "Barrel";
            var min = -5.0f;
            var max = 5.0f;
            await this.Scattering<BombBarrel>(chunks, name, min, max);
        }
        protected override void SetDuration()
        {
            spellDuration = _SpellStatus.SpellDuration;
        }
        protected override void DestroyAll()
        {
            if (this == null) return;
            Destroy(gameObject);
        }
    }
}

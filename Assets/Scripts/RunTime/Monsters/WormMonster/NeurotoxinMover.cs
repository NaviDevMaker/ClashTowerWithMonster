using Cysharp.Threading.Tasks;

using DG.Tweening;
using System;
using System.Collections;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Video;

namespace Game.Monsters.WormMonster
{
    public class NeurotoxinMover : LongDistanceAttack<WormMonsterController>,IRangeAttack
    {
        public GameObject rangeAttackObj { get => throw new System.NotImplementedException();
                                           set => throw new System.NotImplementedException();}
        Material slimeMaterial;
        void Start() => slimeMaterial = GetComponent<MeshRenderer>().material;
        protected override void Update()
        {
            if (IsReachedTargetPos) OnEndProcess?.Invoke(this);
        }
        public async UniTask GroundedProcess()
        {
            try
            {
                var originalScale = transform.lossyScale;
                var targetScale = new Vector3(originalScale.x,1f,originalScale.z);
                var scaleSet = new Vector3TweenSetup(targetScale,0.5f);
                gameObject.Scaler(scaleSet);
                transform.localRotation = Quaternion.identity;
                var existDuration = 5.0f;
                var damageInterval = 1.0f;
                var startTime = Time.time;
                var damage = attacker.StatusData.AttackAmount;
                while(Time.time - startTime < existDuration)
                {
                    var currentTarget = this.GetUnitInSpecificRangeItem(attacker).Invoke();
                    currentTarget.ForEach(target =>
                    {
                        Debug.Log(target.name);
                        //if (target == null) return;
                        target.Damage(damage);
                        EffectManager.Instance.hitEffect.GenerateHitEffect(target);
                    });

                    await UniTask.Delay(TimeSpan.FromSeconds(damageInterval),
                                        cancellationToken: this.GetCancellationTokenOnDestroy());
                }
                var fadeDuration = 2.0f;
                var targetAlpha = 0f;
                var fadeTask = DOTween.To(
                    () => slimeMaterial.GetFloat("_Alpha"),
                    x => slimeMaterial.SetFloat("_Alpha", x),
                    targetAlpha,
                    fadeDuration
                    ).ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
                await fadeTask;
            }
            catch (OperationCanceledException) { }
            finally
            {
                if(this != null)
                {
                    transform.localScale = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                    slimeMaterial.SetFloat("_Alpha", 1.0f);
                    gameObject.SetActive(false);
                }
            }
        }

        protected override IEnumerator MoveToEnemy()
        {
            var col = target.GetComponent<Collider>();
            if (col == null) yield break;
            var closestPos = col.ClosestPoint(transform.position);
            closestPos.y = Terrain.activeTerrain.SampleHeight(closestPos);
            while ((closestPos - transform.position).magnitude > Mathf.Epsilon + 0.1f
                && (!target.isDead && target != null))
            {
                closestPos = col.ClosestPoint(transform.position);
                closestPos.y = Terrain.activeTerrain.SampleHeight(closestPos);
                var move = Vector3.MoveTowards(transform.position,closestPos, Time.deltaTime * moveSpeed);
                var direction = closestPos - transform.position;
                if (direction != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(direction);
                    transform.rotation = rotation;
                }
                transform.position = move;
                yield return null;
            }

            if (target.isDead && target == null)
            {
                closestPos.y = Terrain.activeTerrain.SampleHeight(closestPos);
                while ((closestPos - transform.position).magnitude > Mathf.Epsilon + 0.1f)
                {
                    var move = Vector3.MoveTowards(transform.position, closestPos, Time.deltaTime * moveSpeed);
                    var direction = closestPos - transform.position;
                    if (direction != Vector3.zero)
                    {
                        Quaternion rotation = Quaternion.LookRotation(direction);
                        transform.rotation = rotation;
                    }
                    transform.position = move;
                    yield return null;
                }
            }

            transform.position = closestPos;
            moveCoroutine = null;
            IsReachedTargetPos = true;
        }
        public override async void Move()
        {
            gameObject.SetActive(true);
            var duration = 0.25f;
            var targetScale = Vector3.one * 3f;
            var scaleSet = new Vector3TweenSetup(targetScale,duration);
            try
            {
                await gameObject.Scaler(scaleSet).ToUniTask(cancellationToken:this.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException) { return; }
            transform.SetParent(null);
            base.Move();
        }
        public void SetHitJudgementObject() =>throw new System.NotImplementedException();
    }
}


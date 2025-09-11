using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using System;
using static UnityEngine.GraphicsBuffer;

public class GunMover:LongDistanceAttack<TowerController> 
{
   
    [SerializeField] ParticleSystem trailEffect;
    ParticleSystem hit;
    ParticleSystem trail;
    Quaternion adjust = Quaternion.Euler(0f, 35f, 0f);//パーティクルの調整回転
    readonly Quaternion modelForwardOffset = Quaternion.Euler(10, 58f, -55f);//モデルの前方向補正
    protected override void Update()
    {
        base.Update();
        if (IsReachedTargetPos) DamageToEnemy(target, HitEffect);
    }
    public override void Move()
    {
        gameObject.SetActive(true);
       if (moveCoroutine!=null)
       {
           if (trail.isPlaying) trail.Stop();
           StopCoroutine(moveCoroutine);
       }
        moveCoroutine = StartCoroutine(MoveToEnemy());
    }
    protected override IEnumerator MoveToEnemy()
    {
        Debug.Log("向かいます");
        var height = TargetPositionGetter.GetTargetHeight(target);
        //Debug.Log(GetTargetHeight());
        trail.Play();

       
            var targetPos = target.transform.position + new Vector3(0f, height, 0f);
            while ((targetPos - transform.position).magnitude > Mathf.Epsilon + 0.1f
                && (!target.isDead && target != null))
            {
                targetPos = target.transform.position + new Vector3(0f, height, 0f);
                var move = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed);
                var direction = targetPos - transform.position;
                if (direction != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(direction);
                    transform.rotation = rotation * modelForwardOffset;
                }
                transform.position = move;
                yield return null;
            }    

        if(target.isDead && target == null)
        {
            targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos);
            while ((targetPos - transform.position).magnitude > Mathf.Epsilon + 0.1f)
            {
                var move = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed);
                var direction = targetPos - transform.position;
                if (direction != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(direction);
                    transform.rotation = rotation * modelForwardOffset;// * adjust;
                }
                transform.position = move;
                yield return null;
            }

        }

        transform.position = targetPos;
        trail.Stop();
        moveCoroutine = null;
        IsReachedTargetPos = true;
    }
    protected override void Initialize(TowerController controlller)
    {
        base.Initialize(controlller);
        hit = Instantiate(hitEffect);
        trail = Instantiate(trailEffect);
        trail.gameObject.transform.SetParent(transform);
        trail.transform.localPosition = TrailPosition();
        var euler = trail.transform.rotation.eulerAngles;
        //モデルの逆方向にパーティクルの出口を向けたいからそうなるように調整
        var eulerY = 180.0f -(adjust.eulerAngles.y);
        var rot = Quaternion.Euler(euler.x,eulerY,euler.z);
        trail.transform.rotation = rot;
        transform.rotation = adjust;
        transform.gameObject.SetActive(false);
    }

    Vector3 TrailPosition()
    {
        trail.transform.localPosition = Vector3.zero;
        float backOffset = 3.3f;
        float upOffset = 4.0f;
        float rightOffset = 4.3f;
        var position = (-transform.forward * backOffset) + Vector3.up * upOffset + Vector3.right * rightOffset;// + f;
        return position;
    }

    //float GetTargetHeight()
    //{
    //    Debug.Log(target);
    //    var body = 0;
    //    Renderer meshRenderer = null;
    //    if(target.MySkinnedMeshes.Count != 0) meshRenderer = target.MySkinnedMeshes[body];
    //    else if(target.MyMeshes.Count != 0) meshRenderer= target.MyMeshes[body];
    //    if (meshRenderer != null) return meshRenderer.bounds.size.y/ 2;
    //    else return 1f;
    //}
    async UniTask HitEffect()
    {
        try
        {
            var duration = hit.main.duration;
            hit.gameObject.transform.SetParent(target.transform);
            hit.gameObject.transform.position = target.BodyMesh.bounds.center;
            hit.gameObject.SetActive(true);
            hit.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            hit.gameObject.transform.SetParent(transform);
            hit.gameObject.SetActive(false);
        }
        catch(MissingReferenceException) { }
        catch (NullReferenceException) { }
    }
}

using DG.Tweening;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Game.Players;
using System.Linq;
using Game.Players.Sword;

public class PointerDisplay:MonoBehaviour
{
    [SerializeField] ParticleSystem pointedPositionParticle;
    [SerializeField] GameObject arrow;
    PlayerControllerBase<SwordPlayerController> player;
    float floatTime = 0.5f;
    float destroyTime = 2.0f;
    float upAmount = 1.5f;
    float offsetY = 1.0f;//生成ポジションのoffsetのy
    CancellationTokenSource cls = new CancellationTokenSource();
    bool isAttackingPlayer = false;
    bool isDeadPlayer = false;
    private void Start()
    {
        arrow.SetActive(false);
        SetPlayerEvent();
       
    }

    void SetPlayerEvent()
    {
        var units = FindObjectsByType<UnitBase>(sortMode: FindObjectsSortMode.None);
        foreach (var unit in units)
        {
            if (!(unit is IPlayer)) continue;
            if (unit.ownerID != 0) continue;
            if(unit is SwordPlayerController swordPlayerController) 
            {
                player = swordPlayerController;
                swordPlayerController.OnAttackingPlayer += (isAttacking => isAttackingPlayer = isAttacking);
                swordPlayerController.OnDeathPlayer += (isDead => isDeadPlayer = isDead);
            }
        } 
    }
    //プレイヤーが動き始めてから出したいからLateUpdate
    private void LateUpdate()
    {
        if(!isDeadPlayer)
        {
            if (InputManager.IsClikedNextMovePreparation())
            {
                cls?.Cancel();
                cls?.Dispose();
                cls = null;
                cls = new CancellationTokenSource();
            }
            if (!isAttackingPlayer && InputManager.IsCllikedMoveButton()) DisplayTargetPoint().Forget();
            else if(isAttackingPlayer && InputManager.IsClickedMoveWhenAttacking()) DisplayTargetPoint().Forget();
        }     
    }
    async UniTask DisplayTargetPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(ray);
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                var hitLayer = 1 << hit.collider.gameObject.layer;
                if (Layers.groundLayer == hitLayer)
                {
                    ParticleSystem particle = null;
                    try
                    {
                        arrow.SetActive(true);
                        Debug.Log("ヒット");
                        var targetPos = hit.point;
                        var direction = (targetPos - player.transform.position).normalized;
                        transform.position = GetGeneratePos(direction,targetPos);
                        var genePos = transform.position; //+ new Vector3(0f, offsetY, 0f);
                        particle = Instantiate(pointedPositionParticle, genePos, Quaternion.identity);
                        var move = transform.DOMoveY(transform.position.y + upAmount, floatTime).SetLoops(2, LoopType.Yoyo).ToUniTask(cancellationToken: cls.Token);
                        var wait = UniTask.Delay(TimeSpan.FromSeconds(destroyTime), cancellationToken: cls.Token);
                        await UniTask.WhenAll(move, wait);
                    }
                    finally
                    {
                        if (particle != null) Destroy(particle.gameObject);
                        arrow.SetActive(false);
                    }
                    break;
                }        
            }
        }
    }

    Vector3 GetGeneratePos(Vector3 direction,Vector3 targetPos)
    {
        RaycastHit[] hits;
        var offset_Pos = new Vector3(0f, offsetY, 0f);
        var rayOffset = new Vector3(0f, 0.3f, 0f);
        var playerPos = player.transform.position;
        var origin = playerPos + rayOffset;
        var distance = Vector3.Distance(targetPos, playerPos);
        hits = Physics.RaycastAll(origin,direction, distance, Layers.buildingLayer);
        if(hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                var layer = 1 << hit.collider.gameObject.layer;
                if (layer == Layers.buildingLayer)
                {
                    Debug.Log("建物にヒット！！");
                    //Playerが建物を前にしたときにこれ以上進めなくする距離の変数
                    var stopRange = player.PlayerStatus.PlayerAttackType == PlayerAttackType.OnlyGroundedEnemy ? player.PlayerStatus.AttackRange : 1f;
                    var point = hit.point;
                    var fowardOffset = Vector3.zero;
                    var pointDistance = (point - playerPos).sqrMagnitude;

                    if (pointDistance > stopRange) fowardOffset = -(direction * stopRange);
                    else fowardOffset = -(direction * pointDistance);

                    point = point + fowardOffset;
                    point.y = Terrain.activeTerrain.SampleHeight(hit.point);               
                    var genePos = point + offset_Pos;
                    return genePos;
                }
            }
        }
        return targetPos + offset_Pos;
    }

}

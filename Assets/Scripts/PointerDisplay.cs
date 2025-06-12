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
    float offsetY = 1.0f;
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
    //�v���C���[�������n�߂Ă���o����������LateUpdate
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
                        Debug.Log("�q�b�g");
                        var targetPos = hit.point;
                        var direction = (targetPos - transform.position).normalized;
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
        var radius = 2f;
        var offset_Pos = new Vector3(0f, offsetY, 0f);
        var playerPos = player.transform.position;
        var origin = playerPos + offset_Pos;
        var distance = Vector3.Distance(targetPos,playerPos);
        hits = Physics.SphereCastAll(origin, radius, direction, distance, Layers.buildingLayer);
        if(hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                var layer = 1 << hit.collider.gameObject.layer;
                if(layer == Layers.buildingLayer)
                {
                    var genePos = hit.collider.ClosestPoint(playerPos) + offset_Pos;
                    return genePos;
                }
            }
        }
        return targetPos + offset_Pos;
    }

}

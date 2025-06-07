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
        if (Physics.Raycast(ray, out RaycastHit hit))
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
                    transform.position = targetPos;
                    var genePos = targetPos + new Vector3(0f, offsetY, 0f);
                    particle = Instantiate(pointedPositionParticle, genePos, Quaternion.identity);
                    var move = transform.DOMoveY(transform.position.y + upAmount, floatTime).SetLoops(2, LoopType.Yoyo).ToUniTask(cancellationToken: cls.Token);
                    var wait = UniTask.Delay(TimeSpan.FromSeconds(destroyTime),cancellationToken:cls.Token);
                    await UniTask.WhenAll(move, wait);
                }
                finally
                {
                   if(particle != null) Destroy(particle.gameObject);
                   arrow.SetActive(false);
                }            
            }
        }
    }
}

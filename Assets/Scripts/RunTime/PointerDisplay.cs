using DG.Tweening;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Game.Players;
using System.Linq;
using Game.Players.Sword;

public class PointerDisplay: MonoBehaviour
{
    class MaterialInfo
    {
        public Material arrowMaterial { get; set; }
        public Color originalEmmisionColor { get; set; }
        public float maxIntencity {  get; set; }
    }

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
    bool isMoving = false;
    MaterialInfo materialInfo;
    private void Start()
    {
        ArrowSet();
        SetPlayerEvent();      
    }

    void ArrowSet()
    {
        arrow.SetActive(false);
        var arrowMaterial = arrow.GetComponent<MeshRenderer>().material;
        Color originalEmmisionColor = default;
        if(arrowMaterial.HasProperty("_EmissionColor"))
        {
            originalEmmisionColor = arrowMaterial.GetColor("_EmissionColor");
        }
        materialInfo = new MaterialInfo
        {
            arrowMaterial = arrowMaterial,
            originalEmmisionColor = originalEmmisionColor,
            maxIntencity = 1.5f
        };
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

    async void ArrowMaterialColorSet()
    {
       var arrowMaterial = materialInfo.arrowMaterial;
       if (arrowMaterial == null) return;

       var intecityDuration = 0.5f;
       var time = 0f;
       var intecity = materialInfo.maxIntencity;
       var originalColor = materialInfo.originalEmmisionColor;
       while(time < intecityDuration && !isMoving)
       {
          time += Time.deltaTime;
          var lerp = time / intecityDuration;
          var currentIntecity = Mathf.Lerp(intecity, 0f, lerp);
          arrowMaterial.SetColor("_EmissionColor", originalColor * currentIntecity);
          await UniTask.Yield();
       }
       if (isMoving) return;
       arrowMaterial.SetColor("_EmissionColor", originalColor * 0f);
       arrowMaterial.DisableKeyword("_EMISSION");
       var newColor = arrowMaterial.color;
       var duration = 1.0f;
       while (time < duration && !isMoving)
       {
           time += Time.deltaTime;
           var lerp = time / duration;
           var alpha = Mathf.Lerp(1.0f, 0f, lerp);
           newColor.a = alpha;
           arrowMaterial.color = newColor;
           await UniTask.Yield();
       } 
       if(!isMoving)
       {
           newColor.a = 0f;
           arrowMaterial.color = newColor;
           arrow.SetActive(false);
       }     
    }

    void ArrowSetToOriginal()
    {
        var arrowMaterial = materialInfo.arrowMaterial;
        var originalColor = materialInfo.originalEmmisionColor;
        var intencity = materialInfo.maxIntencity;
        arrowMaterial.SetColor("_EmissionColor", originalColor * intencity);
        arrowMaterial.EnableKeyword("_EMISSION");
        arrow.SetActive(true);
        var newColor = arrowMaterial.color;
        newColor.a = 1.0f;  
        arrowMaterial.color = newColor; 
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
                        isMoving = true;
                        ArrowSetToOriginal();
                        Debug.Log("ヒット");
                        var targetPos = hit.point;
                        Debug.Log(player);
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
                    }
                    isMoving = false;
                    ArrowMaterialColorSet();
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

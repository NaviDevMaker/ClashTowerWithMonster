using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class LongDistanceAttack<T> : MonoBehaviour where T : UnitBase
{

    public bool IsReachedTargetPos { get; set; } = false;
    public float moveSpeed { get; set; }
    public UnitBase target { get; set; }
    [SerializeField] protected ParticleSystem hitEffect;
    public T attacker { get; private set; }
    protected Coroutine moveCoroutine;
    public UnityAction<LongDistanceAttack<T>> OnEndProcess;
    public int attackAmount { get;private set;} = 0;
    protected virtual void Initialize(T attacker,int attackAmount) 
    {
        this.attacker = attacker;
        this.attackAmount = attackAmount;
    }
    protected virtual void Update() { }
    protected virtual void DamageToEnemy(UnitBase target,Func<UniTask> hitEffect = null)
    {
        if (target.isDead || target == null) OnEndProcess?.Invoke(this);
        else 
        {
           if(target.TryGetComponent(out IUnitDamagable unitDamagable))
           {
                if (hitEffect != null) hitEffect().Forget();
                unitDamagable.Damage(attackAmount);
                OnEndProcess?.Invoke(this);
            }
        }
    }
    protected virtual IEnumerator MoveToEnemy()
    {
        Debug.Log("Œü‚©‚¢‚Ü‚·");
        var height = TargetPositionGetter.GetTargetHeight(target);

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
                transform.rotation = rotation;
            }
            transform.position = move;
            yield return null;
        }

        if (target.isDead && target == null)
        {
            targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos);
            while ((targetPos - transform.position).magnitude > Mathf.Epsilon + 0.1f)
            {
                var move = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed);
                var direction = targetPos - transform.position;
                if (direction != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(direction);
                    transform.rotation = rotation;
                }
                transform.position = move;
                yield return null;
            }
        }

        transform.position = targetPos;
        moveCoroutine = null;
        IsReachedTargetPos = true;
    }

    public void Setup(T attacker,Transform parent,Vector3 pos,Quaternion rot,List<LongDistanceAttack<T>> addedList
        ,UnityAction<LongDistanceAttack<T>> EndAction,float moveSpeed,int atAmount)
    {
        Initialize(attacker,atAmount);
        transform.SetParent(parent);
        transform.localPosition = pos;
        transform.localRotation = rot;
        gameObject.SetActive(false);
        OnEndProcess = EndAction;
        this.moveSpeed = moveSpeed;
        addedList.Add(this);
    }

    public virtual void Move()
    {
        gameObject.SetActive(true);
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveToEnemy());
    }
}

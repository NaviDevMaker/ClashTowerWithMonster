using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.InputSystem.XR;
using UnityEditor.Searcher;
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
    protected virtual void Initialize(T attacker) 
    {
        this.attacker = attacker;
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
                unitDamagable.Damage(attacker.StatusData.AttackAmount);
                OnEndProcess?.Invoke(this);
            }
        }
    }
    protected virtual IEnumerator MoveToEnemy()
    {
        yield return null;
    }
    public void Setup(T attacker,Transform parent,Vector3 pos,Quaternion rot,List<LongDistanceAttack<T>> addedList
        ,UnityAction<LongDistanceAttack<T>> EndAction,float moveSpeed)
    {
        Initialize(attacker);
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

using Cysharp.Threading.Tasks;
using DG.Tweening;
using NUnit.Framework.Constraints;
using System;
using System.Threading;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DeckChooseCameraMover : MonoBehaviour
{
    public PrefabBase currentSelectedPrefab { get; set;}

    Vector3 originalPos;
    public CancellationTokenSource selectedCardCls = null;

    float duration = 0.5f;
    public bool isSettedOriginalPos { get; private set; } = false;
    private void Start()
    {
        originalPos = transform.position;
    }
    public async UniTask MoveToFrontOfObj()
    {
        var endvalue_Mover = GetTargetPos();
        var endvalue_Roter = new Vector3(0f, 180f, 0f);
       
        var set_Mover = new Vector3TweenSetup(endvalue_Mover, duration);
        var set_Roter = new Vector3TweenSetup(endvalue_Roter, duration);
        var task = gameObject.Mover(set_Mover).ToUniTask(cancellationToken: selectedCardCls.Token);
        var task2 = gameObject.Roter(set_Roter).ToUniTask(cancellationToken: selectedCardCls.Token);

        try
        {
            await UniTask.WhenAll(task,task2);

            var center = Vector3.zero;
            if(currentSelectedPrefab is ISelectableMonster monster)
            {
                var bounds = monster._bodyMesh.bounds;
                center = bounds.center;
            }
            Debug.Log(center);
            var direction = center - transform.position;
            direction.x = 0f;
            var rot = Quaternion.LookRotation(direction);
            //var eulerAngles = Quaternion.LookRotation(direction).eulerAngles;
            //var xOnly = new Vector3(eulerAngles.x, 180f, 0f);
            //var lookTargetSet = new Vector3TweenSetup(xOnly, duration);
            var task3 = gameObject.transform.DORotateQuaternion(rot,duration).ToUniTask(cancellationToken: selectedCardCls.Token);
            await task3;
        }
        catch (OperationCanceledException)
        {
            transform.position = originalPos;
        }
    }
    public async void SetOriginalPos()
    {
        if (isSettedOriginalPos || selectedCardCls == null) return;
        if (currentSelectedPrefab is ISelectableMonster monster) monster.Repetrification();
        isSettedOriginalPos = true;
        var endRotValue = new Vector3(90f, 0f, 0f);
       
        var set_Mover = new Vector3TweenSetup(originalPos, duration);
        var set_Roter = new Vector3TweenSetup(endRotValue, duration);
        var task = gameObject.Mover(set_Mover).ToUniTask(cancellationToken: selectedCardCls.Token);
        var task2 = gameObject.Roter(set_Roter).ToUniTask(cancellationToken: selectedCardCls.Token);

        try
        {
            await UniTask.WhenAll(task, task2);
        }
        catch (OperationCanceledException) { isSettedOriginalPos = false; }
        isSettedOriginalPos = false;
    }
    Vector3 GetTargetPos()
    {
        var size = currentSelectedPrefab.colliderSize;
        var z = size.z;
        var adjust = 2.0f;    
        var offsetY = 0.5f;
        var targetPos = currentSelectedPrefab.transform.position;
        if (currentSelectedPrefab is ISelectableMonster monster)
        {
            targetPos.y = !monster._isFlying ? Terrain.activeTerrain.SampleHeight(targetPos) + offsetY : targetPos.y + offsetY;
            adjust = !monster._isFlying ? 2.0f : 3.0f;
        }
        var offset = currentSelectedPrefab.gameObject.transform.forward * z * adjust;
        targetPos += offset;
        return targetPos;
    }
}

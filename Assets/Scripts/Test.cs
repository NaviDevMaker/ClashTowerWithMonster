using UnityEngine;
using DG.Tweening;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;
public class Test : MonoBehaviour
{
    CancellationTokenSource cts = new CancellationTokenSource();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TestMeso();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) cts.Cancel();
    }

    async void TestMeso()
    {
        try
        {
            var task = transform.DOMove(transform.position + Vector3.forward * 5f, 5.0f).SetEase(Ease.Linear).ToUniTask(tweenCancelBehaviour: TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: cts.Token);
            await task;
        }
        catch(OperationCanceledException)
        {

        }
        finally
        {
            Debug.Log("ssss");
        }       
    }
}

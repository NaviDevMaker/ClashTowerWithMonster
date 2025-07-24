using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ScrollManager : MonoBehaviour
{
    public static ScrollManager Instance { get; private set; } 
    Image scrollImage;
    ScrollRect scrollRect;

    bool isSliding = false;
    bool isStoping = false;
    CancellationTokenSource cls = new CancellationTokenSource();
    public bool isPointerDowned = false;
    private void Awake() => Instance = this;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();    
    }
    // Update is called once per frame
    void Update()
    {
        Debug.Log(Input.mouseScrollDelta.y);
        if(InputManager.IsClickedSlideButton())
        {
            if (!isSliding) StartSliding();
            else
            {
                cls.Cancel();
                cls.Dispose();
                cls = new CancellationTokenSource();
            }
        }
        else
        {
            if (isSliding) StopSliding();
            else scrollRect.vertical = false;
        }
    }

    void StartSliding()
    {
        cls.Cancel();
        cls.Dispose();
        cls = new CancellationTokenSource();
        isSliding = true;
        scrollRect.vertical = true;
    }

    async void StopSliding()
    {
        if (isStoping) return;
        Debug.Log("スライディングを止めます");
        try
        {
            isStoping = true;
            var interval = 1.0f;
            var time = 0f;
            while(time < interval && !cls.IsCancellationRequested)
            {
                time += Time.deltaTime;
                var y = Mathf.Abs(Input.mouseScrollDelta.y);
                if (y >= 1.0f || isPointerDowned)
                {
                    cls.Cancel();
                    break;
                }
                await UniTask.Yield(cancellationToken: cls.Token);
            }           
           
        }
        catch (OperationCanceledException) { }
        finally
        {
            scrollRect.vertical = false;
            isSliding = false;
            isStoping = false;
        }
    }
}

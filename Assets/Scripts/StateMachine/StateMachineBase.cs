using UnityEditor;
using UnityEngine;

//各ステイトマシンの親クラス、使いまわせるようにジェネリック型
public abstract class StateMachineBase<T> where T : MonoBehaviour
{
    protected T controller;
    protected float clipLength = 0f;

    protected StateMachineBase<T> nextState;
    public StateMachineBase(T controller)
    {
        this.controller = controller;
    }

    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();
}

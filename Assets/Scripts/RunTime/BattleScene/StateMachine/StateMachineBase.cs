using UnityEditor;
using UnityEngine;

//�e�X�e�C�g�}�V���̐e�N���X�A�g���܂킹��悤�ɃW�F�l���b�N�^
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

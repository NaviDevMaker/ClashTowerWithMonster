using Cysharp.Threading.Tasks;
using Game.Monsters;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public interface IFlying
{
    float FlyingOffsetY { get;}
}
[CreateAssetMenu]
public class FlyingMonsterStatusData : MonsterStatusData,IFlying
{
    [SerializeField] float flyingOffsetY;
    public float FlyingOffsetY => flyingOffsetY;

    //FieldInfo isAbsorbedField;
    //object _attackInstance;

    //����V�����C���X�^���X���������l�񂾁i�΁j����g��Ȃ������A�Ƃ������ł��邾�낤���Ǎ��̉��ł͖������A
    //���g���@�������GPT�ɕ����Ȃ��ł��ꌩ�Ď��͂ł�ꉴ
    //public override void AttackStateInitialize<Tonwer>(Tonwer controller)
    //{
    //    Debug.Log($"�A�^�b�N�X�e�C�g�̃C���X�^���X��{_attackInstance}");
    //    if (isInitilizedAttackState) return;
    //    var attackStateBase = typeof(AttackStateBase<>);
    //    var stateType = attackStateBase.MakeGenericType(controller.GetType());
    //    _attackInstance = controller._attackState;
    //    isAbsorbedField = stateType.GetField("_isAbsorbed", BindingFlags.NonPublic | BindingFlags.Instance);
    //    isInitilizedAttackState = true;
    //}
    public override void AttackStateUpdateMethod<Tonwer>(Tonwer controller)
    {      
        //if (_attackInstance == null || isAbsorbedField == null) return;
        var isAbsorbed = controller.statusCondition.Absorption.isActive;
        var previousAbsorbed = controller.AttackState._isAbsorbed;/*(bool)isAbsorbedField.GetValue(_attackInstance);*/
        if (previousAbsorbed && !isAbsorbed)
        {
            MoveToCorrectPos(controller);
        }
        controller.AttackState._isAbsorbed = isAbsorbed;
        //isAbsorbedField.SetValue(_attackInstance,isAbsorbed);
    }

    async void MoveToCorrectPos<Tonwer>(Tonwer controller) where Tonwer : MonsterControllerBase<Tonwer>
    {
        Debug.Log("���̈ʒu�ɖ߂�܂�");
        try
        {
             var targetPos = PositionGetter.GetFlatPos(controller.transform.position) + Vector3.up * FlyingOffsetY;
             var moveSpeed = 10f;
             while (!controller.statusCondition.Absorption.isActive && !controller.isDead
                    && Vector3.Distance(controller.transform.position, targetPos) >= Mathf.Epsilon)
             {
                targetPos = PositionGetter.GetFlatPos(controller.transform.position)
                            + Vector3.up * FlyingOffsetY;
                var move = Vector3.MoveTowards(controller.transform.position, targetPos, Time.deltaTime * moveSpeed);
                controller.transform.position = move;
                await UniTask.Yield(cancellationToken: controller.GetCancellationTokenOnDestroy());
             }
        }
        catch (OperationCanceledException) { }
    }
}

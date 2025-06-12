using UnityEngine;

public static class InputManager
{
    /// <summary>
    /// ���[�u�{�^��(���N���b�N)�������ꂽ���ǂ�����Ԃ��܂�
    /// </summary>
   public static bool IsCllikedMoveButton()
   {
        return Input.GetMouseButtonUp(0);
   }

    /// <summary>
    /// �܂��͍U���L�����Z�������Ĉړ��������Ƃ��̃{�^��(���N���b�N����A�{�^����������Ă��Ȃ�)�������ꂽ���ǂ�����Ԃ��܂�
    /// </summary>
    public static bool IsClickedMoveWhenAttacking()
   {
        return Input.GetMouseButtonUp(0) && !Input.GetKey(KeyCode.A);
   }
    /// <summary>
    /// ���[�u�{�^��(���N���b�N)���G�𔭌����掩���ōU���ɐ؂�ւ��{�^��(A�{�^��)�������ꂽ�ǂ����Ԃ��܂�
    /// </summary>
   public static bool IsClickedMoveAndAutoAttack()
   {
        return Input.GetMouseButtonUp(0) && Input.GetKey(KeyCode.A);
   }


    /// <summary>
    /// �����{�^��(�E�N���b�N)�������ꂽ���ǂ����Ԃ��܂�
    /// </summary>
   public static bool IsClickedSummonButton()
   {
        return Input.GetMouseButtonUp(1);
   }
    /// <summary>
    /// ��D�͈͓̔��ł����Ă������ł���{�^��(E�{�^��)�������ꂽ���ǂ����Ԃ��܂�
    /// </summary>
   public static bool IsClickedSummonButtonOnHandField()
   {
        return Input.GetKeyDown(KeyCode.E);
   }
    /// <summary>
    /// ��D�̕\���A��\����ς���{�^��(Q�{�^��)�������ꂽ���ǂ����Ԃ��܂�
    /// </summary>
   public static bool IsClickedSwitchHandDisplay()
   {
        return Input.GetKeyDown(KeyCode.Q);
   }

    /// <summary>
    /// �I�񂾃J�[�h����D�ɖ߂��{�^��(W�{�^��)�������ꂽ���ǂ����Ԃ��܂�
    /// </summary>
   public static bool IsClickedResetSelectedCard()
   {
        return Input.GetKeyDown(KeyCode.W);
   }

    public static bool IsClikedNextMovePreparation()
    {
        return Input.GetMouseButtonDown(0);
    }
}

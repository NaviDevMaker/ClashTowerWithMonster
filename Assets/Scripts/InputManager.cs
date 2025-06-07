using UnityEngine;

public static class InputManager
{
    /// <summary>
    /// ムーブボタン(左クリック)が押されたかどうかを返します
    /// </summary>
   public static bool IsCllikedMoveButton()
   {
        return Input.GetMouseButtonUp(0);
   }

    /// <summary>
    /// または攻撃キャンセルをして移動したいときのボタン(左クリックかつAボタンが押されていない)が押されたかどうかを返します
    /// </summary>
    public static bool IsClickedMoveWhenAttacking()
   {
        return Input.GetMouseButtonUp(0) && !Input.GetKey(KeyCode.A);
   }
    /// <summary>
    /// ムーブボタン(左クリック)かつ敵を発見次第自動で攻撃に切り替わるボタン(Aボタン)が押されたどうか返します
    /// </summary>
   public static bool IsClickedMoveAndAutoAttack()
   {
        return Input.GetMouseButtonUp(0) && Input.GetKey(KeyCode.A);
   }


    /// <summary>
    /// 召喚ボタン(右クリック)が押されたかどうか返します
    /// </summary>
   public static bool IsClickedSummonButton()
   {
        return Input.GetMouseButtonUp(1);
   }
    /// <summary>
    /// 手札の範囲内であっても召喚できるボタン(Eボタン)が押されたかどうか返します
    /// </summary>
   public static bool IsClickedSummonButtonOnHandField()
   {
        return Input.GetKeyDown(KeyCode.E);
   }
    /// <summary>
    /// 手札の表示、非表示を変えるボタン(Qボタン)が押されたかどうか返します
    /// </summary>
   public static bool IsClickedSwitchHandDisplay()
   {
        return Input.GetKeyDown(KeyCode.Q);
   }

    /// <summary>
    /// 選んだカードを手札に戻すボタン(Wボタン)が押されたかどうか返します
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

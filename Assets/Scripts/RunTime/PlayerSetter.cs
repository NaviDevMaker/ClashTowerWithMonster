using Game.Players;
using Game.Players.Sword;
using UnityEngine;

/// <summary>
/// GameManager���瑗���Ă���Player�̏�񂩂�Player�ɃC�x���g�Ƃ�������
/// </summary>
public class PlayerSetter : MonoBehaviour
{
    [SerializeField] EnergyGageController energyGageController;
    public void Setup<T>(PlayerControllerBase<T> player) where T : PlayerControllerBase<T>
    {
        player.OnInvokeSkill = energyGageController.ReduceSkillEnergy;
        player.MeetSkillEnergy = ((skillEnergy) => energyGageController.currentEnergy >= skillEnergy);
    }
}

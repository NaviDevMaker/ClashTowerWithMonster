using Game.Players;
using Game.Players.Sword;
using UnityEngine;

/// <summary>
/// GameManager‚©‚ç‘—‚ç‚ê‚Ä‚«‚½Player‚Ìî•ñ‚©‚çPlayer‚ÉƒCƒxƒ“ƒg‚Æ‚©‚ğ‚Â‚¯‚é
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

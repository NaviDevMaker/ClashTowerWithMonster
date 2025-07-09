using Game.Players.Sword;
using UnityEngine;

public class GameManager : SingletonMonobehavier<GameManager>
{
    [SerializeField] PlayerSetter playerSetter;
    [SerializeField] SwordPlayerController player;
    private void Start()
    {
        playerSetter.Setup<SwordPlayerController>(player);
    }
}

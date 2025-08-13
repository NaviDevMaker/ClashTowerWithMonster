using Game.Players.Sword;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonMonobehavier<GameManager>
{
    [SerializeField] PlayerSetter playerSetter;
    [SerializeField] SwordPlayerController player;
    private void Start()
    {
        playerSetter.Setup<SwordPlayerController>(player);
    }

    void SetupField(Scene scene)
    {
        switch (scene.name)
        {
            case "DeckChooseScene":
                break;
            case "BattleScene":
                break;
        }

    }
}

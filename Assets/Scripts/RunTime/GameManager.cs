using Game.Players.Sword;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonMonobehavier<GameManager>
{
    [SerializeField] PlayerSetter playerSetter;// これ将来はprivateね
   　[SerializeField] SwordPlayerController player;//これ将来はprivateね
    DeckPreserver deckPreserver;
    private async void Start()
    {
        deckPreserver = await SetFieldFromAssets.SetField<DeckPreserver>("Datas/DeckPreserver");
        playerSetter.Setup<SwordPlayerController>(player);
        SceneManager.activeSceneChanged += SetupField;
        SceneManager.activeSceneChanged += PoolObjectPreserver.ListClear;
    }

    void SetupField(Scene previousScene,Scene newScene)
    {
        switch (newScene.name)
        {
            case "BattleScene":
                //Photonでは相手のPlayerSetterは表示されないからこれでいい
                playerSetter = GameObject.FindFirstObjectByType<PlayerSetter>();

                var players = GameObject.FindObjectsByType<SwordPlayerController>(sortMode:FindObjectsSortMode.None);
                foreach (var player in players)
                {
                    //ここ最終的にはPhotonのisMineかどうかで判断する、もしくはIDで判断するけど今は０が味方だからこれで行く
                    var id = player.ownerID;
                    if (id == 0) this.player = player;
                    break;
                }
                break;
            default:
                break;
        }
    }
}

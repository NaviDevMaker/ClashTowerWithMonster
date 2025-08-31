using Game.Players.Sword;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonMonobehavier<GameManager>
{
    [SerializeField] PlayerSetter playerSetter;// ���ꏫ����private��
   �@[SerializeField] SwordPlayerController player;//���ꏫ����private��
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
                //Photon�ł͑����PlayerSetter�͕\������Ȃ����炱��ł���
                playerSetter = GameObject.FindFirstObjectByType<PlayerSetter>();

                var players = GameObject.FindObjectsByType<SwordPlayerController>(sortMode:FindObjectsSortMode.None);
                foreach (var player in players)
                {
                    //�����ŏI�I�ɂ�Photon��isMine���ǂ����Ŕ��f����A��������ID�Ŕ��f���邯�Ǎ��͂O�����������炱��ōs��
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

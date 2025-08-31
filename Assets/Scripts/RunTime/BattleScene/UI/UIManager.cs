using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UIManager : SingletonMonobehavier<UIManager>
{
    //public static UIManager Instance { get; private set;}

    TimerSetter timerSetter;
    [SerializeField] SummonedMonsterDisplayUI summoneMonsterDisplayUI;//将来はいらないけどバトルシーンでテストするときに必要
    //private void Awake() => Instance = this;
    private void Start()
    {
        timerSetter = TimerSetter.Instance;
        SceneManager.activeSceneChanged += (oldScene,newScene) =>
        {
            var battleScene = NetWorkSceneManager.Instance.sceneNames.BattleScene;
            if(newScene.name == battleScene) summoneMonsterDisplayUI = GameObject.FindFirstObjectByType<SummonedMonsterDisplayUI>();
        };
    }
    public async UniTask StartSummonTimer(float summonTime, UnitBase targetUnit) => await timerSetter.StartSummonTimer(summonTime, targetUnit);
    public void StartSpellTimer(float spellTime, ISpells spell,UnityAction<GameObject> setTimerObj = null) => timerSetter.StartSpellTimer(spellTime, spell,setTimerObj);
    public void StartSkillTimer(float skillTime, ISkills skill) => StartSkillTimer(skillTime, skill);

    public void SummonedNameDisplay<T>(T unitObj) where T : MonoBehaviour,ISide => summoneMonsterDisplayUI.SummonedUIDisplay(unitObj);
}


using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set;}

    [SerializeField] TimerSetter timerSetter;
    [SerializeField] SummonedMonsterDisplayUI summoneMonsterDisplayUI; 
    private void Awake() => Instance = this;

    public async UniTask StartSummonTimer(float summonTime, UnitBase targetUnit) => await timerSetter.StartSummonTimer(summonTime, targetUnit);
    public void StartSpellTimer(float spellTime, ISpells spell) => timerSetter.StartSpellTimer(spellTime, spell);
    public void StartSkillTimer(float skillTime, ISkills skill) => StartSkillTimer(skillTime, skill);

    public void SummonedNameDisplay<T>(T unitObj) where T : MonoBehaviour,ISide => summoneMonsterDisplayUI.SummonedUIDisplay(unitObj);
}


using UnityEngine;

[CreateAssetMenu]
public class SceneNames : ScriptableObject
{
    [SerializeField] string battleScene;
    [SerializeField] string deckSelectScene;

    public string BattleScene  => battleScene;
    public string DeckSelectScene => deckSelectScene;
}

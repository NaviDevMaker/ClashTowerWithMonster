using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu]
public class CardData : ScriptableObject
{
    [SerializeField] Sprite icon;
    [SerializeField] Sprite energyImage;
    [SerializeField] int energy;
    [SerializeField] CardName cardName;
    [SerializeField] protected GameObject cardPrefab;
    [SerializeField] CardType cardType;

    public Sprite Icon { get => icon;}
    public int Energy { get => energy;}
    public CardName CardName { get => cardName;}
    public GameObject CardPrefab { get => cardPrefab;}
    public Sprite EnergyImage { get => energyImage; }
    public CardType CardType { get => cardType;}
}
public enum CardName
{ 
    Skelton,
    Gardian,
    ShotGiant,
    Gagoyle,
    tg,
    ad,
    adad,
    caca,
}

public enum CardType
{
   Monster,
   Spell,
}


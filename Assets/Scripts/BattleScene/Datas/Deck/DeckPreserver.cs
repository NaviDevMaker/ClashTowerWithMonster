using System.Collections.Generic;
using UnityEngine;

public class DeckPreserver:ScriptableObject
{
    [SerializeField] List<CardData> choosenDecks;
    public List<CardData> ChoosenDecks { get => choosenDecks; set => choosenDecks = value; }
}

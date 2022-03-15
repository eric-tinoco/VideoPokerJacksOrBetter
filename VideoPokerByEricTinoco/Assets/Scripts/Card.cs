using UnityEngine;

/// <summary>
/// Basic Scriptable object that holds three main variables
/// cardValue: value of the card
/// cardType: suit of the card
/// cardSprite: appearance of the card of type sprite
/// </summary>
[CreateAssetMenu(fileName ="New Card", menuName ="Card")]
public class Card : ScriptableObject
{
    public int cardValue;
    public CardType cardType;
    public Sprite cardSprite;
}
// the types of suits
public enum CardType {
    Club,
    Heart,
    Diamond,
    Spade
}


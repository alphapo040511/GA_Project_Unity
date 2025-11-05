using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager instance;

    private void Awake()
    {
        instance = this;

        usedDeck = new List<CardData>(deckReference);
        Shuffle();
    }

    public List<CardData> deckReference = new List<CardData>();

    public List<CardData> hands = new List<CardData>();

    private Queue<CardData> deck = new Queue<CardData>();
    private List<CardData> usedDeck = new List<CardData>();

   
    public List<CardData> CardDraw()
    { 
        while(hands.Count < 4)
        {
            if (deck.Count == 0)
                Shuffle();
            else
                hands.Add(deck.Dequeue());
        }

        return hands;
    }

    public void UseCard(CardData card)
    {
        hands.Remove(card);
        usedDeck.Add(card);
    }

    private void Shuffle()
    {
        while(usedDeck.Count > 0)
        {
            // ·£´ýÇÏ°Ô ¼¯±â
            CardData currentData = usedDeck[Random.Range(0, usedDeck.Count)];
            deck.Enqueue(currentData);
            usedDeck.Remove(currentData);
        }

        Debug.Log("µ¦ ¼¯±â ¿Ï·á");
    }
}

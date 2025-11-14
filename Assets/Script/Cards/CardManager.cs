using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cards
{
    public class CardManager : Singleton<CardManager>
    {
        [SerializeField]
        private CardData NormalCard;
        [SerializeField]
        private CardData FriendCard;
        [SerializeField]
        private CardData OldManCard;
        [SerializeField]
        private CardData FamilyCard;

        public List<CardData> deck;

        private Dictionary<string, int> InitialValue = new Dictionary<string, int>()
        {
            { "Normal", 10 },
            { "Friend", 10 },
            { "OldMan", 10 },
            { "Family", 10 },
        };

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetDeck()
        {
            foreach (KeyValuePair<string, int> card in InitialValue)
            {
                for ( int value = 0; value < card.Value; value++)
                {
                    if (card.Key == "Normal")
                        deck.Add(NormalCard);
                    if (card.Key == "Friend")
                        deck.Add(FriendCard);
                    if (card.Key == "OldMan")
                        deck.Add(OldManCard);
                    if (card.Key == "Family")
                        deck.Add(FamilyCard);                 
                }
            }
        }

        public void FirstDraw(List<CardData> hand)
        {
            for (int i = 0; i < 3; i++)
                DrawCard(hand);
        }

        public void DrawCard(List<CardData> hand)
        {
            deck[0].coolTime = UnityEngine.Random.Range(2, 4);
            hand.Add(deck[0]);
            deck.RemoveAt(0);
        }

        public void ShuffleDeck()
        {
            deck = deck.OrderBy(a => Guid.NewGuid()).ToList();
        }
    }
}

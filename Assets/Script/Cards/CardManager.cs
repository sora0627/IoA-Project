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

        [SerializeField]
        private Transform parent;

        private Dictionary<string, int> InitialValue = new Dictionary<string, int>()
        {
            { "Normal", 50 },
            { "Friend", 20 },
            { "OldMan", 15 },
            { "Family", 15 },
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
            CardData cloneCard = null;

            foreach (KeyValuePair<string, int> card in InitialValue)
            {
                for ( int value = 0; value < card.Value; value++)
                {
                    if (card.Key == "Normal")
                        cloneCard = Instantiate(NormalCard);
                    if (card.Key == "Friend")
                        cloneCard = Instantiate(FriendCard);
                    if (card.Key == "OldMan")
                        cloneCard = Instantiate(OldManCard);
                    if (card.Key == "Family")
                        cloneCard = Instantiate(FamilyCard);

                    cloneCard.transform.parent = parent;
                    deck.Add(cloneCard);
                }
            }
        }

        public void FirstDraw(List<CardData> hand)
        {
            for (int i = 0; i < 2; i++)
                DrawCard(hand);
        }

        public void DrawCard(List<CardData> hand)
        {
            if (deck.Count > 0)
            {
                hand.Add(deck[0]);
                SetOutTime(deck[0]);
                deck.RemoveAt(0);
            }
        }

        public void ShuffleDeck()
        {
            deck = deck.OrderBy(a => Guid.NewGuid()).ToList();
        }

        public void SetOutTime(CardData cardData)
        {
            int coolTime, coolTime1;

            switch (cardData.Type)
            {
                case Move.CardType.Normal:
                case Move.CardType.OldMan:
                    coolTime = UnityEngine.Random.Range(1, 101);
                    cardData.outTime = OutTime(coolTime);
                    break; 
                case Move.CardType.Friend:
                case Move.CardType.Family:
                    coolTime = UnityEngine.Random.Range(1, 101);
                    coolTime1 = UnityEngine.Random.Range(1, 101);
                    cardData.outTime = OutTime(coolTime);
                    cardData.outTime1 = OutTime(coolTime1);
                    break;
            }

        }

        private int OutTime(int time)
        {
            int outTime;

            if (time < 15) outTime = 3;
            else if(time < 45) outTime = 4;
            else if(time < 80) outTime = 5;
            else outTime = 6;
            return outTime;
        }
    }
}

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

        [SerializeField]
        private GameObject Human;

        [SerializeField]
        private Transform GenerationPos;

        public List<CardData> deck;

        [SerializeField]
        private Transform parent;

        private Dictionary<string, int> InitialValue = new Dictionary<string, int>()
        {
            { "Normal", 10 },
            //{ "Friend", 10 },
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
            hand.Add(deck[0]);
            deck.RemoveAt(0);
        }

        public void ShuffleDeck()
        {
            deck = deck.OrderBy(a => Guid.NewGuid()).ToList();
        }

        public void CharacterGeneration(CardData SelectCard)
        {
            string cardName = SelectCard.CardName;
            GameObject cloneObject = null;
            
            if (cardName.Equals("Normal"))
            {
                cloneObject = Instantiate(Human, GenerationPos.position, Quaternion.identity);
            }

            if (cardName.Equals("Friend"))
            {

            }

            if (cardName.Equals("OldMan"))
            {

            }

            if (cardName.Equals("Family"))
            {

            }
        }
    }
}

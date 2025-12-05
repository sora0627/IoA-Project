using Cards;
using System.Collections;
using System.Collections.Generic;
using Systems;
using UnityEngine;

namespace Player
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [SerializeField]
        public List<CardData> hands;

        [SerializeField] private Transform parent;
        [SerializeField] private List<Transform> HandPos;
        [SerializeField] private List<bool> handRestrictions = new List<bool>();

        private GameObject currentSelectCard;
        private bool isDraw = false;
        private bool isGeneration = false;

        public bool isSet = false;

        public GameObject SelectCard
        {
            get { return currentSelectCard; }
            set { currentSelectCard = value; }
        }

        // Update is called once per frame
        void Update()
        {
            if (!GameManager.instance.IsPlayerTurn) return;

            if (GameManager.instance.IsSelect)
            {
                parent.gameObject.SetActive(true);

                if (!isDraw)
                {
                    TurnStart();
                }
            }

            if (GameManager.instance.IsSet)
            {
                parent.gameObject.SetActive(false);

                if (!isGeneration)
                {
                    isGeneration = true;
                    CardData cardData = SelectCard.GetComponent<CardData>();
                    Stage.StageManager.instance.CharacterGeneration(cardData);
                }
                if (!isSet)
                {
                    TurnEnd();
                }
            }
        }

        public void SetCard()
        {
            for (int index = 0; index < hands.Count; index++)
            {
                CardData cardData = hands[index];
                GameObject card = cardData.gameObject;
                card.transform.position = HandPos[index].position;
                cardData.coolTime = Random.Range(2, 5);
                card.transform.parent = parent;
            }
        }

        public void UseHand(int index)
        {
            Destroy(currentSelectCard);
            hands.RemoveAt(index);
            SetCard();
            GameManager.instance.IsSet = true;
        }

        void TurnStart()
        {
            isSet = false;
            isDraw = true;
            CardManager.instance.DrawCard(hands);
            SetCard();

            handRestrictions.Clear();
            foreach (CardData card in hands)
            {
                handRestrictions.Add(card.IsRestrictedType);
            }

            Move.MouseDrag.CheckGameOverAtStartOfTurn("Toilet", true, handRestrictions);
        }

        void TurnEnd()
        {
            GameManager.instance.TurnChange();
            isDraw = false;
            isGeneration = false;
        }
    }
}

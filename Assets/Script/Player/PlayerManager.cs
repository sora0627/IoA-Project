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

        [SerializeField]
        private Transform parent;

        [SerializeField]
        private List<Transform> HandPos;

        private GameObject currentSelectCard;
        private bool isDraw = false;

        public GameObject SelectCard
        {
            get { return currentSelectCard; }
            set { currentSelectCard = value; }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!GameManager.instance.IsPlayerTurn) return;

            if (GameManager.instance.IsSelect)
            {
                if (!isDraw)
                {
                    isDraw = true;
                    CardManager.instance.DrawCard(hands);
                    SetCard();
                }
            }

            if (GameManager.instance.IsSet)
            {
                parent.gameObject.SetActive(false);
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

        void TurnEnd()
        {
            parent.gameObject.SetActive(true);
            Debug.Log("EnemyTurn");
            GameManager.instance.IsSelect = true;
            GameManager.instance.IsPlayerTurn = false;
            isDraw = false;
        }
    }
}

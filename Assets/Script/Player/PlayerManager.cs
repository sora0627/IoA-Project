using Cards;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [SerializeField]
        public List<CardData> hands;

        [SerializeField]
        private Transform parent;

        private GameObject currentSelectCard;

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

        }

        public void SetCard()
        {
            for (int index = 0; index < hands.Count; index++)
            {
                int posX = 2, posY = -3;
                CardData cardData = hands[index];
                GameObject card = cardData.gameObject;
                card.transform.position = new Vector2(-posX + posX * index, posY);
                cardData.coolTime = Random.Range(2, 4);
                card.transform.parent = parent;
            }
        }

        public void UseHand(int index)
        {
            Destroy(currentSelectCard);
            hands.RemoveAt(index);
            FirstSetCard();
        }
    }
}

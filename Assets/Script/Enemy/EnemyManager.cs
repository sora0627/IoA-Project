using Cards;
using System.Collections;
using System.Collections.Generic;
using Systems;
using UnityEngine;

namespace Enemy
{
    public class EnemyManager : Singleton<EnemyManager>
    {
        [SerializeField]
        public List<CardData> hands;

        private bool isDraw = false;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager.instance.IsPlayerTurn) return;

            if (GameManager.instance.IsSelect)
            {
                if (!isDraw)
                {
                    isDraw = true;
                    CardManager.instance.DrawCard(hands);
                }

                int index = SelectCard();
                UseHand(index);
            }

            if (GameManager.instance.IsSet)
            {
                if (Input.GetKeyDown(KeyCode.N))
                {
                    TurnEnd();
                }
            }
        }

        //Ç±Ç±Ç…CPUÇÃÉvÉçÉOÉâÉÄÇÇ©Ç≠
        int SelectCard()
        {
            return 1;
        }

        public void UseHand(int index)
        {
            hands.RemoveAt(index);
            GameManager.instance.IsSet = true;
        }

        void TurnEnd()
        {
            GameManager.instance.TurnChange();
            isDraw = false;
        }
    }
}

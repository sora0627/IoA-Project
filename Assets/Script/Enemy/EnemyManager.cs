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
            }

            if (GameManager.instance.IsSet)
            {

            }
        }

        public void UseHand(int index)
        {
            hands.RemoveAt(index);
            GameManager.instance.IsSet = true;
        }

        void TurnEnd()
        {
            Debug.Log("PlayerTurn");
            GameManager.instance.IsSelect = true;
            GameManager.instance.IsPlayerTurn = true;
            isDraw = false;
        }
    }
}

using Cards;
using Stage;
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

        //ここにCPUのプログラムをかく
        int SelectCard()
        {
            List<bool> isOccupied = new List<bool>();

            isOccupied = IsOccupied();
            return 1;
        }

        /// <summary>
        /// トイレに人がいるか調べる
        /// true -> 人がいる
        /// false　-> 人がいない
        /// </summary>
        /// <returns></returns>
        private List<bool> IsOccupied()
        {
            List<bool> isOccupied = new List<bool>();
            foreach (GameObject toilet in StageManager.instance.toilet)
            {
                ToiletHighlight toiletHighlight = toilet.GetComponent<ToiletHighlight>();
                isOccupied.Add(toiletHighlight.IsOccupied);
            }

            return isOccupied;
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

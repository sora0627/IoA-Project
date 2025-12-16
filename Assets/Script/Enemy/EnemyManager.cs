using Cards;
using Stage;
using System.Collections;
using System.Collections.Generic;
using Systems;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Move;

namespace Enemy
{
    public class EnemyManager : Singleton<EnemyManager>
    {
        [SerializeField] public List<CardData> hands;

        [SerializeField] private Transform parent;

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
                TrueStart();

                int index = SelectCard();
                if (index != -1) UseHand(index);
                else
                {
                    GameManager.instance.IsGameEnd = true;
                }

            }

            else if (GameManager.instance.IsSet)
            {


                if (Input.GetKeyDown(KeyCode.N) || GameManager.instance.IsTrueEnd)
                {
                    GameManager.instance.IsTrueEnd = true;
                    TurnEnd();
                }
            }
        }

        //ここにCPUのプログラムをかく
        int SelectCard()
        {
            List<int> isOccupied = new List<int>();
            List<CardType> priority = new List<CardType>();
            int index = -1;

            isOccupied = Canposition(IsOccupied());
            priority = Priority(isOccupied);
            List<CardType> handType = hands.Select(obj => obj.Type).ToList();

            foreach (CardType cardType in priority)
            {
                if (handType.Contains(cardType))
                {
                    index = handType.IndexOf(cardType);
                    Debug.Log(cardType);
                    break;
                }
            }
            return index;
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

        /// <summary>
        /// 空いている場所を書き出していく
        /// 連続でどれだけ空いているかを調べてくる
        /// </summary>
        /// <param name="bools"></param>
        /// <returns></returns>
        private List<int> Canposition(List<bool> bools)
        {
            List<int> canPosition = new List<int>();
            int value = 0;

            bools.Add(true);

            foreach( bool flag  in bools)
            {
                if (!flag)
                {
                    value++;
                }
                else
                {
                    canPosition.Add(value);
                    value = 0;
                }
            }
            return canPosition;

        }

        List<CardType> Priority(List<int> position)
        {
            bool haveOldman = false;

            foreach (CardData cardData in hands)
            {
                if (cardData.Equals(CardType.OldMan)) haveOldman = true;
            }

            if (position.Max() >= 6 && !haveOldman)
            {
                return new List<CardType>() { CardType.Friend, CardType.Family, CardType.Normal, CardType.OldMan };
            }
            else if (position.Max() >= 3)
            {
                return new List<CardType> { CardType.Friend, CardType.Family, CardType.Normal, CardType.OldMan };
            }
            else
            {
                return new List<CardType> { CardType.OldMan };
            }
        }

        public void UseHand(int index)
        {
            StageManager.instance.CharacterGeneration(hands[index]);
            hands.RemoveAt(index);
            GameManager.instance.IsSet = true;
        }

        public void SetHuman()
        {

        }

        void TrueStart()
        {
            if (!isDraw)
            {
                isDraw = true;
                CardManager.instance.DrawCard(hands);

                MouseDrag.CheckGameOverAtStartOfTurn(true, hands);
            }
        }

        void TurnEnd()
        {
            isDraw = false;
        }
    }
}

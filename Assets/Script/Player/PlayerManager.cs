using Cards;
using System.Collections;
using System.Collections.Generic;
using Systems;
using UnityEngine;
using Stage;
using Move;

namespace Player
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [SerializeField]
        public List<CardData> hands;

        [SerializeField] private Transform parent;
        [SerializeField] private List<Transform> HandPos;

        private GameObject currentSelectCard;
        private bool isDraw = false;
        private bool isGeneration = false;

        public bool isSet = false;

        public GameObject SelectCard
        {
            get { return currentSelectCard; }
            set { currentSelectCard = value; }
        }

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
            if (index < 0 || index >= hands.Count) return;

            CardData cardToUse = hands[index];

            // 現在の盤面で配置可能かチェック
            if (!CanPlaceCard(cardToUse))
            {
                Debug.Log($"配置できない条件です: {cardToUse.Type} (ペア成立せず)");
                return;
            }

            Destroy(currentSelectCard);
            hands.RemoveAt(index);
            SetCard();
            GameManager.instance.IsSet = true;
        }

        private bool CanPlaceCard(CardData cardData)
        {
            if (StageManager.instance == null) return false;
            var toilets = StageManager.instance.toilet;

            List<ToiletHighlight> slots = new List<ToiletHighlight>();
            foreach (var t in toilets)
            {
                if (t != null) slots.Add(t.GetComponent<ToiletHighlight>());
            }

            if (slots.Count == 0) return false;

            CardType type = cardData.Type;

            // --- 1枚目か2枚目かの判定 ---
            int sameTypeCountInHand = 0;
            foreach (var c in hands)
            {
                if (c.Type == type) sameTypeCountInHand++;
            }

            ToiletHighlight partnerSlot = null;
            int partnerIndex = -1;
            bool isPartnerOnBoard = false;

            if (type == CardType.Family || type == CardType.Friend)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].IsOccupied && slots[i].OccupyingObject != null)
                    {
                        if (slots[i].OccupyingObject.cardType == type)
                        {
                            partnerSlot = slots[i];
                            partnerIndex = i;
                            isPartnerOnBoard = true;
                            break;
                        }
                    }
                }
            }

            // 手札が偶数枚なら、盤面の同種は他人扱い（1枚目として配置）
            if (isPartnerOnBoard && sameTypeCountInHand % 2 == 0)
            {
                partnerSlot = null;
            }

            // --- 1枚目（相方不在）の場合のペア成立シミュレーション ---
            if (partnerSlot == null)
            {
                if (type == CardType.Friend)
                {
                    bool result = CanPlaceFriendPairSimulation(slots);
                    if (!result) Debug.Log("Friend: 1枚目配置シミュレーション失敗");
                    return result;
                }
                else if (type == CardType.Family)
                {
                    bool asParent = CanPlaceFamilyPairSimulation(slots, true);
                    bool asChild = CanPlaceFamilyPairSimulation(slots, false);
                    return asParent || asChild;
                }
            }

            // --- それ以外（通常、老人、または相方ありの2枚目）の判定 ---
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsOccupied) continue;

                bool canPlaceHere = false;

                ToiletHighlight left = (i > 0) ? slots[i - 1] : null;
                ToiletHighlight right = (i < slots.Count - 1) ? slots[i + 1] : null;
                bool isLeftOccupied = left != null && left.IsOccupied;
                bool isRightOccupied = right != null && right.IsOccupied;
                bool hasOccupiedNeighbor = isLeftOccupied || isRightOccupied;

                switch (type)
                {
                    case CardType.OldMan:
                        canPlaceHere = true;
                        break;

                    case CardType.Family:
                        if (partnerSlot != null)
                        {
                            // 2枚目: 相方の隣ならOK
                            if (Mathf.Abs(i - partnerIndex) == 1) canPlaceHere = true;
                        }
                        break;

                    case CardType.Friend:
                        // 2枚目: 他人の隣NG
                        bool leftOk = !isLeftOccupied || (left.OccupyingObject != null && left.OccupyingObject.cardType == type);
                        bool rightOk = !isRightOccupied || (right.OccupyingObject != null && right.OccupyingObject.cardType == type);
                        if (leftOk && rightOk) canPlaceHere = true;
                        break;

                    case CardType.Normal:
                    default:
                        if (!hasOccupiedNeighbor) canPlaceHere = true;
                        break;
                }

                if (canPlaceHere) return true;
            }

            return false;
        }

        private bool CanPlaceFriendPairSimulation(List<ToiletHighlight> slots)
        {
            // 全スロットを走査し、1枚目を置く候補(i)を探す
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsOccupied) continue;

                // 1枚目: 他人の隣には置けない（相方不在なので両隣空き必須）
                bool i_LeftBlocked = (i > 0) && slots[i - 1].IsOccupied;
                bool i_RightBlocked = (i < slots.Count - 1) && slots[i + 1].IsOccupied;

                if (i_LeftBlocked || i_RightBlocked) continue;

                // 2枚目の場所があるかチェック
                for (int j = 0; j < slots.Count; j++)
                {
                    if (i == j) continue;
                    if (slots[j].IsOccupied) continue;

                    // 2枚目(j): 左隣チェック
                    bool j_LeftOk = true;
                    if (j > 0)
                    {
                        // 左隣が埋まっている場合
                        if (slots[j - 1].IsOccupied)
                        {
                            // その埋まっているのが i (1枚目) でなければNG
                            if ((j - 1) != i) j_LeftOk = false;
                        }
                    }

                    // 2枚目(j): 右隣チェック
                    bool j_RightOk = true;
                    if (j < slots.Count - 1)
                    {
                        // 右隣が埋まっている場合
                        if (slots[j + 1].IsOccupied)
                        {
                            // その埋まっているのが i (1枚目) でなければNG
                            if ((j + 1) != i) j_RightOk = false;
                        }
                    }

                    // 両方OKならペア成立可能
                    if (j_LeftOk && j_RightOk)
                    {
                        // Debug.Log($"Friendペア成立候補: 1枚目[{i}], 2枚目[{j}]");
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CanPlaceFamilyPairSimulation(List<ToiletHighlight> slots, bool isParent)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsOccupied) continue;

                // 1枚目の条件
                if (!isParent) // 子
                {
                    bool i_LeftBlocked = (i > 0) && slots[i - 1].IsOccupied;
                    bool i_RightBlocked = (i < slots.Count - 1) && slots[i + 1].IsOccupied;
                    if (i_LeftBlocked || i_RightBlocked) continue;
                }

                // 2枚目（相方）の条件
                // 左隣(i-1)に2枚目を置けるか？
                if (i > 0 && !slots[i - 1].IsOccupied)
                {
                    bool canPlaceLeft = true;
                    if (isParent) // 2枚目は子
                    {
                        if (i - 1 > 0 && slots[i - 2].IsOccupied) canPlaceLeft = false;
                    }
                    if (canPlaceLeft) return true;
                }

                // 右隣(i+1)に2枚目を置けるか？
                if (i < slots.Count - 1 && !slots[i + 1].IsOccupied)
                {
                    bool canPlaceRight = true;
                    if (isParent) // 2枚目は子
                    {
                        if (i + 1 < slots.Count - 1 && slots[i + 2].IsOccupied) canPlaceRight = false;
                    }
                    if (canPlaceRight) return true;
                }
            }
            return false;
        }

        void TurnStart()
        {
            isSet = false;
            isDraw = true;
            CardManager.instance.DrawCard(hands);
            SetCard();

            Move.MouseDrag.CheckGameOverAtStartOfTurn(true, hands);
        }

        void TurnEnd()
        {
            GameManager.instance.TurnChange();
            isDraw = false;
            isGeneration = false;
        }
    }
}
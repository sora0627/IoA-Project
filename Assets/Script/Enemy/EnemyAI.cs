using Cards;
using Stage;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Move;

namespace Enemy
{
    /// <summary>
    /// 敵の「思考」を担当するクラス
    /// どのカードを使うか、どこに配置するかの計算を行います。
    /// </summary>
    public class EnemyAI
    {
        /// <summary>
        /// 手札と盤面から、使用するべきカードのインデックスを選択します。
        /// </summary>
        public int SelectCard(List<CardData> hands, List<ToiletHighlight> targetHighlights, out CardType selectedCardType)
        {
            selectedCardType = CardType.Normal;

            if (hands.Count == 0 || targetHighlights.Count == 0) return -1;

            // 盤面の空き状況から優先度を決定
            List<bool> isOccupiedList = targetHighlights.Select(h => h.IsOccupied).ToList();
            List<int> emptySpaceCounts = GetContinuousEmptySpaces(isOccupiedList);
            List<CardType> priority = GetPriority(emptySpaceCounts, hands);

            List<CardType> handTypes = hands.Select(obj => obj.Type).ToList();

            // 優先度の高い順に、盤面に配置可能なカードを探す
            foreach (CardType cardType in priority)
            {
                if (handTypes.Contains(cardType) && CanPlaceCardOnBoard(cardType, targetHighlights))
                {
                    selectedCardType = cardType;
                    return handTypes.IndexOf(cardType);
                }
            }

            return -1; // 配置できるカードがない（手詰まり）
        }

        /// <summary>
        /// 1体目の配置場所をランダムに決定します。
        /// </summary>
        public int DetermineFirstPlacement(MouseDrag myDrag, List<ToiletHighlight> targetHighlights)
        {
            if (myDrag == null || targetHighlights.Count == 0) return -1;

            List<int> validIndices = new List<int>();
            CardType type = myDrag.cardType;
            bool isParent = myDrag.isFamilyParent;

            for (int i = 0; i < targetHighlights.Count; i++)
            {
                if (targetHighlights[i].IsOccupied) continue;
                if (!CheckFirstPlacementCondition(i, targetHighlights, type, isParent)) continue;

                // ペア生成カード（Friend, Family）の場合は、2体目が置けるかも確認する
                if (type == CardType.Friend || type == CardType.Family)
                {
                    bool canPlaceSecond = false;
                    for (int j = 0; j < targetHighlights.Count; j++)
                    {
                        if (i == j || targetHighlights[j].IsOccupied) continue;
                        if (CheckSecondPlacementCondition(i, j, targetHighlights, type, isParent))
                        {
                            canPlaceSecond = true;
                            break;
                        }
                    }
                    if (canPlaceSecond) validIndices.Add(i);
                }
                else
                {
                    // 1体のみのカードなら条件クリアでOK
                    validIndices.Add(i);
                }
            }

            return RandomSelect(validIndices);
        }

        /// <summary>
        /// 1体目の配置場所（firstIndex）を基に、2体目の配置場所をランダムに決定します。
        /// </summary>
        public int DetermineSecondPlacement(MouseDrag myDrag, int firstIndex, List<ToiletHighlight> targetHighlights)
        {
            if (myDrag == null || targetHighlights.Count == 0 || firstIndex == -1) return -1;

            List<int> validIndices = new List<int>();
            CardType type = myDrag.cardType;

            // ★修正点: CheckSecondPlacementCondition が求めているのは「1体目が親かどうか(isParent1)」
            // 家族(Family)の場合、2体目(myDrag)が親なら1体目は子、2体目が子なら1体目は親になります。
            bool isParent1 = type == CardType.Family ? !myDrag.isFamilyParent : myDrag.isFamilyParent;

            for (int j = 0; j < targetHighlights.Count; j++)
            {
                if (firstIndex == j || targetHighlights[j].IsOccupied) continue;

                if (CheckSecondPlacementCondition(firstIndex, j, targetHighlights, type, isParent1))
                {
                    validIndices.Add(j);
                }
            }

            return RandomSelect(validIndices);
        }

        // =================================================================================
        // 以下、内部の判定ロジック群
        // =================================================================================

        private List<int> GetContinuousEmptySpaces(List<bool> isOccupiedList)
        {
            List<int> canPosition = new List<int>();
            int value = 0;

            // 副作用を防ぐためローカルコピーを作成して処理
            List<bool> bools = new List<bool>(isOccupiedList);
            bools.Add(true); // 終端処理用

            foreach (bool isOccupied in bools)
            {
                if (!isOccupied)
                {
                    value++;
                }
                else
                {
                    if (value > 0) canPosition.Add(value);
                    value = 0;
                }
            }
            return canPosition;
        }

        private List<CardType> GetPriority(List<int> emptySpaces, List<CardData> hands)
        {
            bool haveOldman = hands.Any(card => card.Type == CardType.OldMan);

            if (emptySpaces.Count > 0 && emptySpaces.Max() >= 6 && !haveOldman)
            {
                return new List<CardType>() { CardType.Friend, CardType.Family, CardType.Normal, CardType.OldMan };
            }
            else
            {
                return new List<CardType> { CardType.Family, CardType.Normal, CardType.Friend, CardType.OldMan };
            }
        }

        private bool CanPlaceCardOnBoard(CardType type, List<ToiletHighlight> targetHighlights)
        {
            for (int i = 0; i < targetHighlights.Count; i++)
            {
                if (targetHighlights[i].IsOccupied) continue;

                // 1体目の条件を満たすか
                if (!CheckFirstPlacementCondition(i, targetHighlights, type, false)) continue;

                // 1体のみのカード（OldMan, Normal）なら、この時点で配置可能と判断して良い
                if (type == CardType.OldMan || type == CardType.Normal)
                {
                    return true;
                }

                // 2体目の場所を探す（ペア生成カードのみ）
                for (int j = 0; j < targetHighlights.Count; j++)
                {
                    if (i == j || targetHighlights[j].IsOccupied) continue;

                    if (CheckSecondPlacementCondition(i, j, targetHighlights, type, true))
                    {
                        return true; // 1組でもペア成立場所があればOK
                    }
                }
            }
            return false;
        }

        private bool CheckFirstPlacementCondition(int idx, List<ToiletHighlight> targetHighlights, CardType type, bool isParent)
        {
            bool leftOccupied = (idx > 0 && targetHighlights[idx - 1].IsOccupied);
            bool rightOccupied = (idx < targetHighlights.Count - 1 && targetHighlights[idx + 1].IsOccupied);

            switch (type)
            {
                case CardType.OldMan:
                    return true;
                case CardType.Normal:
                case CardType.Friend:
                    if (leftOccupied || rightOccupied) return false;
                    return true;
                case CardType.Family:
                    if (isParent) return true;
                    if (leftOccupied || rightOccupied) return false;
                    return true;
            }
            return false;
        }

        private bool CheckSecondPlacementCondition(int idx1, int idx2, List<ToiletHighlight> targetHighlights, CardType type, bool isParent1)
        {
            bool leftIsStranger = false;
            if (idx2 > 0)
            {
                if (targetHighlights[idx2 - 1].IsOccupied) leftIsStranger = true;
                if ((idx2 - 1) == idx1) leftIsStranger = false;
            }

            bool rightIsStranger = false;
            if (idx2 < targetHighlights.Count - 1)
            {
                if (targetHighlights[idx2 + 1].IsOccupied) rightIsStranger = true;
                if ((idx2 + 1) == idx1) rightIsStranger = false;
            }

            switch (type)
            {
                case CardType.OldMan:
                case CardType.Normal:
                    return !leftIsStranger && !rightIsStranger;

                case CardType.Friend:
                    if (leftIsStranger || rightIsStranger) return false;
                    return true;

                case CardType.Family:
                    if (Mathf.Abs(idx1 - idx2) != 1) return false;
                    if (isParent1)
                    {
                        if (leftIsStranger || rightIsStranger) return false;
                    }
                    return true;
            }
            return false;
        }

        private int RandomSelect(List<int> list)
        {
            if (list == null || list.Count == 0) return -1;
            return list[Random.Range(0, list.Count)];
        }
    }
}
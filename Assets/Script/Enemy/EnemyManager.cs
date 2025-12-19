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
        public string dropTargetsRootName = "ToiletParent";

        [SerializeField] public List<CardData> hands;

        [SerializeField] private Transform parent;

        private List<ToiletHighlight> targetHighlights = new List<ToiletHighlight>();
        private bool isDraw = false;
        private CardType currentSelectCardType;

        public CardType CurrentSelectCard
        {
            get { return currentSelectCardType; }
            set { currentSelectCardType = value; }
        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager.instance.IsPlayerTurn) return;

            if (GameManager.instance.IsSelect)
            {
                TurnStart();

                int index = SelectCard();
                if (index != -1)
                {
                    UseHand(index);
                    GameManager.instance.IsTrueEnd = true;
                }
                else
                {
                    GameManager.instance.IsGameEnd = true;
                    return;
                }
            }

            if (GameManager.instance.IsTrueEnd)
            {
                TurnEnd();
            }
        }

        public void Initialization()
        {
            isDraw = false;
        }

        public void SetTargetHighlights(List<GameObject> toilet)
        {
            targetHighlights.Clear();
            if (StageManager.instance.toilet.Count > 0)
            {
                foreach (var obj in toilet)
                {
                    if (obj == null) continue;
                    var hl = obj.GetComponent<ToiletHighlight>();
                    if (hl != null) targetHighlights.Add(hl);
                }
            }
        }

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
                Debug.Log(cardType + ":" + CheckPlaceablePairs(cardType));
                if (handType.Contains(cardType) && CheckPlaceablePairs(cardType))
                {
                    index = handType.IndexOf(cardType);
                    CurrentSelectCard = cardType;
                    break;
                }
            }
            return index;
        }

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
            else
            {
                return new List<CardType> { CardType.Family, CardType.Normal, CardType.Friend, CardType.OldMan };
            }
        }

        public void UseHand(int index)
        {
            StageManager.instance.CharacterGeneration(hands[index]);
            hands.RemoveAt(index);
        }

        public void SetHuman(GameObject cloneObj, GameObject cloneObj1)
        {
            List<int> setPosition;
            List<int> setPosition1;

            if (cloneObj == null) return;

            if (cloneObj1 != null)
            {
                MouseDrag myDrag2 = cloneObj1.GetComponent<MouseDrag>();
                DebugPlaceablePairs(myDrag2);
            }

            MouseDrag myDrag = cloneObj.GetComponent<MouseDrag>();
            setPosition = GetPlaceableIndices(myDrag);

            //-----変更点(CPU強化point)----//
            int firstIndex = RandomSelect(setPosition);
            //-----------------------------//

            targetHighlights[firstIndex].SetOccupier(myDrag);
            myDrag.currentSlot = targetHighlights[firstIndex];
            cloneObj.transform.position = targetHighlights[firstIndex].gameObject.transform.position;
            LockCard(cloneObj);

            if (cloneObj1 == null) return;
            MouseDrag myDrag1 = cloneObj1.GetComponent<MouseDrag>();
            setPosition1 = GetSecondPlaceableIndices(myDrag1, firstIndex);

            //-----変更点(CPU強化point)----//
            int secondIndex = RandomSelect(setPosition1);
            //-----------------------------//

            targetHighlights[secondIndex].SetOccupier(myDrag1);
            myDrag1.currentSlot = targetHighlights[secondIndex];
            cloneObj1.transform.position = targetHighlights[secondIndex].gameObject.transform.position;
            LockCard(cloneObj1);
        }

        /// <summary>
        /// カードデータ（MouseDrag付き）を受け取り、1体目と2体目の配置可能ペアを計算してログ出力する
        /// </summary>
        public void DebugPlaceablePairs(MouseDrag myDrag)
        {
            if (StageManager.instance == null) return;

            if (targetHighlights.Count == 0) return;

            if (myDrag == null)
            {
                return;
            }

            CardType type = myDrag.cardType;
            bool isParent = myDrag.isFamilyParent;

            bool anyPairFound = false;

            // 1. 全スロットを走査して「1体目の候補地」を探す
            for (int i = 0; i < targetHighlights.Count; i++)
            {
                if (targetHighlights[i].IsOccupied) continue;

                // 1体目が条件を満たすかチェック
                if (!CheckFirstPlacementCondition(i, targetHighlights, type, isParent)) continue;

                // 2. 「1体目をiに置いた」と仮定して、「2体目の候補地」を探す
                List<int> validSecondIndices = new List<int>();

                for (int j = 0; j < targetHighlights.Count; j++)
                {
                    if (i == j) continue; // 同じ場所には置けない
                    if (targetHighlights[j].IsOccupied) continue; // 埋まっている場所には置けない

                    // 2体目が条件を満たすかチェック（iには味方がいる前提）
                    if (CheckSecondPlacementCondition(i, j, targetHighlights, type, isParent))
                    {
                        validSecondIndices.Add(j);
                    }
                }

                // 2体目の置き場所が1つでもあれば、1体目の配置場所として有効
                if (validSecondIndices.Count > 0)
                {
                    anyPairFound = true;
                    Debug.Log($"1体目[{i}] OK -> 2体目候補: [{string.Join(", ", validSecondIndices)}]");
                }
            }

            if (!anyPairFound)
            {
                Debug.Log("配置可能なペア（両方置ける場所）が見つかりませんでした。");
            }
            Debug.Log("=========================================");
        }

        /// <summary>
        /// カードデータ（MouseDrag付き）を受け取り、1体目と2体目の配置可能ペアを計算して配置できるか判断する
        /// </summary>
        public bool CheckPlaceablePairs(CardType type)
        {
            bool anyPairFound = false;

            // 1. 全スロットを走査して「1体目の候補地」を探す
            for (int i = 0; i < targetHighlights.Count; i++)
            {
                if (targetHighlights[i].IsOccupied) continue;

                // 1体目が条件を満たすかチェック
                if (!CheckFirstPlacementCondition(i, targetHighlights, type, false)) continue;

                // 2. 「1体目をiに置いた」と仮定して、「2体目の候補地」を探す
                List<int> validSecondIndices = new List<int>();

                for (int j = 0; j < targetHighlights.Count; j++)
                {
                    if (i == j) continue; // 同じ場所には置けない
                    if (targetHighlights[j].IsOccupied) continue; // 埋まっている場所には置けない

                    // 2体目が条件を満たすかチェック（iには味方がいる前提）
                    if (CheckSecondPlacementCondition(i, j, targetHighlights, type, true))
                    {
                        validSecondIndices.Add(j);
                    }
                }

                // 2体目の置き場所が1つでもあれば、1体目の配置場所として有効
                if (validSecondIndices.Count > 0)
                {
                    anyPairFound = true;
                }
            }
            return anyPairFound;
        }

        /// <summary>
        /// 1体目を指定したインデックスに置いたと仮定して、2体目を配置可能なインデックスのリストを返します。
        /// </summary>
        /// <param name="cardData">カードデータ</param>
        /// <param name="firstIndex">1体目の仮配置インデックス</param>
        /// <returns>2体目の配置可能インデックスリスト</returns>
        public List<int> GetSecondPlaceableIndices(MouseDrag myDrag, int firstIndex)
        {
            List<int> secondIndices = new List<int>();
            secondIndices.Clear();
            if (StageManager.instance == null) return secondIndices;

            if (firstIndex < 0 || firstIndex >= targetHighlights.Count) return secondIndices;

            if (myDrag == null) return secondIndices;

            CardType type = myDrag.cardType;
            bool isParent = myDrag.isFamilyParent;

            // 1体目を firstIndex に置いたと仮定して、2体目の候補を探す
            switch (type)
            {
                case CardType.Friend:
                    for (int j = 0; j < targetHighlights.Count; j++)
                    {
                        if (j == firstIndex) continue; // 同じ場所は不可
                        if (targetHighlights[j].IsOccupied) continue; // 埋まってる場所は不可

                        // 2体目(j)の条件: 隣が「空き」または「1体目(firstIndex)」ならOK（他人の隣はNG）
                        bool leftOk = true;
                        if (j > 0)
                        {
                            // 左が埋まっていて、かつ 1体目(firstIndex) でなければNG（＝他人）
                            if (targetHighlights[j - 1].IsOccupied && (j - 1) != firstIndex) leftOk = false;
                        }

                        bool rightOk = true;
                        if (j < targetHighlights.Count - 1)
                        {
                            // 右が埋まっていて、かつ 1体目(firstIndex) でなければNG（＝他人）
                            if (targetHighlights[j + 1].IsOccupied && (j + 1) != firstIndex) rightOk = false;
                        }

                        if (leftOk && rightOk) secondIndices.Add(j);
                    }
                    break;

                case CardType.Family:
                    // 2体目は必ず隣接していなければならない

                    // 左隣(firstIndex - 1)をチェック
                    if (firstIndex > 0 && !targetHighlights[firstIndex - 1].IsOccupied)
                    {
                        // 2体目に対する条件チェック
                        bool canPlace = true;
                        if (isParent)
                        {
                            // 1体目が親(true) -> 2体目は子。その左(firstIndex-2)が他人だとNG
                            if (firstIndex - 1 > 0 && targetHighlights[firstIndex - 2].IsOccupied) canPlace = false;
                        }
                        // 1体目が子(false) -> 2体目は親。制限なし

                        if (canPlace) secondIndices.Add(firstIndex - 1);
                    }

                    // 右隣(firstIndex + 1)をチェック
                    if (firstIndex < targetHighlights.Count - 1 && !targetHighlights[firstIndex + 1].IsOccupied)
                    {
                        // 2体目に対する条件チェック
                        bool canPlace = true;
                        if (isParent)
                        {
                            // 1体目が親(true) -> 2体目は子。その右(firstIndex+2)が他人だとNG
                            if (firstIndex + 1 < targetHighlights.Count - 1 && targetHighlights[firstIndex + 2].IsOccupied) canPlace = false;
                        }
                        // 1体目が子(false) -> 2体目は親。制限なし

                        if (canPlace) secondIndices.Add(firstIndex + 1);
                    }
                    break;

                case CardType.Normal:
                case CardType.OldMan:
                    // 1体生成なので、2体目の配置場所はない（空リストを返す）
                    break;
            }
            Debug.Log($"Card: {type}, Placeable Indices: {string.Join(", ", secondIndices)}");
            return secondIndices;
        }

        /// <summary>
        /// 1体目の配置条件チェック
        /// </summary>
        private bool CheckFirstPlacementCondition(int idx, List<ToiletHighlight> targetHighlights, CardType type, bool isParent)
        {
            // 隣接情報の取得
            bool leftOccupied = (idx > 0 && targetHighlights[idx - 1].IsOccupied);
            bool rightOccupied = (idx < targetHighlights.Count - 1 && targetHighlights[idx + 1].IsOccupied);

            switch (type)
            {
                case CardType.OldMan:
                    // 制限なし
                    return true;

                case CardType.Normal:
                    // 他人の隣はNG
                    if (leftOccupied || rightOccupied) return false;
                    return true;

                case CardType.Friend:
                    // 相方不在の1体目：他人の隣はNG
                    if (leftOccupied || rightOccupied) return false;
                    return true;

                case CardType.Family:
                    // 親：制限なし（他人の隣でもOK）
                    if (isParent) return true;
                    // 子：他人の隣はNG
                    if (leftOccupied || rightOccupied) return false;
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 2体目の配置条件チェック（idx1に1体目があると仮定）
        /// </summary>
        private bool CheckSecondPlacementCondition(int idx1, int idx2, List<ToiletHighlight> targetHighlights, CardType type, bool isParent1)
        {
            // idx2の隣接情報の取得
            // ※ idx1 は「1体目（味方）」として扱うため、IsOccupied=falseでも「埋まっているもの」として判定する

            bool leftIsStranger = false;
            if (idx2 > 0)
            {
                if (targetHighlights[idx2 - 1].IsOccupied) leftIsStranger = true; // 埋まってたら他人
                if ((idx2 - 1) == idx1) leftIsStranger = false;        // ただし1体目なら味方
            }

            bool rightIsStranger = false;
            if (idx2 < targetHighlights.Count - 1)
            {
                if (targetHighlights[idx2 + 1].IsOccupied) rightIsStranger = true; // 埋まってたら他人
                if ((idx2 + 1) == idx1) rightIsStranger = false;        // ただし1体目なら味方
            }

            switch (type)
            {
                case CardType.OldMan:
                case CardType.Normal:
                    // 1体生成タイプなので、ここでは常にtrueかfalse（仕様による）
                    // ペア生成でない場合は2体目は存在しないが、一応通すならNormal同様
                    return !leftIsStranger && !rightIsStranger;

                case CardType.Friend:
                    // 他人の隣はNG（空き、または1体目ならOK）
                    if (leftIsStranger || rightIsStranger) return false;
                    return true;

                case CardType.Family:
                    // 1. 吸着ルール：必ず1体目の隣でなければならない
                    if (Mathf.Abs(idx1 - idx2) != 1) return false;

                    // 2. 親子ルール
                    // 1体目が親(true) -> 2体目は子 -> 2体目の隣(1体目じゃない側)が他人だとNG
                    if (isParent1)
                    {
                        if (leftIsStranger || rightIsStranger) return false;
                    }
                    // 1体目が子(false) -> 2体目は親 -> 2体目は他人の隣でもOK（制限なし）

                    return true;
            }
            return false;
        }

        // 既存のメソッド（必要であれば残す、または上記ロジックに統合）
        public List<int> GetPlaceableIndices(MouseDrag myDrag)
        {
            // 簡易実装: 上記ロジックを使って、1体目が置ける場所のリストだけ返す例
            List<int> validIndices = new List<int>();
            if (StageManager.instance == null) return validIndices;

            if (targetHighlights.Count == 0) return validIndices;

            if (myDrag == null) return validIndices;

            CardType type = myDrag.cardType;
            bool isParent = myDrag.isFamilyParent;

            for (int i = 0; i < targetHighlights.Count; i++)
            {
                if (targetHighlights[i].IsOccupied) continue;
                if (!CheckFirstPlacementCondition(i, targetHighlights, type, isParent)) continue;

                // 2体目チェック（Friend/Familyのみ）
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
                    // 1体のみ配置のカードなら条件クリアでOK
                    validIndices.Add(i);
                }
            }
            Debug.Log($"Card: {type}, Placeable Indices: {string.Join(", ", validIndices)}");
            return validIndices;
        }

        /// <summary>
        /// 設置する場所をランダムで決める
        /// CPU強化ポイント
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private int RandomSelect(List<int> list)
        {
            int randomValue = Random.Range(0, list.Count);
            return list[randomValue];
        }

        public void LockCard(GameObject obj)
        {
            if (obj.GetComponent<Collider2D>() == null) return;
            
            Collider2D collider2D = obj.GetComponent<Collider2D>();
            if (collider2D != null) collider2D.enabled = false;
        }

        void TurnStart()
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

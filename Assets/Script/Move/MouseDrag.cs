using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cards;
using Stage;
using Systems;
using System.Linq;

namespace Move
{
    // カードの種類定義
    public enum CardType
    {
        Normal,
        Friend,
        OldMan,
        Family
    }

    public class MouseDrag : MonoBehaviour
    {
        [Header("ドロップ判定設定")]
        public string dropTargetsRootName = "ToiletParent";
        public float dropDistanceThreshold = 1.0f;

        [Header("カード能力設定")]
        public CardType cardType = CardType.Normal;

        [Tooltip("「友達」や「家族」タイプの場合、ここに相方のオブジェクトを登録してください")]
        public MouseDrag partnerCard;

        [Tooltip("【家族のみ】これは親オブジェクトですか？（True=親、False=子）")]
        public bool isFamilyParent = false;

        [Tooltip("このカードは、隣接ルールを無視して置けますか？（通常タイプ用）")]
        public bool canIgnoreNeighborRestriction = false;

        [Header("全体ルール設定")]
        public bool enableNeighborRestriction = true;

        // 内部変数
        private List<ToiletHighlight> targetHighlights = new List<ToiletHighlight>();
        private Vector3 initialPosition;
        private Vector3 dragOffset;
        private float zPosition;
        private new Collider2D collider2D = null;
        private bool isLocked = false;

        private ToiletHighlight currentSlot = null;
        public ToiletHighlight CurrentSlot => currentSlot;
        public bool IsPlaced => currentSlot != null;

        private void Awake()
        {
            collider2D = GetComponent<Collider2D>();
            initialPosition = transform.position;
        }

        private void Start()
        {
            if (StageManager.instance != null && StageManager.instance.toilet.Count > 0)
            {
                foreach (var obj in StageManager.instance.toilet)
                {
                    if (obj == null) continue;
                    var hl = obj.GetComponent<ToiletHighlight>();
                    if (hl != null) targetHighlights.Add(hl);
                }
            }
            else
            {
                GameObject root = GameObject.Find(dropTargetsRootName);
                if (root != null)
                {
                    foreach (Transform child in root.transform)
                    {
                        ToiletHighlight hl = child.GetComponent<ToiletHighlight>();
                        if (hl != null) targetHighlights.Add(hl);
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------
        // ★ ターン開始時の判定ロジック
        // --------------------------------------------------------------------------------
        public static void CheckGameOverAtStartOfTurn(bool globalNeighborRestriction, List<CardData> handCards)
        {
            if (StageManager.instance == null) return;
            var toilets = StageManager.instance.toilet;

            List<ToiletHighlight> slots = new List<ToiletHighlight>();
            foreach (var t in toilets)
            {
                if (t != null) slots.Add(t.GetComponent<ToiletHighlight>());
            }

            if (slots.Count == 0) return;

            bool canPlaceAnyCard = false;

            // 盤面に「相方候補（同種）」がいるかチェック
            Dictionary<CardType, bool> hasTypeOnBoard = new Dictionary<CardType, bool>();
            foreach (CardType ct in System.Enum.GetValues(typeof(CardType)))
            {
                bool exists = false;
                foreach (var slot in slots)
                {
                    if (slot.IsOccupied && slot.OccupyingObject != null && slot.OccupyingObject.cardType == ct)
                    {
                        exists = true;
                        break;
                    }
                }
                hasTypeOnBoard[ct] = exists;
            }

            // 手札にある各タイプの枚数をカウント
            Dictionary<CardType, int> handTypeCounts = new Dictionary<CardType, int>();
            foreach (var c in handCards)
            {
                if (!handTypeCounts.ContainsKey(c.Type)) handTypeCounts[c.Type] = 0;
                handTypeCounts[c.Type]++;
            }

            // Friendのペア配置シミュレーション関数
            System.Func<bool> canPlaceFriendPair = () =>
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].IsOccupied) continue;
                    // 1枚目: 両隣が空きでなければNG
                    if ((i > 0 && slots[i - 1].IsOccupied) || (i < slots.Count - 1 && slots[i + 1].IsOccupied)) continue;

                    // 2枚目があるか探す
                    for (int j = 0; j < slots.Count; j++)
                    {
                        if (i == j || slots[j].IsOccupied) continue;

                        // 2枚目: 隣が「空き」または「1枚目(i)」ならOK
                        bool lOk = true;
                        if (j > 0)
                        {
                            if (slots[j - 1].IsOccupied && (j - 1) != i) lOk = false;
                        }

                        bool rOk = true;
                        if (j < slots.Count - 1)
                        {
                            if (slots[j + 1].IsOccupied && (j + 1) != i) rOk = false;
                        }

                        if (lOk && rOk) return true; // ペア成立可能
                    }
                }
                return false;
            };

            // Family用シミュレーション（親子区別できないので両パターン試す）
            System.Func<bool, bool> canPlaceFamilyPair = (bool asParent) =>
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].IsOccupied) continue;

                    // 1枚目の条件
                    if (!asParent)
                    {
                        // 子として置くなら、両隣空き必須
                        if ((i > 0 && slots[i - 1].IsOccupied) || (i < slots.Count - 1 && slots[i + 1].IsOccupied)) continue;
                    }

                    // 2枚目（相方）を隣に置けるか
                    // 左隣(i-1)
                    if (i > 0 && !slots[i - 1].IsOccupied)
                    {
                        // 2枚目に対する条件
                        if (asParent)
                        {
                            // 2枚目は子。さらにその隣(i-2)が空き必要
                            if (i - 1 == 0 || !slots[i - 2].IsOccupied) return true;
                        }
                        else return true; // 2枚目は親なのでOK
                    }
                    // 右隣(i+1)
                    if (i < slots.Count - 1 && !slots[i + 1].IsOccupied)
                    {
                        if (asParent)
                        {
                            // 2枚目は子。さらにその隣(i+2)が空き必要
                            if (i + 1 == slots.Count - 1 || !slots[i + 2].IsOccupied) return true;
                        }
                        else return true;
                    }
                }
                return false;
            };

            // シミュレーション結果をキャッシュ
            bool friendPairResult = canPlaceFriendPair();
            bool familyPairResult = canPlaceFamilyPair(true) || canPlaceFamilyPair(false);

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsOccupied) continue;

                ToiletHighlight left = (i > 0) ? slots[i - 1] : null;
                ToiletHighlight right = (i < slots.Count - 1) ? slots[i + 1] : null;
                bool isLeftOccupied = left != null && left.IsOccupied;
                bool isRightOccupied = right != null && right.IsOccupied;
                bool hasOccupiedNeighbor = isLeftOccupied || isRightOccupied;

                foreach (var cardData in handCards)
                {
                    CardType type = cardData.Type;

                    if (globalNeighborRestriction)
                    {
                        // 「盤面に同種がいる」かつ「手札の同種が奇数枚」の場合、相方が配置済みとみなす
                        bool isPartnerLikelyOnBoard = hasTypeOnBoard.ContainsKey(type) && hasTypeOnBoard[type];
                        if (handTypeCounts.ContainsKey(type) && handTypeCounts[type] % 2 == 0)
                        {
                            isPartnerLikelyOnBoard = false;
                        }

                        switch (type)
                        {
                            case CardType.OldMan:
                                canPlaceAnyCard = true;
                                break;

                            case CardType.Family:
                                if (isPartnerLikelyOnBoard)
                                {
                                    // 2枚目：相方の隣ならOK
                                    bool adjPartner = (isLeftOccupied && IsSameType(left.OccupyingObject, type)) ||
                                                      (isRightOccupied && IsSameType(right.OccupyingObject, type));
                                    if (adjPartner) canPlaceAnyCard = true;
                                }
                                else
                                {
                                    // 1枚目：ペア配置可能か
                                    if (familyPairResult) canPlaceAnyCard = true;
                                }
                                break;

                            case CardType.Friend:
                                if (isPartnerLikelyOnBoard)
                                {
                                    // 2枚目：他人の隣NG (空き or 同種ならOK)
                                    bool leftOk = !isLeftOccupied || IsSameType(left.OccupyingObject, type);
                                    bool rightOk = !isRightOccupied || IsSameType(right.OccupyingObject, type);
                                    if (leftOk && rightOk) canPlaceAnyCard = true;
                                }
                                else
                                {
                                    // 1枚目：ペア配置可能か
                                    if (friendPairResult) canPlaceAnyCard = true;
                                }
                                break;

                            case CardType.Normal:
                            default:
                                if (!hasOccupiedNeighbor) canPlaceAnyCard = true;
                                break;
                        }
                    }
                    else
                    {
                        canPlaceAnyCard = true;
                    }

                    if (canPlaceAnyCard) break;
                }
                if (canPlaceAnyCard) break;
            }

            if (!canPlaceAnyCard)
            {
                Debug.Log("【GAME OVER】手詰まりです。");
                if (GameManager.instance != null) GameManager.instance.IsGameEnd = true;
            }
            else
            {
                Debug.Log("【Turn Start】置ける場所があります。");
            }
        }

        private static bool IsSameType(MouseDrag obj, CardType type)
        {
            if (obj == null) return false;
            return obj.cardType == type;
        }

        // --------------------------------------------------------------------------------
        // マウス操作イベント
        // --------------------------------------------------------------------------------

        private void OnMouseDown()
        {
            if (isLocked) return;
            if (currentSlot != null)
            {
                currentSlot.Vacate();
                currentSlot = null;
            }
            initialPosition = transform.position;
            zPosition = transform.position.z;
            dragOffset = transform.position - GetMouseWorldPosition();
        }

        private void OnMouseDrag()
        {
            if (isLocked) return;

            Vector3 newPosition = GetMouseWorldPosition() + dragOffset;
            newPosition.z = zPosition;
            transform.position = newPosition;

            for (int i = 0; i < targetHighlights.Count; i++)
            {
                ToiletHighlight target = targetHighlights[i];

                if (target.IsOccupied || CheckRestriction(i))
                {
                    target.Highlight(0);
                    continue;
                }

                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance <= dropDistanceThreshold) target.Highlight(2);
                else target.Highlight(1);
            }
        }

        private void OnMouseUp()
        {
            if (isLocked) return;

            ToiletHighlight successTarget = null;
            float minDistance = float.MaxValue;
            int successIndex = -1;

            for (int i = 0; i < targetHighlights.Count; i++)
            {
                ToiletHighlight target = targetHighlights[i];
                target.Highlight(0);

                if (target.IsOccupied || CheckRestriction(i)) continue;

                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance <= dropDistanceThreshold && distance < minDistance)
                {
                    minDistance = distance;
                    successTarget = target;
                    successIndex = i;
                }
            }

            if (successTarget != null)
            {
                Debug.Log("ドロップ成功");
                successTarget.SetOccupier(this);
                currentSlot = successTarget;
                transform.position = successTarget.transform.position;

                switch (cardType)
                {
                    case CardType.Friend:
                        if (partnerCard != null && partnerCard.IsPlaced)
                        {
                            Debug.Log("友達ペアが揃いました。ロックします");
                            this.LockCard();
                            partnerCard.LockCard();
                        }
                        break;

                    case CardType.Family:
                        if (partnerCard != null && partnerCard.IsPlaced && IsNextToPartner(successIndex))
                        {
                            Debug.Log("家族ペアが隣同士になりました。ロックします");
                            this.LockCard();
                            partnerCard.LockCard();
                        }
                        break;

                    case CardType.Normal:
                    case CardType.OldMan:
                    default:
                        LockCard();
                        break;
                }
            }
            else
            {
                Debug.Log("失敗：置けない場所です");
                transform.position = initialPosition;
            }
        }

        public void LockCard()
        {
            isLocked = true;
            if (collider2D != null) collider2D.enabled = false;
        }

        private bool IsNextToPartner(int myIndex)
        {
            if (partnerCard == null || !partnerCard.IsPlaced) return false;
            int partnerIndex = targetHighlights.IndexOf(partnerCard.CurrentSlot);
            if (partnerIndex == -1) return false;
            return Mathf.Abs(myIndex - partnerIndex) == 1;
        }

        /// <summary>
        /// その場所に置けるかどうかの判定（Trueなら制限にかかり置けない）
        /// </summary>
        private bool CheckRestriction(int index)
        {
            if (!enableNeighborRestriction) return false;

            ToiletHighlight left = (index > 0) ? targetHighlights[index - 1] : null;
            ToiletHighlight right = (index < targetHighlights.Count - 1) ? targetHighlights[index + 1] : null;
            bool isLeftOccupied = left != null && left.IsOccupied;
            bool isRightOccupied = right != null && right.IsOccupied;

            switch (cardType)
            {
                case CardType.Normal:
                    if (canIgnoreNeighborRestriction) return false;
                    if (isLeftOccupied || isRightOccupied) return true;
                    return false;

                case CardType.OldMan:
                    return false;

                case CardType.Friend:
                    // 1枚目（相方不在）の時、ここにおいた結果2枚目が置けなくなるならNG
                    if (partnerCard == null || !partnerCard.IsPlaced)
                    {
                        // 1. まずここは置けるか？（他人の隣はNG）
                        if (isLeftOccupied || isRightOccupied) return true;

                        // 2. ここを埋めたと仮定して、残りの場所で2枚目が置けるかシミュレーション
                        bool possibleToPlaceSecond = false;
                        for (int j = 0; j < targetHighlights.Count; j++)
                        {
                            if (index == j) continue;
                            if (targetHighlights[j].IsOccupied) continue;

                            // 2枚目の条件: 他人の隣NG (隣が空き or 自分(index)ならOK)
                            bool lOk = true;
                            if (j > 0)
                            {
                                // 左が埋まってる場合、それが自分(index)でなければNG（＝他人）
                                if (targetHighlights[j - 1].IsOccupied) lOk = false;
                                // 左が index (自分) ならOK
                                if ((j - 1) == index) lOk = true;
                            }

                            bool rOk = true;
                            if (j < targetHighlights.Count - 1)
                            {
                                // 右が埋まってる場合、それが自分(index)でなければNG（＝他人）
                                if (targetHighlights[j + 1].IsOccupied) rOk = false;
                                // 右が index (自分) ならOK
                                if ((j + 1) == index) rOk = true;
                            }

                            if (lOk && rOk)
                            {
                                possibleToPlaceSecond = true;
                                break;
                            }
                        }

                        // 2枚目が置けないなら、ここには置かせない
                        if (!possibleToPlaceSecond) return true;

                        return false;
                    }
                    else
                    {
                        // 2枚目（相方あり）
                        // 他人の隣には置けない（隣が空き、または相方ならOK）
                        if (isLeftOccupied && left.OccupyingObject != partnerCard) return true;
                        if (isRightOccupied && right.OccupyingObject != partnerCard) return true;
                        return false;
                    }

                case CardType.Family:
                    if (partnerCard == null || !partnerCard.IsPlaced)
                    {
                        // 1枚目（相方不在）
                        // 条件1: ここに置けるか
                        // 親なら他人隣OK、子なら他人隣NG
                        if (!isFamilyParent && (isLeftOccupied || isRightOccupied)) return true;

                        // 条件2: ここを埋めたと仮定して、隣に2枚目（相方）が置けるかシミュレーション
                        bool possibleToPlaceSecond = false;

                        // 左隣(index-1)に2枚目を置けるか？
                        if (index > 0 && !targetHighlights[index - 1].IsOccupied)
                        {
                            // 2枚目に対する判定
                            // 自分が親 -> 2枚目は子 -> 2枚目の左(index-2)が他人だとNG
                            // 自分が子 -> 2枚目は親 -> 制限なし（indexの隣だから）
                            if (isFamilyParent)
                            {
                                // index-2 が他人だとNG
                                if (index - 1 == 0 || !targetHighlights[index - 2].IsOccupied) possibleToPlaceSecond = true;
                            }
                            else possibleToPlaceSecond = true; // 2枚目は親なのでOK
                        }

                        // 右隣(index+1)に2枚目を置けるか？
                        if (!possibleToPlaceSecond && index < targetHighlights.Count - 1 && !targetHighlights[index + 1].IsOccupied)
                        {
                            if (isFamilyParent)
                            {
                                // index+2 が他人だとNG
                                if (index + 1 == targetHighlights.Count - 1 || !targetHighlights[index + 2].IsOccupied) possibleToPlaceSecond = true;
                            }
                            else possibleToPlaceSecond = true;
                        }

                        if (!possibleToPlaceSecond) return true;
                        return false;
                    }
                    else
                    {
                        // 2枚目（相方あり）
                        int partnerIndex = targetHighlights.IndexOf(partnerCard.CurrentSlot);
                        if (partnerIndex != -1 && Mathf.Abs(index - partnerIndex) != 1) return true;

                        if (!isFamilyParent)
                        {
                            if (isLeftOccupied && left.OccupyingObject != partnerCard) return true;
                            if (isRightOccupied && right.OccupyingObject != partnerCard) return true;
                        }
                        return false;
                    }

                default:
                    return false;
            }
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = Camera.main.nearClipPlane + 10f;
            return Camera.main.ScreenToWorldPoint(mousePoint);
        }
    }
}
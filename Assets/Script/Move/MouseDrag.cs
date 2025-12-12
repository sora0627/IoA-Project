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

            foreach (var cardData in handCards)
            {
                CardType type = cardData.Type;
                bool canPlaceThisCard = false;

                if (globalNeighborRestriction)
                {
                    switch (type)
                    {
                        case CardType.OldMan:
                            // どこか1つでも空いていればOK
                            if (slots.Any(s => !s.IsOccupied)) canPlaceThisCard = true;
                            break;

                        case CardType.Normal:
                            // 他人の隣でない場所があるか
                            if (CheckNormalAvailability(slots)) canPlaceThisCard = true;
                            break;

                        case CardType.Friend:
                            // 2体配置可能か
                            if (CheckFriendPairAvailability(slots)) canPlaceThisCard = true;
                            break;

                        case CardType.Family:
                            // 2体配置可能か
                            if (CheckFamilyPairAvailability(slots)) canPlaceThisCard = true;
                            break;
                    }
                }
                else
                {
                    // 制限なしなら空きがあればOK
                    if (slots.Any(s => !s.IsOccupied)) canPlaceThisCard = true;
                }

                if (canPlaceThisCard)
                {
                    canPlaceAnyCard = true;
                    break;
                }
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

        // --- 判定用ヘルパー (PlayerManagerと同じロジック) ---

        private static bool CheckNormalAvailability(List<ToiletHighlight> slots)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsOccupied) continue;
                bool leftOk = (i == 0) || !slots[i - 1].IsOccupied;
                bool rightOk = (i == slots.Count - 1) || !slots[i + 1].IsOccupied;
                if (leftOk && rightOk) return true;
            }
            return false;
        }

        private static bool CheckFriendPairAvailability(List<ToiletHighlight> slots)
        {
            List<int> emptyIndices = new List<int>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsOccupied) emptyIndices.Add(i);
            }
            if (emptyIndices.Count < 2) return false;

            for (int i = 0; i < emptyIndices.Count; i++)
            {
                for (int j = i + 1; j < emptyIndices.Count; j++)
                {
                    int idxA = emptyIndices[i];
                    int idxB = emptyIndices[j];
                    if (IsSafeForFriend(slots, idxA, idxB) && IsSafeForFriend(slots, idxB, idxA)) return true;
                }
            }
            return false;
        }

        private static bool IsSafeForFriend(List<ToiletHighlight> slots, int targetIdx, int partnerIdx)
        {
            // 左隣チェック
            if (targetIdx > 0)
            {
                // 左が埋まっていて、かつそれが相方(partnerIdx)でなければNG（＝他人）
                if (slots[targetIdx - 1].IsOccupied) return false;
                // ※slots[targetIdx-1].IsOccupied が false なら、空きか partnerIdx(まだ埋まってない) なのでOK
            }
            // 右隣チェック
            if (targetIdx < slots.Count - 1)
            {
                if (slots[targetIdx + 1].IsOccupied) return false;
            }
            return true;
        }

        private static bool CheckFamilyPairAvailability(List<ToiletHighlight> slots)
        {
            for (int i = 0; i < slots.Count - 1; i++)
            {
                if (!slots[i].IsOccupied && !slots[i + 1].IsOccupied)
                {
                    // パターン1: [子][親] (i=子) -> 子の左が空きであること
                    bool p1 = (i == 0) || !slots[i - 1].IsOccupied;
                    // パターン2: [親][子] (i+1=子) -> 子の右が空きであること
                    bool p2 = (i + 1 == slots.Count - 1) || !slots[i + 2].IsOccupied;

                    if (p1 || p2) return true;
                }
            }
            return false;
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
                    // 他人の隣はNG
                    if (isLeftOccupied || isRightOccupied) return true;
                    return false;

                case CardType.OldMan:
                    return false;

                case CardType.Friend:
                    if (partnerCard == null || !partnerCard.IsPlaced)
                    {
                        // --- 1体目（相方未配置） ---

                        // 1. ここ(index)自体が置けるか？（他人の隣はNG）
                        if (isLeftOccupied || isRightOccupied) return true;

                        // 2. ここを埋めたと仮定して、相方(2体目)を置ける場所が残るか？
                        bool possibleToPlacePartner = false;
                        for (int j = 0; j < targetHighlights.Count; j++)
                        {
                            if (index == j) continue;
                            if (targetHighlights[j].IsOccupied) continue;

                            // 2体目の条件: 他人の隣NG (隣が空き or 1体目(index)ならOK)
                            bool lOk = true;
                            if (j > 0)
                            {
                                // 左が埋まっていて、かつ 1体目(index) でなければNG
                                if (targetHighlights[j - 1].IsOccupied && (j - 1) != index) lOk = false;
                            }

                            bool rOk = true;
                            if (j < targetHighlights.Count - 1)
                            {
                                // 右が埋まっていて、かつ 1体目(index) でなければNG
                                if (targetHighlights[j + 1].IsOccupied && (j + 1) != index) rOk = false;
                            }

                            if (lOk && rOk)
                            {
                                possibleToPlacePartner = true;
                                break;
                            }
                        }

                        // 相方を置ける場所がないなら、ここはNG
                        if (!possibleToPlacePartner) return true;

                        return false;
                    }
                    else
                    {
                        // --- 2体目（相方配置済み） ---
                        // 他人の隣には置けない（隣が空き、または相方ならOK）
                        if (isLeftOccupied && left.OccupyingObject != partnerCard) return true;
                        if (isRightOccupied && right.OccupyingObject != partnerCard) return true;
                        return false;
                    }

                case CardType.Family:
                    if (partnerCard == null || !partnerCard.IsPlaced)
                    {
                        // --- 1体目（相方未配置） ---

                        // 1. ここ(index)に相方（2体目）を隣接させて置けるか？
                        bool canPlaceLeft = false;
                        bool canPlaceRight = false;

                        // 左隣(index-1)に相方を置けるか？
                        if (index > 0 && !targetHighlights[index - 1].IsOccupied)
                        {
                            // 並び順: [相方][自分(index)]
                            // 自分(index)の右隣チェック
                            bool selfOk = isFamilyParent || (index == targetHighlights.Count - 1 || !targetHighlights[index + 1].IsOccupied);

                            // 相方(index-1)の左隣チェック
                            bool partnerOk = !isFamilyParent || (index - 1 == 0 || !targetHighlights[index - 2].IsOccupied);
                            // ※ !isFamilyParent(自分は子) -> 相方は親 -> 相方の左は誰でもOK -> partnerOk=true
                            // ※ isFamilyParent(自分は親) -> 相方は子 -> 相方の左は空き必須

                            if (selfOk && partnerOk) canPlaceLeft = true;
                        }

                        // 右隣(index+1)に相方を置けるか？
                        if (index < targetHighlights.Count - 1 && !targetHighlights[index + 1].IsOccupied)
                        {
                            // 並び順: [自分(index)][相方]
                            // 自分(index)の左隣チェック
                            bool selfOk = isFamilyParent || (index == 0 || !targetHighlights[index - 1].IsOccupied);

                            // 相方(index+1)の右隣チェック
                            bool partnerOk = !isFamilyParent || (index + 1 == targetHighlights.Count - 1 || !targetHighlights[index + 2].IsOccupied);

                            if (selfOk && partnerOk) canPlaceRight = true;
                        }

                        // 左右どちらにもペアを作れないならNG
                        if (!canPlaceLeft && !canPlaceRight) return true;
                        return false;
                    }
                    else
                    {
                        // --- 2体目（相方配置済み） ---

                        // 吸着ルール: 相方の隣でなければNG
                        int partnerIndex = targetHighlights.IndexOf(partnerCard.CurrentSlot);
                        if (partnerIndex != -1 && Mathf.Abs(index - partnerIndex) != 1) return true;

                        // 親子のルール:
                        // 自分が「子」なら、他人の隣はNG（相方の隣はOKだが、反対側が他人だとNG）
                        if (!isFamilyParent)
                        {
                            // 左チェック: 埋まってるなら相方でなければNG
                            if (isLeftOccupied && left.OccupyingObject != partnerCard) return true;
                            // 右チェック: 埋まってるなら相方でなければNG
                            if (isRightOccupied && right.OccupyingObject != partnerCard) return true;
                        }
                        // 自分が「親」なら、相方の隣であれば反対側が他人でもOK（制限なし）

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cards;
using Stage;
using Systems;

namespace Move
{
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
                        switch (type)
                        {
                            case CardType.OldMan:
                                canPlaceAnyCard = true;
                                break;

                            case CardType.Family:
                                canPlaceAnyCard = true;
                                break;

                            case CardType.Friend:
                                if (!hasOccupiedNeighbor) canPlaceAnyCard = true;
                                else
                                {
                                    if (isLeftOccupied && IsSameType(left.OccupyingObject, type)) canPlaceAnyCard = true;
                                    if (isRightOccupied && IsSameType(right.OccupyingObject, type)) canPlaceAnyCard = true;
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

            if (!canPlaceAnyCard) GameManager.instance.IsGameEnd = true;
            else Debug.Log("【Turn Start】置ける場所があります。");
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

                // --- ロック（配置確定）判定 ---
                switch (cardType)
                {
                    case CardType.Friend:
                        // 友達：隣でなくてもよい。両方が配置されたらロックする。
                        if (partnerCard != null && partnerCard.IsPlaced)
                        {
                            Debug.Log("友達ペアが揃いました（位置不問）。ロックします");
                            this.LockCard();
                            partnerCard.LockCard();
                        }
                        else
                        {
                            Debug.Log("相方がまだ配置されていません。再移動可能です");
                        }
                        break;

                    case CardType.Family:
                        // 家族：隣同士でなければならない（吸着ルールがあるため置けた時点で隣のはずだが、完了条件としてチェック）
                        if (partnerCard != null && partnerCard.IsPlaced && IsNextToPartner(successIndex))
                        {
                            Debug.Log("家族ペアが隣同士になりました。ロックします");
                            this.LockCard();
                            partnerCard.LockCard();
                        }
                        else
                        {
                            Debug.Log("相方がいない、または正規の位置ではありません。");
                        }
                        break;

                    case CardType.Normal:
                    case CardType.OldMan:
                    default:
                        // それ以外は置いた時点で確定
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

        // 指定したインデックスが相方の隣かどうか
        private bool IsNextToPartner(int myIndex)
        {
            if (partnerCard == null || !partnerCard.IsPlaced) return false;

            // 相方の場所を探す
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
                    // 相方が置かれていない場合
                    if (partnerCard == null || !partnerCard.IsPlaced)
                    {
                        // 通常通り隣が空いていればOK
                        if (isLeftOccupied || isRightOccupied) return true;
                        return false;
                    }
                    else
                    {
                        // 相方が置かれている場合
                        // 他人の隣には置けない（相方か、空き地ならOK）

                        if (isLeftOccupied)
                        {
                            if (left.OccupyingObject != partnerCard) return true; // 相方以外の隣はNG
                        }
                        if (isRightOccupied)
                        {
                            if (right.OccupyingObject != partnerCard) return true; // 相方以外の隣はNG
                        }

                        // 両隣が空き、または相方ならOK（離れていてもOK）
                        return false;
                    }

                case CardType.Family:
                    // 1. 吸着ルール: 相方がいるならその隣にしか置けない
                    if (partnerCard != null && partnerCard.IsPlaced)
                    {
                        int partnerIndex = targetHighlights.IndexOf(partnerCard.CurrentSlot);
                        if (partnerIndex != -1)
                        {
                            if (Mathf.Abs(index - partnerIndex) != 1) return true;
                        }
                    }

                    // 2. 親子の隣接ルール
                    if (isFamilyParent)
                    {
                        // 親：他人の隣でもOK
                        return false;
                    }
                    else
                    {
                        // 子：相方（親）以外の隣はNG
                        if (isLeftOccupied && left.OccupyingObject != partnerCard) return true;
                        if (isRightOccupied && right.OccupyingObject != partnerCard) return true;
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
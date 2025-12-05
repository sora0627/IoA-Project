using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cards; // CardDataを利用するため
using Stage; // StageManagerを利用するため

namespace Move
{
    // カードの種類定義（CardData.csで使用）
    public enum CardType
    {
        Normal, // 通常（隣が空いていれば置ける）
        Friend, // 友達（相方が隣に来るまで動かせる、他人の隣はNG）
        OldMan, // 老人（隣に誰かいても置ける）
        Family  // 家族（親子でルールが異なる。親は他人の隣OK、子はNG。隣同士でロック）
    }

    /// <summary>
    /// キャラクターをドラッグ＆ドロップで移動させ、配置ルールを管理するクラス
    /// </summary>
    public class MouseDrag : MonoBehaviour
    {
        [Header("ドロップ判定設定")]
        [Tooltip("ドロップターゲットの親オブジェクトの名前")]
        public string dropTargetsRootName = "ToiletParent";
        [Tooltip("ドロップターゲットからの許容誤差")]
        public float dropDistanceThreshold = 1.0f;

        [Header("カード能力設定")]
        [Tooltip("カードの種類")]
        public CardType cardType = CardType.Normal;

        [Tooltip("「友達」や「家族」タイプの場合、ここに相方のオブジェクトを登録してください")]
        public MouseDrag partnerCard;

        [Tooltip("【家族のみ】これは親オブジェクトですか？（True=親、False=子）")]
        public bool isFamilyParent = false;

        [Tooltip("このカードは、隣接ルールを無視して置けますか？（通常タイプ用）")]
        public bool canIgnoreNeighborRestriction = false;

        [Header("全体ルール設定")]
        [Tooltip("ゲーム全体として隣接制限ルールが有効かどうか")]
        public bool enableNeighborRestriction = true;

        // 内部変数
        private List<ToiletHighlight> targetHighlights = new List<ToiletHighlight>();
        private Vector3 initialPosition;
        private Vector3 dragOffset;
        private float zPosition;
        private new Collider2D collider2D = null;
        private bool isLocked = false; // 完全に配置確定して動かせなくなったか

        // 現在自分が置かれているスロット
        private ToiletHighlight currentSlot = null;
        public ToiletHighlight CurrentSlot => currentSlot;

        // 外部から「配置済みか」を確認するためのプロパティ
        public bool IsPlaced => currentSlot != null;

        private void Awake()
        {
            collider2D = GetComponent<Collider2D>();
            initialPosition = transform.position;
        }

        private void Start()
        {
            // StageManagerが存在する場合、そこからトイレリストを取得
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
                // フォールバック
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

            // スロットリスト作成
            List<ToiletHighlight> slots = new List<ToiletHighlight>();
            foreach (var t in toilets)
            {
                if (t == null) continue;
                var hl = t.GetComponent<ToiletHighlight>();
                if (hl != null) slots.Add(hl);
            }

            if (slots.Count == 0) return;

            bool canPlaceAnyCard = false;

            // 各スロットについて
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsOccupied) continue; // 埋まってたら無理

                ToiletHighlight left = (i > 0) ? slots[i - 1] : null;
                ToiletHighlight right = (i < slots.Count - 1) ? slots[i + 1] : null;

                bool isLeftOccupied = left != null && left.IsOccupied;
                bool isRightOccupied = right != null && right.IsOccupied;
                bool hasOccupiedNeighbor = isLeftOccupied || isRightOccupied;

                // 手札チェック
                foreach (var cardData in handCards)
                {
                    CardType type = cardData.Type;

                    if (globalNeighborRestriction)
                    {
                        switch (type)
                        {
                            case CardType.Friend:
                                // Friend: 隣が空き、または相方ならOK
                                // 手札時点では相方インスタンス不明のため、「他人がいたらNG」と判定
                                if (hasOccupiedNeighbor)
                                {
                                    bool leftIsStranger = isLeftOccupied && !IsSameType(left.OccupyingObject, type);
                                    bool rightIsStranger = isRightOccupied && !IsSameType(right.OccupyingObject, type);

                                    // 両隣が他人でなければ（同種族がいれば）置ける可能性がある
                                    if (!leftIsStranger && !rightIsStranger) canPlaceAnyCard = true;
                                }
                                else
                                {
                                    canPlaceAnyCard = true;
                                }
                                break;

                            case CardType.Family:
                                // Family: 親ならどこでも置ける、子なら他人NG
                                // ※CardDataに「親か子か」の情報がない場合、安全策として「置ける可能性がある」とみなすか
                                // 厳密にチェックする必要があります。ここでは「親なら置ける」ため、Familyがあればほぼ置けると判定します。
                                canPlaceAnyCard = true;
                                break;

                            case CardType.OldMan:
                                // OldMan: 隣に誰がいても置ける
                                canPlaceAnyCard = true;
                                break;

                            case CardType.Normal:
                            default:
                                // Normal: 隣が空いていればOK
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
                Debug.Log("【GAME OVER】手詰まりです。");
            else
                Debug.Log("【Turn Start】置ける場所があります。");
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
                // if(Player.PlayerManager.instance != null) Player.PlayerManager.instance.isSet = false;
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

            // ハイライト更新
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
                // Friend/Family共通: 相方が配置済み、かつ「隣同士」ならロック
                if (cardType == CardType.Friend || cardType == CardType.Family)
                {
                    if (partnerCard != null && partnerCard.IsPlaced && IsNextToPartner())
                    {
                        Debug.Log("ペアが隣同士になりました！ロックします");
                        this.LockCard();
                        partnerCard.LockCard();
                    }
                    else
                    {
                        Debug.Log("相方がいない、または隣ではありません。再移動可能です");
                    }
                }
                else
                {
                    // Normal, OldManは即座にロック
                    LockCard();
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

        // 相方が隣のスロットにいるか判定
        private bool IsNextToPartner()
        {
            if (partnerCard == null || partnerCard.CurrentSlot == null || this.CurrentSlot == null) return false;

            // スロットのインデックスなどを比較するのが確実ですが、
            // ここでは簡易的に距離で判定します（スロット間隔+遊び）
            float dist = Vector3.Distance(transform.position, partnerCard.transform.position);
            // ※間隔が1.5fの場合、2.0f未満なら隣とみなす等
            return dist < 2.0f;
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

            // --- カードタイプ別の制限ルール ---
            switch (cardType)
            {
                case CardType.Normal:
                    // 隣にオブジェクトが存在しないなら置ける
                    // => 逆に言うと、どちらかが埋まっていたらNG
                    if (canIgnoreNeighborRestriction) return false;
                    if (isLeftOccupied || isRightOccupied) return true;
                    return false;

                case CardType.OldMan:
                    // 隣にオブジェクトが存在しても置くことができる
                    // => 制限なし
                    return false;

                case CardType.Friend:
                    // 隣にオブジェクトが存在しないなら置ける (A)
                    // 同じペア同士なら置ける (B)
                    // 同じFriendでもペアでなければ隣には置けない (C)

                    // 左チェック
                    if (isLeftOccupied)
                    {
                        // 埋まっているのが相方でなければNG
                        if (left.OccupyingObject != partnerCard) return true;
                    }
                    // 右チェック
                    if (isRightOccupied)
                    {
                        // 埋まっているのが相方でなければNG
                        if (right.OccupyingObject != partnerCard) return true;
                    }
                    return false;

                case CardType.Family:
                    // 親: 親オブジェクトのもう片方の隣にオブジェクトが存在しても置くことができる
                    //     => 親は「他人の隣」でもOK。つまり制限なし（OldManと同じ配置能力）
                    if (isFamilyParent)
                    {
                        return false;
                    }
                    // 子: 子オブジェクトの隣にオブジェクトが存在してはいけない
                    //     => ただし「ペアとなるオブジェクト（親）」は例外的にOKであるはず（そうしないと並べない）
                    else
                    {
                        // 左チェック
                        if (isLeftOccupied)
                        {
                            // 親以外（他人）ならNG
                            if (left.OccupyingObject != partnerCard) return true;
                        }
                        // 右チェック
                        if (isRightOccupied)
                        {
                            // 親以外（他人）ならNG
                            if (right.OccupyingObject != partnerCard) return true;
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
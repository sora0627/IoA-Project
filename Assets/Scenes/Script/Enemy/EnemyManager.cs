using Cards;
using Stage;
using System.Collections;
using System.Collections.Generic;
using Systems;
using UnityEngine;
using Move;

namespace Enemy
{
    /// <summary>
    /// 敵のターンの進行や状態管理を担当するクラス
    /// AIの思考部分は EnemyAI に委譲し、ここではゲームの流れを制御します。
    /// </summary>
    public class EnemyManager : Singleton<EnemyManager>
    {
        public string dropTargetsRootName = "ToiletParent";

        [SerializeField] public List<CardData> hands;
        [SerializeField] private Transform parent;

        [Header("AI 思考時間設定（秒）")]
        [SerializeField] private float waitBeforeThinking = 1.0f; // ターン開始時の待機時間
        [SerializeField] private float waitAfterCardSelect = 1.5f; // カード決定中の待機時間
        [SerializeField] private float waitAfterPlacement = 1.0f; // 配置完了からターン終了までの待機時間

        private List<ToiletHighlight> targetHighlights = new List<ToiletHighlight>();
        private bool isDraw = false;
        private bool isThinking = false; // 二重実行防止フラグ
        private CardType currentSelectCardType;

        // ★追加：前回のフレームでのターン状態を記録する変数
        private bool wasEnemyTurn = false;

        // AI思考モジュール
        private EnemyAI enemyAI;

        public CardType CurrentSelectCard
        {
            get { return currentSelectCardType; }
            set { currentSelectCardType = value; }
        }

        protected override void Awake()
        {
            base.Awake();
            enemyAI = new EnemyAI(); // AIモジュールの初期化
        }

        void Update()
        {
            bool isEnemyTurn = !GameManager.instance.IsPlayerTurn;

            // ★相手ターンから自分のターンに切り替わった瞬間を検知し、状態を確実にリセットする
            if (isEnemyTurn && !wasEnemyTurn)
            {
                Initialization();
            }
            wasEnemyTurn = isEnemyTurn;


            if (GameManager.instance.IsPlayerTurn) return;

            // ★ターン開始アニメーション中は行動（思考）を開始しないようにブロックする
            if (UI.TurnUIController.instance != null && UI.TurnUIController.instance.IsAnimating) return;

            // 敵の選択ターンが来て、かつまだ思考中でなければコルーチンを開始
            if (GameManager.instance.IsSelect && !isThinking)
            {
                StartCoroutine(EnemyTurnCoroutine());
            }

            // 万が一の予備として残しておく
            if (GameManager.instance.IsTrueEnd)
            {
                TurnEnd();
            }
        }

        /// <summary>
        /// 敵のターン進行をコントロールするコルーチン（ここで遅延を発生させます）
        /// </summary>
        private IEnumerator EnemyTurnCoroutine()
        {
            isThinking = true;

            TurnStart();

            // 1. ターン開始時、プレイヤーが状況を把握するための間
            yield return new WaitForSeconds(waitBeforeThinking);

            // 2. AIが使用するカードを選択
            int cardIndex = enemyAI.SelectCard(hands, targetHighlights, out currentSelectCardType);

            // 3. AIが「考えている」ことを演出するための間
            yield return new WaitForSeconds(waitAfterCardSelect);

            if (cardIndex != -1)
            {
                // 4. カードを使用し、盤面に配置
                UseHand(cardIndex);

                // 5. 配置された結果をプレイヤーが見るための間
                yield return new WaitForSeconds(waitAfterPlacement);

                GameManager.instance.IsTrueEnd = true;
            }
            else
            {
                // 置けるカードがない場合（手詰まり）
                GameManager.instance.IsGameEnd = true;
            }

            isThinking = false;
        }

        public void Initialization()
        {
            isDraw = false;
            isThinking = false;
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

        public void UseHand(int index)
        {
            // キャラクターを生成（この中で SetHuman が呼ばれる想定）
            StageManager.instance.CharacterGeneration(hands[index]);
            hands.RemoveAt(index);
        }

        /// <summary>
        /// 生成されたキャラクターオブジェクトを盤面にセットします。
        /// </summary>
        public void SetHuman(GameObject cloneObj, GameObject cloneObj1)
        {
            if (cloneObj == null) return;

            // 1体目の配置場所をAIに決定させる
            MouseDrag myDrag = cloneObj.GetComponent<MouseDrag>();
            int firstIndex = enemyAI.DetermineFirstPlacement(myDrag, targetHighlights);

            if (firstIndex != -1)
            {
                PlaceCharacterAt(myDrag, firstIndex);
            }

            targetHighlights[firstIndex].SetOccupier(myDrag);
            myDrag.currentSlot = targetHighlights[firstIndex];
            cloneObj.transform.position = targetHighlights[firstIndex].gameObject.transform.position;
            LockCard(cloneObj);
            // 2体目の配置（ある場合）
            if (cloneObj1 != null && firstIndex != -1)
            {
                MouseDrag myDrag1 = cloneObj1.GetComponent<MouseDrag>();
                int secondIndex = enemyAI.DetermineSecondPlacement(myDrag1, firstIndex, targetHighlights);

                if (secondIndex != -1)
                {
                    PlaceCharacterAt(myDrag1, secondIndex);
                }
            }
        }

        /// <summary>
        /// 指定したインデックスのスロットにキャラクターを配置・ロックします。
        /// </summary>
        private void PlaceCharacterAt(MouseDrag drag, int index)
        {
            targetHighlights[index].SetOccupier(drag);
            drag.currentSlot = targetHighlights[index];
            drag.gameObject.transform.position = targetHighlights[index].gameObject.transform.position;
            LockCard(drag.gameObject);
        }

        public void LockCard(GameObject obj)
        {
            if (obj == null) return;
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
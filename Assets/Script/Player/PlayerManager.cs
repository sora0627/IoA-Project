using Cards;
using System.Collections;
using System.Collections.Generic;
using Systems;
using UnityEngine;
using Stage;
using Move;

namespace Player
{
    /// <summary>
    /// プレイヤーのターン進行・状態・手札などのUI管理を担当するクラス。
    /// </summary>
    public class PlayerManager : Singleton<PlayerManager>
    {
        [SerializeField]
        public List<CardData> hands;

        [SerializeField] public Transform parent;
        [SerializeField] private List<Transform> HandPos;

        private GameObject currentSelectCard;
        private bool isDraw = false;
        private bool isGeneration = false;

        // 配置判定ロジックを担当するモジュール
        private PlayerPlacementValidator placementValidator = new PlayerPlacementValidator();

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
                TurnStart();
            }

            if (GameManager.instance.IsSet)
            {
                parent.gameObject.SetActive(false);

                if (!isGeneration)
                {
                    isGeneration = true;
                    CardData cardData = SelectCard.GetComponent<CardData>();
                    StageManager.instance.CharacterGeneration(cardData);
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
            isGeneration = false;
        }

        public void SetCard()
        {
            for (int index = 0; index < hands.Count; index++)
            {
                CardData cardData = hands[index];
                GameObject card = cardData.gameObject;
                card.transform.position = HandPos[index].position;
                card.transform.parent = parent;
            }
        }

        public void UseHand(int index)
        {
            if (index < 0 || index >= hands.Count) return;

            CardData cardToUse = hands[index];

            // 盤面の取得
            var toilets = StageManager.instance.toilet;
            List<ToiletHighlight> slots = new List<ToiletHighlight>();
            foreach (var t in toilets)
            {
                if (t != null) slots.Add(t.GetComponent<ToiletHighlight>());
            }

            // 現在の盤面で配置可能か、Validatorを使ってチェック
            if (!placementValidator.CanPlaceCard(cardToUse, slots))
            {
                Debug.Log($"【配置不可】{cardToUse.Type}: 配置できる場所がありません。");
                return;
            }

            Destroy(currentSelectCard);
            hands.RemoveAt(index);
            SetCard();
            GameManager.instance.IsSet = true;
        }

        void TurnStart()
        {
            if (!isDraw)
            {
                isDraw = true;
                CardManager.instance.DrawCard(hands);
                SetCard();

                MouseDrag.CheckGameOverAtStartOfTurn(true, hands);
            }
        }

        void TurnEnd()
        {
            isDraw = false;
            isGeneration = false;
        }
    }
}
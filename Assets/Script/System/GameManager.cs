using Stage;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Purchasing;
using UnityEngine;
using System;

namespace Systems
{
    public class GameManager : Singleton<GameManager>
    {
        enum State
        {
            Ready,
            OnGame,
            Select,
            Set,
            TrueEnd,
            Pause,
            GameEnd,
        }

        private State state = State.Ready;
        private State previousState = State.Ready; // ポーズ前の状態を保持する変数

        private bool isPlayerTurn;
        private bool isGameOver = false;

        public bool IsReady
        {
            get { return state == State.Ready; }
            set { if (value) state = State.Ready; }
        }

        public bool IsOnGame
        {
            get { return state == State.OnGame; }
        }

        public bool IsSelect
        {
            get { return state == State.Select; }
            set { if (value) state = State.Select; }
        }

        public bool IsSet
        {
            get { return state == State.Set; }
            set { if (value) state = State.Set; }
        }

        public bool IsPause
        {
            get { return state == State.Pause; }
        }

        public bool IsTrueEnd
        {
            get { return state == State.TrueEnd; }
            set { if (value) state = State.TrueEnd; }
        }

        public bool IsGameEnd
        {
            get { return state == State.GameEnd; }
            set { if (value) state = State.GameEnd; }
        }

        public bool IsPlayerTurn
        {
            get { return isPlayerTurn; }
            set { isPlayerTurn = value; }
        }

        // Update is called once per frame
        void Update()
        {
            // リセット処理（ポーズ中もやり直せるように先頭に配置）
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (IsPause) ResumeGame(); // ポーズ中にリセットした場合は時間停止を解除
                ResetGame();
            }

            // ESCキーでポーズ画面の切り替え
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }

            // ★ポーズ中はこれ以降のゲーム進行（状態遷移）を行わない
            if (IsPause) return;

            if (IsReady)
            {
                Initialization();
            }

            if (IsOnGame)
            {
                IsSelect = true;
            }

            if (IsTrueEnd)
            {
                TurnChange();
            }

            if (IsGameEnd)
            {
                GameOver();
            }
        }

        /// <summary>
        /// ポーズの切り替えを行います
        /// </summary>
        public void TogglePause()
        {
            // ★通常のゲーム中以外（準備中やゲームオーバー後）はポーズの切り替えを行わない
            if (state == State.Ready || state == State.GameEnd || isGameOver) return;

            if (IsPause)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        /// <summary>
        /// ゲームを一時停止します
        /// </summary>
        private void PauseGame()
        {
            previousState = state; // 現在の進行状況（SelectやSet等）を記憶
            state = State.Pause;

            // ゲーム内時間を停止（コルーチンの待機やアニメーションが止まります）
            Time.timeScale = 0f;

            Debug.Log("【Pause】ゲームを一時停止しました");

            // TODO: UIManagerなどでポーズ画面（UI）を表示する処理をここに記述します
            // 例: UIManager.instance.ShowPauseMenu();
        }

        /// <summary>
        /// ゲームを再開します
        /// </summary>
        public void ResumeGame()
        {
            state = previousState; // 記憶していた進行状況を復元

            // ゲーム内時間を再開
            Time.timeScale = 1f;

            Debug.Log("【Resume】ゲームを再開しました");

            // TODO: UIManagerなどでポーズ画面（UI）を非表示にする処理をここに記述します
            // 例: UIManager.instance.HidePauseMenu();
        }

        void Initialization()
        {
            state = State.Ready;
            isGameOver = false;

            // 初期化時に時間の進み方を確実に戻す
            Time.timeScale = 1f;

            Player.PlayerManager.instance.Initialization();
            Enemy.EnemyManager.instance.Initialization();

            Player.PlayerManager.instance.hands.Clear();
            Enemy.EnemyManager.instance.hands.Clear();
            DestroyChildAll(Player.PlayerManager.instance.parent);
            DestroyChildAll(StageManager.instance.parent);

            foreach (var toilet in StageManager.instance.toilet)
            {
                ToiletHighlight highlight = toilet.GetComponent<ToiletHighlight>();
                highlight.Vacate();
            }

            Cards.CardManager.instance.SetDeck();
            Cards.CardManager.instance.ShuffleDeck();
            Cards.CardManager.instance.FirstDraw(Player.PlayerManager.instance.hands);
            Cards.CardManager.instance.FirstDraw(Enemy.EnemyManager.instance.hands);

            Player.PlayerManager.instance.SetCard();
            StartSetTurn();

            state = State.OnGame;
        }

        void StartSetTurn()
        {
            int rand = UnityEngine.Random.Range(0, 2);
            isPlayerTurn = (rand == 0) ? true : false;
            Debug.Log((isPlayerTurn) ? ("先行") : ("後攻"));
            if (IsPlayerTurn) UI.TurnUIController.instance.ShowPlayerTurn();
            else UI.TurnUIController.instance.ShowEnemyTurn();
        }

        void TurnChange()
        {
            IsPlayerTurn = !IsPlayerTurn;
            Debug.Log((isPlayerTurn) ? ("PlayerTurn") : ("EnemyTurn"));
            StageManager.instance.ReduseCheckoutTime();
            if (IsPlayerTurn) UI.TurnUIController.instance.ShowPlayerTurn();
            else UI.TurnUIController.instance.ShowEnemyTurn();
            IsSelect = true;
        }

        private void GameOver()
        {
            if (!isGameOver)
            {
                Debug.Log("【GAME OVER】手詰まりです。");
                if (IsPlayerTurn)
                {
                    Debug.Log("You Lose");
                }
                else
                {
                    Debug.Log("You Win");
                }
                isGameOver = true;
            }
        }

        private void ResetGame()
        {
            Debug.Log("Restart");
            IsReady = true;
        }

        // rootの子オブジェクトをすべてDestroyする
        private void DestroyChildAll(Transform root)
        {
            //自分の子供を全て調べる
            foreach (Transform child in root)
            {
                //自分の子供をDestroyする
                Destroy(child.gameObject);
            }
        }
    }
}
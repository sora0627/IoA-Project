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
            set { if(value) state = State.GameEnd; }
        }

        public bool IsPlayerTurn
        {
            get { return isPlayerTurn; }
            set { isPlayerTurn = value; }
        }

        // Update is called once per frame
        void Update()
        {
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

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ResetGame();
            }
        }

        void Initialization()
        {
            state = State.Ready;

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

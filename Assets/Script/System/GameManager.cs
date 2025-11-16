using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System
{
    public class GameManager : Singleton<GameManager>
    {
        enum State
        {
            Ready,
            OnGame,
            SelectCard,
            SelectToilet,
            Pause,
            GameEnd,
        }

        private State state;

        private bool isPlayerTurn;

        public bool IsReady
        {
            get { return state == State.Ready; }
        }

        public bool IsOnGame
        {
            get { return state == State.OnGame; }
        }

        public bool IsSelectCard
        {
            get { return state == State.SelectCard; }
        }

        public bool IsSelectToilet
        {
            get { return state == State.SelectToilet; }
        }

        public bool IsPause
        {
            get { return state == State.Pause; }
        }

        public bool IsGameEnd
        {
            get { return state == State.GameEnd; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Initialization();
        }

        // Update is called once per frame
        void Update()
        {
            if (IsOnGame)
            {
                
            }

            if (IsGameEnd)
            {

            }
        }

        void Initialization()
        {
            state = State.Ready;

            Cards.CardManager.instance.SetDeck();
            Cards.CardManager.instance.ShuffleDeck();
            Cards.CardManager.instance.FirstDraw(Player.PlayerManager.instance.hands);
            Cards.CardManager.instance.FirstDraw(Enemy.EnemyManager.instance.hands);

            Player.PlayerManager.instance.SetCard();
            SetTurn();

            state = State.OnGame;
        }

        void SetTurn()
        {
            int rand = UnityEngine.Random.Range(0, 2);
            isPlayerTurn = (rand == 0) ? true : false;
            Debug.Log((isPlayerTurn) ? ("êÊçs") : ("å„çU"));
        }
    }
}

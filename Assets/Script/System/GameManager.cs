using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public bool IsGameEnd
        {
            get { return state == State.GameEnd; }
        }

        public bool IsPlayerTurn
        {
            get { return isPlayerTurn; }
            set { isPlayerTurn = value; }
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
            StartSetTurn();

            state = State.OnGame;
        }

        void StartSetTurn()
        {
            int rand = Random.Range(0, 2);
            isPlayerTurn = (rand == 0) ? true : false;
            Debug.Log((isPlayerTurn) ? ("êÊçs") : ("å„çU"));
        }
    }
}

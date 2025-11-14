using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System
{
    public class GameManager : MonoBehaviour
    {
        enum State
        {
            Ready,
            OnGame,
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

            SetTurn();

            state = State.OnGame;
        }

        void SetTurn()
        {
            int rand = UnityEngine.Random.Range(0, 1);
            isPlayerTurn = (rand == 0) ? true : false;
        }
    }
}

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

        }

        void Initialization()
        {
            state = State.Ready;

            Cards.CardManager.instance.SetDeck();
            Cards.CardManager.instance.ShuffleDeck();
        }
    }
}

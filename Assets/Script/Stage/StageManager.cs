using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    enum State
    {
        Ready,
        OnGame,
        GameClear,
        GameOver
    }

    private State state;

    public bool IsReady
    {
        get { return state == State.Ready; }
        set
        {
            if (value) 
                state = State.Ready;
            else 
                state = State.OnGame;
        }
    }

    public bool IsOnGame
    {
        get { return state == State.OnGame; }
    }

    public bool IsGameClear
    {
        get { return state == State.GameClear; }
    }

    public bool IsGameOver
    {
        get { return state == State.GameOver; }
    }

    // Start is called before the first frame update
    void Start()
    {
        initialized();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //ステージの初期化
    void initialized()
    {
        state = State.Ready;
    }
}

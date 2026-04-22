using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Close : MonoBehaviour
{
    // 「ゲーム終了」ボタンを押した時に呼ばれる
    public void QuitGame()
    {
        Debug.Log("ゲームを終了します"); // 確認用メッセージ

        // ▼ここから終了処理
#if UNITY_EDITOR
        // Unityエディタ上でプレイしている時は、プレイモードを解除する
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // ビルドした実際のゲームでは、アプリケーションを終了する
            Application.Quit();
#endif
    }
}

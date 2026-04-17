using UnityEngine;
using UnityEngine.SceneManagement; // シーン切り替えに必要

public class Move_Scenes_Title : MonoBehaviour
{
    // ボタンが押された時に実行するメソッド
    public void GoToGame()
    {
        // "GameScene" という名前のシーンを読み込む
        SceneManager.LoadScene("ModeSlect");
    }
}
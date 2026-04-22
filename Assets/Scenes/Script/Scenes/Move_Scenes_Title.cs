using Systems;
using UnityEngine;
using UnityEngine.SceneManagement; // シーン切り替えに必要

public class Move_Scenes_Title : MonoBehaviour
{
    private void Start()
    {
        SoundManager.instance.PlayBGM(SoundManager.instance.titleBGM);
    }

    // ボタンが押された時に実行するメソッド
    public void GoToGame()
    {
        // "GameScene" という名前のシーンを読み込む
        SceneManager.LoadScene("ModeSlect");
    }
}
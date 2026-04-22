using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Move_Scenes_ModeSelect : MonoBehaviour
{
    public void GoToGame()
    {
        // "GameScene" ‚Æ‚¢‚¤–¼‘O‚ÌƒV[ƒ“‚ğ“Ç‚İ‚Ş
        SceneManager.LoadScene("MainScene");

    }
}

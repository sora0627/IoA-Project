using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultButtonUI : MonoBehaviour
{
    public void Retry()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void TitleTransition()
    {
        SceneManager.LoadScene("Title");
    }
}

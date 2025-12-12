using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    // カウントダウンする秒数（10秒）
    public int countdown = 10;

    // 時間を溜めておくための変数
    private float timer = 0.0f;

    public TextMeshProUGUI countdownText;

    // Start is called before the first frame update
    void Start()
    {
        // ゲーム開始時に一度だけログを出す
        Debug.Log("カウントダウンを開始します");
        
        if (countdownText != null)
        {
            countdownText.text = countdown.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 0秒より多い場合のみ時間を減らす
        if (countdown > 0)
        {
            // timerに「前のフレームからの経過時間」を足していく
            timer += Time.deltaTime;

            // timerが1.0秒（つまり1秒）を超えたら
            if (timer >= 1.0f)
            {
                // カウントを1減らす
                countdown--;

                // コンソールに表示
                Debug.Log(countdown);

                // タイマーを0に戻して、また次の1秒を計り始める
                timer = 0.0f;
                
                // コンソールだけでなく、画面の文字も書き換える
                // countdownTextという入れ物に中身が入っているか確認してから実行
                if (countdownText != null)
                {
                    countdownText.text = countdown.ToString();
                }
            }
        }
        else if (countdown == 0)
        {
            // 念のため0になった後の処理（これ以上ログを出さないように-1にするなど）
            countdown = -1;
            Debug.Log("終了！");
        }
    }
}

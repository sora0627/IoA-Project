using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RulePopupController : MonoBehaviour
{
    [Header("全ページをここに入れる")]
    public GameObject[] pages;

    [Header("ボタンをここに入れる")]
    public GameObject nextButton; // 右矢印
    public GameObject prevButton; // 左矢印

    private int currentPageIndex = 0; // 現在開いているページ番号(0からスタート)

    // ルール画面を開く時に呼ばれる
    public void OpenPopup()
    {
        currentPageIndex = 0; // 常に1ページ目から表示する
        UpdatePageDisplay();
        gameObject.SetActive(true); // ポップアップ全体を表示
    }

    // 「とじる」ボタンを押した時に呼ばれる
    public void ClosePopup()
    {
        gameObject.SetActive(false); // ポップアップ全体を非表示
    }

    // 「右矢印」を押した時に呼ばれる
    public void NextPage()
    {
        if (currentPageIndex < pages.Length - 1)
        {
            currentPageIndex++;
            UpdatePageDisplay();
        }
    }

    // 「左矢印」を押した時に呼ばれる
    public void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageDisplay();
        }
    }

    // ページの表示/非表示を自動で切り替える裏方の処理
    private void UpdatePageDisplay()
    {
        // 1. 一旦すべてのページを見えなくする
        foreach (var page in pages)
        {
            page.SetActive(false);
        }

        // 2. 現在のページだけを見えるようにする
        pages[currentPageIndex].SetActive(true);

        // 3. 1ページ目の時は「左矢印」を消し、それ以外なら出す
        prevButton.SetActive(currentPageIndex > 0);

        // 4. 最後のページの時は「右矢印」を消し、それ以外なら出す
        nextButton.SetActive(currentPageIndex < pages.Length - 1);
    }
}

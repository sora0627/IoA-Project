using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// ゲーム全体のUI（ポーズ画面やリザルト画面など）を管理するクラス
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Header("UIパネル設定")]
        [Tooltip("ポーズ中に表示するパネル（半透明の背景や「PAUSE」の文字など）を指定してください")]
        [SerializeField] private GameObject pausePanel;

        protected override void Awake()
        {
            base.Awake();

            // ゲーム開始時はポーズ画面を確実に非表示にしておく
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        /// <summary>
        /// ポーズ画面を表示します
        /// (GameManagerから呼ばれます)
        /// </summary>
        public void ShowPauseMenu()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("PausePanelがUIManagerに設定されていません！");
            }
        }

        /// <summary>
        /// ポーズ画面を非表示にします
        /// (GameManagerから呼ばれます)
        /// </summary>
        public void HidePauseMenu()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }
    }
}
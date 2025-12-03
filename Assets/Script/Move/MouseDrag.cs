using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Move
{
    public class MouseDrag : MonoBehaviour
    {
        [Header("ドロップ判定設定")]
        [Tooltip("ドロップターゲットの親オブジェクトの名前")]
        public string dropTargetsRootName = "ToiletPos";
        [Tooltip("ドロップターゲットからの許容誤差")]
        public float dropDistanceThreshold = 1.0f;

        [Header("ゲームルール設定")]
        [Tooltip("チェックを入れると、すでに置かれている場所の『両隣』には置けなくなります")]
        public bool enableNeighborRestriction = true;

        [Header("UI設定")]
        [Tooltip("ゲームオーバー時に表示するUI（PanelやTextなど）")]
        public GameObject gameOverUI;

        // 内部変数
        private List<ToiletHighlight> targetHighlights = new List<ToiletHighlight>();
        private Vector3 initialPosition;
        private Vector3 dragOffset;
        private float zPosition;
        private new Collider2D collider2D = null;
        private bool isGameOver = false; // ゲームオーバー状態管理フラグ

        private void Awake()
        {
            collider2D = GetComponent<Collider2D>();
            GameObject root = GameObject.Find(dropTargetsRootName);

            if (root != null)
            {
                foreach (Transform child in root.transform)
                {
                    ToiletHighlight hl = child.GetComponent<ToiletHighlight>();
                    if (hl != null)
                    {
                        targetHighlights.Add(hl);
                    }
                }
            }
            else
            {
                Debug.LogError($"{dropTargetsRootName} が見つかりません");
            }

            initialPosition = transform.position;

            // ゲーム開始時はUIを隠しておく
            if (gameOverUI != null)
            {
                gameOverUI.SetActive(false);
            }
        }

        private void OnMouseDown()
        {
            if (isGameOver) return;

            initialPosition = transform.position;
            zPosition = transform.position.z;
            dragOffset = transform.position - GetMouseWorldPosition();
        }

        private void OnMouseDrag()
        {
            if (isGameOver) return;

            Vector3 newPosition = GetMouseWorldPosition() + dragOffset;
            newPosition.z = zPosition;
            transform.position = newPosition;

            // ドラッグ中のハイライト更新
            for (int i = 0; i < targetHighlights.Count; i++)
            {
                ToiletHighlight target = targetHighlights[i];

                // 埋まっている、またはルール上置けない場所は無視
                if (target.IsOccupied || IsTargetReserved(i))
                {
                    target.Highlight(0);
                    continue;
                }

                float distance = Vector3.Distance(transform.position, target.transform.position);

                if (distance <= dropDistanceThreshold)
                {
                    target.Highlight(2);
                }
                else
                {
                    target.Highlight(1);
                }
            }
        }

        private void OnMouseUp()
        {
            if (isGameOver) return;

            ToiletHighlight successTarget = null;
            float minDistance = float.MaxValue;

            // ドロップ可能なターゲットを探す
            for (int i = 0; i < targetHighlights.Count; i++)
            {
                ToiletHighlight target = targetHighlights[i];
                target.Highlight(0); // 色をリセット

                // 置けない場所（占有済み or 隣接ルール違反）はスキップ
                if (target.IsOccupied || IsTargetReserved(i))
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, target.transform.position);

                if (distance <= dropDistanceThreshold && distance < minDistance)
                {
                    minDistance = distance;
                    successTarget = target;
                }
            }

            if (successTarget != null)
            {
                // --- ドロップ成功時の処理 ---
                Debug.Log("ドロップ成功");
                successTarget.Occupy();

                if (Player.PlayerManager.instance != null)
                {
                    Player.PlayerManager.instance.isSet = true;
                }

                transform.position = successTarget.transform.position;
                if (collider2D != null) collider2D.enabled = false;

                // ★手詰まりチェック: 配置後に「もう置ける場所が残っていないか」を確認
                CheckIfAnySpotsAvailable();
            }
            else
            {
                // --- 失敗時の処理 ---
                // お手付きはゲームオーバーにせず、元の位置に戻すだけ
                Debug.Log("失敗：置けない場所です");
                transform.position = initialPosition;
            }
        }

        /// <summary>
        /// 盤面にまだ置ける場所が残っているかチェックし、なければゲームオーバーにする
        /// </summary>
        private void CheckIfAnySpotsAvailable()
        {
            bool hasAvailableSpot = false;

            for (int i = 0; i < targetHighlights.Count; i++)
            {
                // 「空いている」かつ「隣接ルール的にもOK」な場所があるか？
                if (!targetHighlights[i].IsOccupied && !IsTargetReserved(i))
                {
                    hasAvailableSpot = true;
                    break; // 1つでもあればまだゲーム続行可能
                }
            }

            if (!hasAvailableSpot)
            {
                // 置ける場所が一つもない＝手詰まり
                Debug.Log("手詰まりです。置ける場所がありません。");
                TriggerGameOver("置ける場所がなくなりました");
            }
        }

        /// <summary>
        /// ゲームオーバー処理
        /// </summary>
        private void TriggerGameOver(string reason)
        {
            Debug.Log($"Game Over: {reason}");

            isGameOver = true;

            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
            }
        }

        /// <summary>
        /// 隣接ルールなどのチェック
        /// </summary>
        private bool IsTargetReserved(int targetIndex)
        {
            if (!enableNeighborRestriction) return false;

            if (targetIndex > 0)
            {
                if (targetHighlights[targetIndex - 1].IsOccupied) return true;
            }

            if (targetIndex < targetHighlights.Count - 1)
            {
                if (targetHighlights[targetIndex + 1].IsOccupied) return true;
            }

            return false;
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = Camera.main.nearClipPlane + 10f;
            return Camera.main.ScreenToWorldPoint(mousePoint);
        }
    }
}
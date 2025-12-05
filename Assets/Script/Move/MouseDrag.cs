using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Move
{
    public class MouseDrag : MonoBehaviour
    {
        // =========================================================
        //  既存の処理 (ドラッグ操作など)
        //  ※あなたの元のコードに合わせて適宜調整してください
        // =========================================================

        private Vector3 screenPoint;
        private Vector3 offset;

        void OnMouseDown()
        {
            // オブジェクトの位置をスクリーン座標に変換
            screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

            // マウス位置(スクリーン座標)とオブジェクト位置の差分(オフセット)を計算
            offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        }

        void OnMouseDrag()
        {
            // 現在のマウス位置(スクリーン座標)を取得
            Vector3 currentScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

            // ワールド座標に変換し、オフセットを加算して現在位置とする
            Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenPoint) + offset;

            // オブジェクトの位置を更新
            transform.position = currentPosition;
        }

        // =========================================================
        //  新規追加: ターン開始時のゲームオーバー判定用メソッド
        // =========================================================

        /// <summary>
        /// ターン開始時にゲームオーバー（手詰まり）になっていないか確認するメソッド
        /// 引数: ターゲットタグ名, グローバル制限フラグ, 手札の制限情報リスト(List<bool>)
        /// </summary>
        public static void CheckGameOverAtStartOfTurn(string targetTag, bool globalNeighborRestriction, List<bool> handRestrictions)
        {
            // 1. 配置候補となる場所をすべて取得
            GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(targetTag);

            // ターゲットが見つからない場合の安全策
            if (targetObjects == null || targetObjects.Length == 0)
            {
                Debug.LogWarning($"タグ '{targetTag}' を持つオブジェクトが見つかりません。ゲームオーバー判定をスキップします。");
                return;
            }

            // 配置可能な場所が見つかったかを管理するフラグ
            bool canPlaceAnyCard = false;

            // 2. 手札の各カード情報（制限タイプかどうか）についてループ
            foreach (bool isRestricted in handRestrictions)
            {
                // 全ての配置候補場所に対してシミュレーション
                foreach (GameObject target in targetObjects)
                {
                    // 既にカードが置かれている場所（子要素がある場所）はスキップ
                    if (target.transform.childCount > 0) continue;

                    // 配置可能判定（シミュレーション）
                    bool isPlaceable = CheckIfPlaceable(target, isRestricted, globalNeighborRestriction);

                    if (isPlaceable)
                    {
                        // 一つでも置ける場所があれば、このカードに関しては「置ける場所がある」
                        // そして「手札の中に少なくとも1枚は置けるカードがある」ことが確定する
                        canPlaceAnyCard = true;
                        break;
                    }
                }

                // 一枚でも置けるカードが見つかれば、ゲームオーバーではないのでループを抜ける
                if (canPlaceAnyCard) break;
            }

            // 3. 判定結果による分岐
            if (!canPlaceAnyCard)
            {
                Debug.Log("【Game Over】配置できるカードがありません！");
                // TODO: ここに実際のゲームオーバー処理を記述してください
                // 例: GameManager.Instance.TriggerGameOver();
            }
            else
            {
                Debug.Log("配置可能なカードがあります。ゲーム続行。");
            }
        }

        /// <summary>
        /// 実際に配置可能かを判定するヘルパーメソッド（シミュレーション用）
        /// ※ゲームの具体的なルール（隣接制限など）に合わせてロジックを実装してください。
        /// </summary>
        private static bool CheckIfPlaceable(GameObject targetSlot, bool isRestrictedType, bool globalRestriction)
        {
            // --- ここに実際の配置ルール判定を実装します ---

            // 例: グローバル制限（隣接制限など）がある場合
            if (globalRestriction)
            {
                // 隣接するスロットを確認するロジックなどが必要になります。
                // 現在は仮実装として常に true を返します。
                // 実際にはルールに基づいて true/false を返してください。
                return true;
            }

            // 制限がない場合は置けるとする
            return true;
        }
    }
}
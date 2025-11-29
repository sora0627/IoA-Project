using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class DropZoneManager : MonoBehaviour
{
    // === Inspectorで設定する項目 ===
    [Header("ターゲット設定")]
    [Tooltip("ドロップターゲットの親オブジェクトの名前")]
    public string dropTargetsRootName = "ToiletPos";

    // ターゲットの隣接判定に必要な情報
    [Tooltip("ドロップ判定に必要な許容誤差（DragObject2Dと同じ値にすること）")]
    public float dropDistanceThreshold = 1.0f;

    // === スクリプト内部で使う変数 ===
    private List<Transform> dropTargets;

    void Awake()
    {
        // ターゲットの動的検索処理
        GameObject root = GameObject.Find(dropTargetsRootName);

        if (root != null)
        {
            // 直下の子Transformのみを取得
            List<Transform> children = new List<Transform>();
            foreach (Transform child in root.transform)
            {
                children.Add(child);
            }
            dropTargets = children.ToList();
            Debug.Log($"[Manager] {dropTargets.Count}個のドロップターゲットを登録しました。");
        }
        else
        {
            Debug.LogError($"[Manager] ドロップターゲットの親オブジェクト '{dropTargetsRootName}' がシーンに見つかりません。");
            dropTargets = new List<Transform>();
        }
    }

    // 【★追加するパブリックな判定関数】
    /// <summary>
    /// 現在のシーンでドロップ可能なターゲットが一つでも存在するかどうかを判定します。
    /// </summary>
    /// <returns>ドロップ可能な場所があればtrue、なければfalse。</returns>
    public bool CanAnyTargetBeDropped()
    {
        if (dropTargets == null || dropTargets.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < dropTargets.Count; i++)
        {
            ToiletHighlight targetHighlight = dropTargets[i].GetComponent<ToiletHighlight>();

            if (targetHighlight == null) continue;

            // 1. 既に占有されているかチェック
            if (targetHighlight.IsOccupied)
            {
                continue;
            }

            // 2. 隣接予約されているかチェック
            if (IsTargetReserved(i))
            {
                continue;
            }

            // どちらの条件にも当てはまらなければ、ドロップ可能
            return true;
        }

        // 全てのターゲットをチェックしたが、ドロップ可能な場所が見つからなかった
        return false;
    }

    /// <summary>
    /// 指定されたインデックスのターゲットが、占有されたターゲットの隣にあるかチェックします。
    /// </summary>
    public bool IsTargetReserved(int targetIndex)
    {
        // 左隣をチェック
        if (targetIndex > 0)
        {
            ToiletHighlight leftNeighbor = dropTargets[targetIndex - 1].GetComponent<ToiletHighlight>();
            if (leftNeighbor != null && leftNeighbor.IsOccupied)
            {
                return true;
            }
        }

        // 右隣をチェック
        if (targetIndex < dropTargets.Count - 1)
        {
            ToiletHighlight rightNeighbor = dropTargets[targetIndex + 1].GetComponent<ToiletHighlight>();
            if (rightNeighbor != null && rightNeighbor.IsOccupied)
            {
                return true;
            }
        }
        return false;
    }
}
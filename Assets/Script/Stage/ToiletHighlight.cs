using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 変更点はほぼありませんが、MouseDrag側との整合性を保つため提示します
public class ToiletHighlight : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    // 色の設定
    private readonly Color defaultColor = new Color(0.7f, 0.7f, 0.7f, 0.5f); // 未使用・待機
    private readonly Color activeColor = new Color(0f, 1f, 0f, 0.7f);        // ドラッグ中・空きあり
    private readonly Color approachColor = new Color(1f, 1f, 0f, 0.7f);      // ドロップ可能範囲内

    // 占有されているかどうか
    public bool IsOccupied { get; private set; } = false;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            // 自身についている場合も考慮してフォールバック
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            Debug.LogError($"{gameObject.name}: SpriteRendererが見つかりません。ハイライト機能が無効になります。");
            return;
        }

        spriteRenderer.color = defaultColor;
    }

    /// <summary>
    /// ハイライトの色を切り替える
    /// state: 0=通常, 1=アクティブ(遠い), 2=ドロップ可能(近い)
    /// </summary>
    public void Highlight(int state)
    {
        if (spriteRenderer == null) return;

        // 既に埋まっている場合は色を変えない（あるいは占有色にする）
        if (IsOccupied)
        {
            spriteRenderer.color = defaultColor; // 必要に応じて「赤」などにしても分かりやすいです
            return;
        }

        switch (state)
        {
            case 0:
                spriteRenderer.color = defaultColor;
                break;
            case 1:
                spriteRenderer.color = activeColor;
                break;
            case 2:
                spriteRenderer.color = approachColor;
                break;
        }
    }

    /// <summary>
    /// 占有状態を確定させる
    /// </summary>
    public void Occupy()
    {
        IsOccupied = true;
        Highlight(0); // 色をリセット
    }
}
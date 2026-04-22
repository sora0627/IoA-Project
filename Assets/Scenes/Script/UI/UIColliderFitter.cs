using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIColliderFitter : MonoBehaviour
{
    private RectTransform rectTransform;
    private BoxCollider2D boxCollider;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (rectTransform == null || boxCollider == null)
        {
            Debug.LogError("RectTransform または BoxCollider2D が見つかりません。");
            return;
        }

        // RectTransformのサイズ（WidthとHeight）をコライダーのサイズに設定
        Vector2 rectSize = rectTransform.rect.size;
        boxCollider.size = rectSize;

        // オフセットをUI要素の中心に設定（ピボットが(0.5, 0.5)の場合）
        boxCollider.offset = new Vector2(0f, 0f);
    }


    void Update()
    {
        // サイズが変わっているかチェックして更新...
        Vector2 rectSize = rectTransform.rect.size;
        if (boxCollider.size != rectSize)
        {
            boxCollider.size = rectSize;
            boxCollider.offset = new Vector2(0f, 0f); // 必要に応じて
        }
    }
}

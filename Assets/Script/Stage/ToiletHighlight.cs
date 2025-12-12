using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Move; // MouseDragを参照するために名前空間を追加

public class ToiletHighlight : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private readonly Color defaultColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
    private readonly Color activeColor = new Color(0f, 1f, 0f, 0.7f);
    private readonly Color approachColor = new Color(1f, 1f, 0f, 0.7f);

    // ★変更点: 単なるboolではなく、置かれている「物（MouseDrag）」を記憶する
    public MouseDrag OccupyingObject { get; private set; } = null;

    // IsOccupiedは「OccupyingObjectがnullじゃない」ならTrue
    public bool IsOccupied => OccupyingObject != null;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError($"{gameObject.name}: SpriteRendererが見つかりません。");
            return;
        }
        spriteRenderer.color = defaultColor;
    }

    public void Highlight(int state)
    {
        if (spriteRenderer == null) return;

        // 既に埋まっている場合は色を変えない
        if (IsOccupied)
        {
            spriteRenderer.color = defaultColor;
            return;
        }

        switch (state)
        {
            case 0: spriteRenderer.color = defaultColor; break;
            case 1: spriteRenderer.color = activeColor; break;
            case 2: spriteRenderer.color = approachColor; break;
        }
    }

    /// <summary>
    /// オブジェクトをセットして占有状態にする
    /// </summary>
    public void SetOccupier(MouseDrag obj)
    {
        OccupyingObject = obj;
        Highlight(0);
    }

    /// <summary>
    /// ★追加: オブジェクトが退いたので空き状態にする
    /// </summary>
    public void Vacate()
    {
        OccupyingObject = null;
        Highlight(0);
    }
}
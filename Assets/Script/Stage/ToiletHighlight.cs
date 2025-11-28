using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToiletHighlight : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private readonly Color defaultColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
    private readonly Color activeColor = new Color(0f, 1f, 0f, 0.7f);
    private readonly Color approachColor = new Color(1f, 1f, 0f, 0.7f);

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError(gameObject.name + "SpriteRendererが子孫オブジェクトに見つかりませんでした。ハイライト機能が無効になります。");
            return;
        }

        spriteRenderer.color = defaultColor;
    }

    /// <summary>
    /// ドラッグ中のオブジェクトが近づいたら色を変える
    /// </summary>
    /// <param name="isNear"></param>
    public void Highlight(int state)
    {
        if (spriteRenderer == null) return;

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
}

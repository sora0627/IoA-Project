using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageScaler : MonoBehaviour
{
    private static ImageScaler currentMagnifiedSprite = null;

    private float magnifiedScale = 1.2f;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnMouseDown()
    {
        Debug.Log("click");
        if (currentMagnifiedSprite != null && currentMagnifiedSprite != this)
        {
            currentMagnifiedSprite.ResetScale();
        }

        if (transform.localScale.x < magnifiedScale)
        {
            transform.localScale = originalScale * magnifiedScale;
            currentMagnifiedSprite = this;
        }
    }

    public void ResetScale()
    {
        transform.localScale = originalScale;
    }
}

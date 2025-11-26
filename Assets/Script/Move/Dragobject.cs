using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Dragobject : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{

    private Vector2 prevPos; //保存しておく初期position
    private RectTransform rectTransform; // 移動したいオブジェクトのRectTransform
    private RectTransform parentRectTransform; // 移動したいオブジェクトの親(Panel)のRectTransform


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRectTransform = rectTransform.parent as RectTransform;
    }


    // ドラッグ開始時の処理
    public void OnBeginDrag(PointerEventData eventData)
    {
        // ドラッグ前の位置を記憶しておく
        // RectTransformの場合はpositionではなくanchoredPositionを使う
        prevPos = rectTransform.anchoredPosition;

    }

    // ドラッグ中の処理
    public void OnDrag(PointerEventData eventData)
    {
        // eventData.positionから、親に従うlocalPositionへの変換を行う
        // オブジェクトの位置をlocalPositionに変更する

        Vector2 localPosition = GetLocalPosition(eventData.position);
        Rect parentRect = parentRectTransform.rect;
        Rect myRect = rectTransform.rect;
        Vector2 pivot = rectTransform.pivot;
        // 横方向(X)の制限範囲を計算
        // 左端 = 親の左端 + (自分の幅 × ピボットの割合)
        float minX = parentRect.xMin + (myRect.width * pivot.x);
        // 右端 = 親の右端 - (自分の幅 × (1 - ピボットの割合))
        float maxX = parentRect.xMax - (myRect.width * (1 - pivot.x));

        // 縦方向(Y)の制限範囲を計算
        float minY = parentRect.yMin + (myRect.height * pivot.y);
        float maxY = parentRect.yMax - (myRect.height * (1 - pivot.y));

        // 計算した移動先(localPosition)を、制限範囲内に収める (Mathf.Clamp)
        localPosition.x = Mathf.Clamp(localPosition.x, minX, maxX);
        localPosition.y = Mathf.Clamp(localPosition.y, minY, maxY);

        rectTransform.anchoredPosition = localPosition;
    }
    // ドラッグ終了時の処理
    public void OnEndDrag(PointerEventData eventData)
    {
        // オブジェクトをドラッグ前の位置に戻す
        //rectTransform.anchoredPosition = prevPos;
    }
    // ScreenPositionからlocalPositionへの変換関数
    private Vector2 GetLocalPosition(Vector2 screenPosition)
    {
        Vector2 result = Vector2.zero;

        // screenPositionを親の座標系(parentRectTransform)に対応するよう変換する.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, screenPosition, Camera.main, out result);

        return result;
    }

}

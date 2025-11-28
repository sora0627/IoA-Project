using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseDrag : MonoBehaviour
{
    [Header("ドロップ判定設定")]
    [Tooltip("ドロップターゲットの親オブジェクトの名前")]
    public string dropTargetsRootName = "ToiletPos";
    [Tooltip("ドロップターゲットからの許容誤差")]
    public float dropDistanceThreshold = 1.0f;

    private List<Transform> dropTargets;
    private Vector3 initialPosition;
    private Vector3 dragOffset;
    private float zPosition;

    private void Awake()
    {
        GameObject root = GameObject.Find(dropTargetsRootName);

        if (root != null)
        {
            List<Transform> children = new List<Transform>();

            foreach (Transform child in root.transform)
            {
                children.Add(child);
            }

            dropTargets = children.ToList();

            Debug.Log($"ターゲット成功:{dropTargets.Count}");
        }
        else
        {
            Debug.Log($"{dropTargetsRootName}が見つかりません");
            dropTargets = new List<Transform>();
        }

        initialPosition = transform.position;
    }

    private void OnMouseDown()
    {
        initialPosition = transform.position;

        zPosition = transform.position.z;
        dragOffset = transform.position - GetMouseWorldPosition();

        Debug.Log(gameObject.name + "をクリック ");
    }

    private void OnMouseDrag()
    {
        Vector3 newPosition = GetMouseWorldPosition() + dragOffset;
        newPosition.z = zPosition;
        transform.position = newPosition;

        foreach (Transform target in dropTargets)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            ToiletHighlight targetHighlight = target.GetComponent<ToiletHighlight>();

            if (targetHighlight != null)
            {
                if (distance <= dropDistanceThreshold)
                {
                    targetHighlight.Highlight(2);
                }
                else
                {
                    targetHighlight.Highlight(1);
                }
            }
        }
    }

    /// <summary>
    /// ドラッグ終了
    /// </summary>
    private void OnMouseUp()
    {
        Transform successTarget = null;

        foreach (Transform target in dropTargets)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            ToiletHighlight targetHighlight = target.GetComponent<ToiletHighlight>();
            if (targetHighlight != null)
            {
                targetHighlight.Highlight(0);
            }

            if (distance <= dropDistanceThreshold)
            {
                successTarget = target;
                break;
            }

        }

        if (successTarget != null)
        {
            Debug.Log("成功");

            transform.position = successTarget.position;
        }
        else
        {
            Debug.Log("失敗");

            transform.position = initialPosition;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.nearClipPlane + 10f;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}

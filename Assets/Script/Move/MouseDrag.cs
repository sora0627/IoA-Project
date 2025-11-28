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

        private List<Transform> dropTargets;
        private Vector3 initialPosition;
        private Vector3 dragOffset;
        private float zPosition;

        private new Collider2D collider2D = null;

        private void Awake()
        {
            collider2D = GetComponent<Collider2D>();
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

        /// <summary>
        /// 隣にオブジェクトがあるかチェックする
        /// </summary>
        /// <param name="targetIndex"></param>
        /// <returns></returns>
        private bool IsTargetReserved(int targetIndex)
        {
            if (targetIndex > 0)
            {
                ToiletHighlight leftNeighbor = dropTargets[targetIndex - 1].GetComponent<ToiletHighlight>();
                if (leftNeighbor != null && leftNeighbor.IsOccupied)
                {
                    return true;
                }
            }

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
            int index = -1;

            foreach (Transform target in dropTargets)
            {
                index++;
                ToiletHighlight targetHighlight = target.GetComponent<ToiletHighlight>();

                if (targetHighlight != null)
                {
                    if (targetHighlight.IsOccupied)
                    {
                        continue;
                    }

                    if (IsTargetReserved(index))
                    {
                        targetHighlight.Highlight(0); // 念のため薄いグレーを強制
                        continue;
                    }

                    float distance = Vector3.Distance(transform.position, target.position);

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
            int index = -1;

            foreach (Transform target in dropTargets)
            {
                index++;
                ToiletHighlight targetHighlight = target.GetComponent<ToiletHighlight>();
                if (targetHighlight != null && (targetHighlight.IsOccupied || IsTargetReserved(index)))
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, target.position);

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

                successTarget.GetComponent<ToiletHighlight>()?.Occupy();
                Player.PlayerManager.instance.isSet = true;
                transform.position = successTarget.position;

                collider2D.enabled = false;
            }
            else
            {
                Debug.Log("失敗");

                transform.position = initialPosition;
            }

            foreach (Transform target in dropTargets)
            {
                ToiletHighlight targetHighlight = target.GetComponent<ToiletHighlight>();
                if (targetHighlight != null)
                {
                    targetHighlight.Highlight(0);
                }
            }
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = Camera.main.nearClipPlane + 10f;
            return Camera.main.ScreenToWorldPoint(mousePoint);
        }
    }
}

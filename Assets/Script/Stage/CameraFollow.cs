using System.Diagnostics.Contracts;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("追従対象")]
    [Tooltip("追従させるプレイヤー")]
    [SerializeField]
    private Transform target;

    [Header("カメラ設定")]
    [Tooltip("追従の速さ")]
    [SerializeField]
    private float smoothSpeed = 0.125f;

    [Tooltip("プレイヤーとカメラのオフセット")]
    [SerializeField]
    private float offsetX = 0f;

    [Header("ステージ境界設定")]
    [Tooltip("カメラが移動できるX座標の最小値")]
    [SerializeField]
    private float minX = -10f;

    [Tooltip("カメラが移動できるX座標の最大値")]
    [SerializeField]
    private float maxX = 10f;

    [Tooltip("カメラが追従を再開するX座標")]
    [SerializeField]
    private float followResumeDistance = 2f;

    private PlayerMovement playerMovement;
    private bool isFollowing = true;

    private float fixedY;
    private float fixedZ;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = new Vector3(
            transform.position.x,
            fixedY,
            fixedZ
        );

        float targetX = target.position.x + offsetX;
        float currentCameraX = transform.position.x;

        if (!isFollowing)
        {
            if (Mathf.Abs(targetX - currentCameraX) > followResumeDistance)
            {
                isFollowing = true;
            }
        }

        if (isFollowing)
        {
            desiredPosition.x = targetX;
        }
        else
        {
            desiredPosition.x = currentCameraX;
        }

        if (desiredPosition.x < minX || desiredPosition.x > maxX)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);

            if (Mathf.Abs(desiredPosition.x - currentCameraX) > 0.001f)
            {
                isFollowing = false;
            }
        }
        else if (isFollowing)
        {
            desiredPosition.x = targetX;
        }

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(minX, 100, 0), new Vector3(minX, -100, 0));
        Gizmos.DrawLine(new Vector3(maxX, 100, 0), new Vector3(maxX, -100, 0));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;

    [Header("阻尼参数")]
    [Range(0, 1)] public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    [Header("边界控制")]
    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    [Header("偏移量")]
    public Vector3 offset = new Vector3(0, 0, -10);

    public Transform selfTrans;

    private void Awake()
    {
        selfTrans = transform;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 计算目标位置
        Vector3 targetPosition = target.position + offset;

        // 边界限制
        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.z = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }

        // 平滑跟随
        selfTrans.position = Vector3.SmoothDamp(
            selfTrans.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    // 在Scene视图显示边界（调试用）
    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        Gizmos.color = Color.green;
        Vector3 center = new Vector3(
            (minBounds.x + maxBounds.x) * 0.5f,
            transform.position.y,
            (minBounds.y + maxBounds.y) * 0.5f
        );
        Vector3 size = new Vector3(
            maxBounds.x - minBounds.x,
            0.1f,
            maxBounds.y - minBounds.y
        );
        Gizmos.DrawWireCube(center, size);
    }
}

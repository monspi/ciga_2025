using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    [Header("����Ŀ��")]
    public Transform target;

    [Header("�������")]
    [Range(0, 1)] public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    [Header("�߽����")]
    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    [Header("ƫ����")]
    public Vector3 offset = new Vector3(0, 0, -10);

    public Transform selfTrans;

    private void Awake()
    {
        selfTrans = transform;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // ����Ŀ��λ��
        Vector3 targetPosition = target.position + offset;

        // �߽�����
        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.z = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }

        // ƽ������
        selfTrans.position = Vector3.SmoothDamp(
            selfTrans.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    // ��Scene��ͼ��ʾ�߽磨�����ã�
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

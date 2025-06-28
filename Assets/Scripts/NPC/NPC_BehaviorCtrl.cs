using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NPC_BehaviorCtrl : MonoBehaviour
{
    [Header("NPC唯一编号")]
    public int npcID; // NPC的唯一标识符

    public SpriteRenderer spriteRenderer; // 用于渲染NPC的SpriteRenderer组件

    public Transform selfTrans;

    private Vector3 lastPosition; // 上一帧的位置，用于判断是否需要更新渲染顺序

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("NPC_BehaviorCtrl: 没有找到SpriteRenderer组件", gameObject);
            }
        }
        if (selfTrans == null)
        {
            selfTrans = transform;
        }
        HandleSortingOrder();
        lastPosition = selfTrans.position; // 初始化上一帧位置
    }

    private void Update()
    {
        // 检查NPC位置是否发生变化，如果发生变化则更新渲染顺序
        if ((selfTrans.position - lastPosition).sqrMagnitude >= 0.0001f)
        {
            HandleSortingOrder();
            lastPosition = selfTrans.position; // 更新上一帧位置
        }
    }


    void HandleSortingOrder()
    {
        if (spriteRenderer != null)
        {
            Vector3 position = selfTrans.position;
            if (spriteRenderer.spriteSortPoint == SpriteSortPoint.Pivot)
            {
                //100像素比1米，所以要乘以0.01
                position = spriteRenderer.transform.TransformPoint(-spriteRenderer.sprite.pivot * 0.01f);
            }
            // 根据NPC的z坐标设置渲染顺序
            spriteRenderer.sortingOrder = SpriteRenderOrderUtility.GetRenderOrder(position);
        }
    }


}

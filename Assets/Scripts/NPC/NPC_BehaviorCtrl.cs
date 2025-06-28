using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NPC_BehaviorCtrl : MonoBehaviour
{
    [Header("NPCΨһ���")]
    public int npcID; // NPC��Ψһ��ʶ��

    public SpriteRenderer spriteRenderer; // ������ȾNPC��SpriteRenderer���

    public Transform selfTrans;

    private Vector3 lastPosition; // ��һ֡��λ�ã������ж��Ƿ���Ҫ������Ⱦ˳��

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("NPC_BehaviorCtrl: û���ҵ�SpriteRenderer���", gameObject);
            }
        }
        if (selfTrans == null)
        {
            selfTrans = transform;
        }
        HandleSortingOrder();
        lastPosition = selfTrans.position; // ��ʼ����һ֡λ��
    }

    private void Update()
    {
        // ���NPCλ���Ƿ����仯����������仯�������Ⱦ˳��
        if ((selfTrans.position - lastPosition).sqrMagnitude >= 0.0001f)
        {
            HandleSortingOrder();
            lastPosition = selfTrans.position; // ������һ֡λ��
        }
    }


    void HandleSortingOrder()
    {
        if (spriteRenderer != null)
        {
            Vector3 position = selfTrans.position;
            if (spriteRenderer.spriteSortPoint == SpriteSortPoint.Pivot)
            {
                //100���ر�1�ף�����Ҫ����0.01
                position = spriteRenderer.transform.TransformPoint(-spriteRenderer.sprite.pivot * 0.01f);
            }
            // ����NPC��z����������Ⱦ˳��
            spriteRenderer.sortingOrder = SpriteRenderOrderUtility.GetRenderOrder(position);
        }
    }


}

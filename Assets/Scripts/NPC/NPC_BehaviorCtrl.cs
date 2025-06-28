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

    public BaseAnimController AnimController; // ���������������ڿ���NPC�Ķ�������
    

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
        if(AnimController == null)
        {
            AnimController = GetComponent<BaseAnimController>();
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

    public void PlayAnim(FullNpcAnimState animStage)
    {
        if (AnimController != null)
        {            
            AnimController.PlayAnim(animStage.ToString());
        }
        else
        {
            Debug.LogWarning("NPC_BehaviorCtrl: û�����ö���������", gameObject);
        }
    }

    public void PlayAnim(NpcAnimState animStage)
    {
        if (AnimController != null)
        {
            AnimController.PlayAnim(animStage);
        }
        else
        {
            Debug.LogWarning("NPC_BehaviorCtrl: û�����ö���������", gameObject);
        }
    }

    [ContextMenu("����һ������")]
    public void PlayAnim1()
    {
        PlayAnim(NpcAnimState.Anim1);
    }

    [ContextMenu("���ڶ�������")]
    public void PlayAnim2()
    {
        PlayAnim(NpcAnimState.Anim2);
    }

}

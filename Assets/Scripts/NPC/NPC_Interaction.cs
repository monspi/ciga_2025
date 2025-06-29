///npc�����߼���Ӧ�ű������ڴ���NPC����ҵĽ����¼�

using System.Collections;
using System.Collections.Generic;
using GameLogic.Battle;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPC_Interaction : MonoBehaviour
{
    public NPC_BehaviorCtrl npc_BehaviorCtrl;
    private Material _material;

    private void Awake()
    {
        if(npc_BehaviorCtrl == null)
        {
            Reset(); // ���û���ֶ�����npc_BehaviorCtrl�����ԴӸ������ȡ
            _material =  npc_BehaviorCtrl.GetComponent<Renderer>().material;
        }
    }

    private void Reset()
    {
        npc_BehaviorCtrl = GetComponentInParent<NPC_BehaviorCtrl>();
        if (npc_BehaviorCtrl == null)
        {
            Debug.LogError("�Ҳ���npc��Ϊ������",gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ����NPC�����߼�
            Debug.Log($"��NPC {npc_BehaviorCtrl.npcID} ����");
            // ������������������������������Ľ����߼�
            
            BattleController.Inst.StartBattle(npc_BehaviorCtrl.npcID, 3, () =>
            {
                
            }, () =>
            {
                
            });
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ����NPC�����߼�
            Debug.Log($"������NPC {npc_BehaviorCtrl.npcID} �Ľ���");
            // ������������������������������Ľ��������߼�
        }
    }

    public void SetGray()
    {
        StopAllCoroutines();
        StartCoroutine(ToGrayCo());
    }

    public void SetColorful()
    {
        StopAllCoroutines();
        StartCoroutine(ToColorfulCo());
    }
    
    private IEnumerator ToGrayCo()
    {
        float value = _material.GetFloat("_Value");
        _material.SetFloat("_Value", 1);
        while (value > 0 + 1e-3)
        {
            value -= Time.deltaTime * 2;
            if (value <= 0)
            {
                value = 0;
            }
            _material.SetFloat("_Value", value);
            yield return null;
        }
    }
    
    private IEnumerator ToColorfulCo()
    {
        StopAllCoroutines();
        float value = _material.GetFloat("_Value");
        _material.SetFloat("_Value", 1);
        while (value < 1 - 1e-3)
        {
            value += Time.deltaTime * 2;
            if (value <= 0)
            {
                value = 0;
            }
            _material.SetFloat("_Value", value);
            yield return null;
        }
    }
}

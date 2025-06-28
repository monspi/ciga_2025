///npc�����߼���Ӧ�ű������ڴ���NPC����ҵĽ����¼�

using System.Collections;
using System.Collections.Generic;
using GameLogic.Battle;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPC_Interaction : MonoBehaviour
{
    public NPC_BehaviorCtrl npc_BehaviorCtrl;

    private void Awake()
    {
        if(npc_BehaviorCtrl == null)
        {
            Reset(); // ���û���ֶ�����npc_BehaviorCtrl�����ԴӸ������ȡ
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
}

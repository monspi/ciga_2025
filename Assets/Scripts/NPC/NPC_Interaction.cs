///npc交互逻辑响应脚本，用于处理NPC与玩家的交互事件

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPC_Interaction : MonoBehaviour
{
    public NPC_BehaviorCtrl npc_BehaviorCtrl;

    private void Awake()
    {
        if(npc_BehaviorCtrl == null)
        {
            Reset(); // 如果没有手动设置npc_BehaviorCtrl，则尝试从父物体获取
        }
    }

    private void Reset()
    {
        npc_BehaviorCtrl = GetComponentInParent<NPC_BehaviorCtrl>();
        if (npc_BehaviorCtrl == null)
        {
            Debug.LogError("找不到npc行为控制器",gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 触发NPC交互逻辑
            Debug.Log($"与NPC {npc_BehaviorCtrl.npcID} 交互");
            // 可以在这里调用其他方法来处理具体的交互逻辑
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 结束NPC交互逻辑
            Debug.Log($"结束与NPC {npc_BehaviorCtrl.npcID} 的交互");
            // 可以在这里调用其他方法来处理具体的交互结束逻辑
        }
    }
}

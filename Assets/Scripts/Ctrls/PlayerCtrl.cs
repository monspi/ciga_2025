using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class PlayerCtrl : MonoBehaviour
{
    public NavMeshAgent agent; // �����������������·��Ѱ·
    [Header("�ƶ��ٶ�")]
    public float speed = 3.5f; // ����ƶ��ٶ�
    public ParticleSystemRenderer characterParticleRenderer;
    public Transform selfTrans;
    
    // 移动禁用标志，用于战斗时阻止玩家移动
    private bool _movementDisabled = false;

    // Start is called before the first frame update
    void Awake()
    {
        if(agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        if(selfTrans == null)
            selfTrans = transform;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleRendererOrder();
    }

    private void HandleRendererOrder()
    {
        if(characterParticleRenderer != null)
        {
            characterParticleRenderer.sortingOrder = SpriteRenderOrderUtility.GetRenderOrder(selfTrans.position);
        }
    }

    private void HandleMovement()
    {
        // 检查移动是否被禁用（战斗时）
        if (_movementDisabled)
            return;
            
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        // ���û�����룬���ƶ�
        if (Mathf.Approximately(horizontal, 0) && Mathf.Approximately(vertical, 0))
        {
            return;
        }
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
        agent.Move(direction * speed * Time.deltaTime);
    }
    
    /// <summary>
    /// 设置玩家移动启用状态
    /// </summary>
    /// <param name="enabled">true启用移动，false禁用移动</param>
    public void SetMovementEnabled(bool enabled)
    {
        _movementDisabled = !enabled;
        Debug.Log($"[PlayerCtrl] 移动状态设置为: {(enabled ? "启用" : "禁用")}");
    }
}

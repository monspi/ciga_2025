using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class PlayerCtrl : MonoBehaviour
{
    public NavMeshAgent agent; // ���������������·��Ѱ·
    [Header("�ƶ��ٶ�")]
    public float speed = 3.5f; // ����ƶ��ٶ�
    public ParticleSystemRenderer characterParticleRenderer;
    public Transform selfTrans;

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
}

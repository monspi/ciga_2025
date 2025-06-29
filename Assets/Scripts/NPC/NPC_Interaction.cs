///npc�����߼���Ӧ�ű������ڴ���NPC����ҵĽ����¼�

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLogic.Battle;
using JetBrains.Annotations;


[RequireComponent(typeof(Collider))]
public class NPC_Interaction : MonoBehaviour
{
    public int interactnpcid;

    public NPC_BehaviorCtrl npc_BehaviorCtrl;
    public PlayerCtrl playerCtrl;
    private Material _material;

    private void Awake()
    {
        if (npc_BehaviorCtrl == null)
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
            Debug.LogError("�Ҳ���npc��Ϊ������", gameObject);
        }
    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space) && interactnpcid == 1001)
        {
            ;

            GameObject go = GameObject.FindWithTag("Player");
            // ��ȡScriptA���
            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
            // ����ScriptA�е�hp����
            //scriptA.disablewalk = true;
            BattleController.Inst.StartBattle(1001, 10,
                () =>
            {
                GameObject.Destroy(GameObject.FindWithTag("door1"));
                SimpleAudioManager.Instance.PlaySound("door");
                GameObject go = GameObject.FindWithTag("Player");
                // ��ȡScriptA���
                PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                // ����ScriptA�е�hp����
                //scriptA.disablewalk = false;
                GameObject shit = GameObject.Find("shit");
                ShitAnimController ShitAnimController = shit.GetComponent<ShitAnimController>();

                ShitAnimController.PlayAnim("Die");
                //GameObject parentObj = GameObject.Find("Canvas");
                //parentObj.transform.Find("BOOMVideo").gameObject.SetActive(true);
            }, () =>
            {
                GameObject go = GameObject.FindWithTag("Player");
                // ��ȡScriptA���
                PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                // ����ScriptA�е�hp����
                //scriptA.disablewalk = false;
            }
        );

        };
        if (Input.GetKeyDown(KeyCode.Space) && interactnpcid == 1002)
        {
            ;

            GameObject go = GameObject.FindWithTag("Player");
            // ��ȡScriptA���
            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
            // ����ScriptA�е�hp����
            //scriptA.disablewalk = true;
            BattleController.Inst.StartBattle(1001, 10,
                () =>
                {
                    GameObject go = GameObject.FindWithTag("Player");
                    // ��ȡScriptA���
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // ����ScriptA�е�hp����
                   // scriptA.disablewalk = false;
                    //GameObject parentObj = GameObject.Find("Canvas");
                    //parentObj.transform.Find("BOOMVideo").gameObject.SetActive(true);
                }, () =>
                {
                    GameObject go = GameObject.FindWithTag("Player");
                    // ��ȡScriptA���
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // ����ScriptA�е�hp����
                    //scriptA.disablewalk = false;
                }
        );

        };
        if (Input.GetKeyDown(KeyCode.Space) && interactnpcid == 1004)
        {
            ;

            GameObject go = GameObject.FindWithTag("Player");
            // ��ȡScriptA���
            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
            // ����ScriptA�е�hp����
            //scriptA.disablewalk = true;
            BattleController.Inst.StartBattle(1002, 10,
                () =>
                {
                    GameObject.Destroy(GameObject.FindWithTag("door2"));
                    GameObject go = GameObject.FindWithTag("Player");
                    // ��ȡScriptA���
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // ����ScriptA�е�hp����
                    //scriptA.disablewalk = false;
                    GameObject npc4 = GameObject.Find("npc4");
                    Npc4AnimController npc4AnimController = npc4.GetComponent<Npc4AnimController>();

                    npc4AnimController.PlayAnim("Cry");
                    //GameObject parentObj = GameObject.Find("Canvas");
                    //parentObj.transform.Find("BOOMVideo").gameObject.SetActive(true);
                }, () =>
                {
                    GameObject go = GameObject.FindWithTag("Player");
                    // ��ȡScriptA���
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // ����ScriptA�е�hp����
                    //scriptA.disablewalk = false;
                }
        );

        };
        if (Input.GetKeyDown(KeyCode.Space) && interactnpcid == 1005)
        {
            ;

            GameObject go = GameObject.FindWithTag("Player");
            // ��ȡScriptA���
            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
            // ����ScriptA�е�hp����
            //scriptA.disablewalk = true;
            BattleController.Inst.StartBattle(1003, 10,
                () =>
                {
                    GameObject.Destroy(GameObject.FindWithTag("door3"));
                    GameObject go = GameObject.FindWithTag("Player");
                    // ��ȡScriptA���
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // ����ScriptA�е�hp����
                    //scriptA.disablewalk = false;
                    GameObject npc5 = GameObject.Find("npc5");
                    Npc5AnimController npc5AnimController = npc5.GetComponent<Npc5AnimController>();
                    GameObject parentObj = GameObject.Find("NPC");
                    parentObj.transform.Find("boss").gameObject.SetActive(true);

                    npc5AnimController.PlayAnim("Frown");
                    //GameObject parentObj = GameObject.Find("Canvas");
                    //parentObj.transform.Find("BOOMVideo").gameObject.SetActive(true);
                }, () =>
                {
                    GameObject go = GameObject.FindWithTag("Player");
                    // ��ȡScriptA���
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // ����ScriptA�е�hp����
                    //scriptA.disablewalk = false;
                }
        );

        };
        if (Input.GetKeyDown(KeyCode.Space) && interactnpcid == 1006)
        {
            ;

            GameObject go = GameObject.FindWithTag("Player");
            // ��ȡScriptA���
            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
            // ����ScriptA�е�hp����
            //scriptA.disablewalk = true;
            BattleController.Inst.StartBattle(1006, 10,
                () =>
                {
                    GameObject parentObj = GameObject.Find("Canvas");
                    parentObj.transform.Find("BOOMVideo").gameObject.SetActive(true);
                    GameObject.Destroy(GameObject.FindWithTag("door1"));
                    SimpleAudioManager.Instance.PlaySound("door");
                    GameObject go = GameObject.FindWithTag("Player");
                    // ��ȡScriptA���
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // ����ScriptA�е�hp����
                   // scriptA.disablewalk = false;
                    //GameObject shit = GameObject.Find("shit");
                    //ShitAnimController ShitAnimController = shit.GetComponent<ShitAnimController>();

                    //ShitAnimController.PlayAnim("Die");

                }, () =>
                {
                    GameObject go = GameObject.FindWithTag("Player");
                    // ��ȡScriptA���
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // ����ScriptA�е�hp����
                   // scriptA.disablewalk = false;
                }
        );

        };
        //if (Input.GetKeyDown(KeyCode.Space) && interactnpcid == 1001)
        //{
        //    ;

        //    GameObject go = GameObject.FindWithTag("Player");
        //    // ��ȡScriptA���
        //    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
        //    // ����ScriptA�е�hp����
        //    scriptA.disablewalk = true;
        //    BattleController.Inst.StartBattle(1001, 10,
        //        () =>
        //        {
        //            GameObject.Destroy(GameObject.FindWithTag("door1"));
        //            GameObject go = GameObject.FindWithTag("Player");
        //            // ��ȡScriptA���
        //            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
        //            // ����ScriptA�е�hp����
        //            scriptA.disablewalk = false;
        //            GameObject shit = GameObject.Find("shit");
        //            ShitAnimController ShitAnimController = shit.GetComponent<ShitAnimController>();

        //            ShitAnimController.PlayAnim("Die");
        //            //GameObject parentObj = GameObject.Find("Canvas");
        //            //parentObj.transform.Find("BOOMVideo").gameObject.SetActive(true);
        //        }, () =>
        //        {
        //            GameObject go = GameObject.FindWithTag("Player");
        //            // ��ȡScriptA���
        //            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
        //            // ����ScriptA�е�hp����
        //            scriptA.disablewalk = false;
        //        }
        //);

        //};
        //if (Input.GetKeyDown(KeyCode.Space) && interactnpcid == 1001)
        //{
        //    ;

        //    GameObject go = GameObject.FindWithTag("Player");
        //    // ��ȡScriptA���
        //    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
        //    // ����ScriptA�е�hp����
        //    scriptA.disablewalk = true;
        //    BattleController.Inst.StartBattle(1001, 10,
        //        () =>
        //        {
        //            GameObject.Destroy(GameObject.FindWithTag("door1"));
        //            GameObject go = GameObject.FindWithTag("Player");
        //            // ��ȡScriptA���
        //            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
        //            // ����ScriptA�е�hp����
        //            scriptA.disablewalk = false;
        //            GameObject shit = GameObject.Find("shit");
        //            ShitAnimController ShitAnimController = shit.GetComponent<ShitAnimController>();

        //            ShitAnimController.PlayAnim("Die");
        //            //GameObject parentObj = GameObject.Find("Canvas");
        //            //parentObj.transform.Find("BOOMVideo").gameObject.SetActive(true);
        //        }, () =>
        //        {
        //            GameObject go = GameObject.FindWithTag("Player");
        //            // ��ȡScriptA���
        //            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
        //            // ����ScriptA�е�hp����
        //            scriptA.disablewalk = false;
        //        }
        //);

        //};
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ����NPC�����߼�
            Debug.Log($"��NPC {npc_BehaviorCtrl.npcID} ����");
            
            Debug.Log("[NPC_Interaction] 即将播放交互音效");
            if (SimpleAudioManager.Instance == null)
            {
                Debug.LogError("[NPC_Interaction] SimpleAudioManager.Instance 为 null!");
            }
            else
            {
                Debug.Log("[NPC_Interaction] SimpleAudioManager.Instance 存在，尝试播放 interact 音效");
                SimpleAudioManager.Instance.PlaySound("interact");
            }
            
            interactnpcid = npc_BehaviorCtrl.npcID;
            GameObject parentObj = GameObject.FindWithTag("Player");
            parentObj?.transform.Find("space").gameObject.SetActive(true);
            // �������������������������������Ľ����߼�
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactnpcid = 0;
            // ����NPC�����߼�
            Debug.Log($"������NPC {npc_BehaviorCtrl.npcID} �Ľ���");
            GameObject.FindWithTag("INTERACT").SetActive(false);
            // �������������������������������Ľ��������߼�
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

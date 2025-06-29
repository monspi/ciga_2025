///npc交互逻辑响应脚本，用于处理NPC与玩家的交互事件

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
            Reset(); // 如果没有手动设置npc_BehaviorCtrl，则尝试从父物体获取
            _material =  npc_BehaviorCtrl.GetComponent<Renderer>().material;
        }
    }

    private void Reset()
    {
        npc_BehaviorCtrl = GetComponentInParent<NPC_BehaviorCtrl>();
        if (npc_BehaviorCtrl == null)
        {
            Debug.LogError("找不到npc行为控制器", gameObject);
        }
    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space) && interactnpcid == 1001)
        {
            ;

            GameObject go = GameObject.FindWithTag("Player");
            // 获取ScriptA组件
            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
            // 访问ScriptA中的hp变量
            //scriptA.disablewalk = true;
            BattleController.Inst.StartBattle(1001, 10,
                () =>
            {
                GameObject.Destroy(GameObject.FindWithTag("door1"));
                GameObject go = GameObject.FindWithTag("Player");
                // 获取ScriptA组件
                PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                // 访问ScriptA中的hp变量
                //scriptA.disablewalk = false;
                GameObject shit = GameObject.Find("shit");
                ShitAnimController ShitAnimController = shit.GetComponent<ShitAnimController>();

                ShitAnimController.PlayAnim("Die");
                //GameObject parentObj = GameObject.Find("Canvas");
                //parentObj.transform.Find("BOOMVideo").gameObject.SetActive(true);
            }, () =>
            {
                GameObject go = GameObject.FindWithTag("Player");
                // 获取ScriptA组件
                PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                // 访问ScriptA中的hp变量
                //scriptA.disablewalk = false;
            }
        );

        };
        if (Input.GetKeyDown(KeyCode.Space) && interactnpcid == 1002)
        {
            ;

            GameObject go = GameObject.FindWithTag("Player");
            // 获取ScriptA组件
            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
            // 访问ScriptA中的hp变量
            //scriptA.disablewalk = true;
            BattleController.Inst.StartBattle(1001, 10,
                () =>
                {
                    GameObject go = GameObject.FindWithTag("Player");
                    // 获取ScriptA组件
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // 访问ScriptA中的hp变量
                   // scriptA.disablewalk = false;
                    //GameObject parentObj = GameObject.Find("Canvas");
                    //parentObj.transform.Find("BOOMVideo").gameObject.SetActive(true);
                }, () =>
                {
                    GameObject go = GameObject.FindWithTag("Player");
                    // 获取ScriptA组件
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // 访问ScriptA中的hp变量
                    //scriptA.disablewalk = false;
                }
        );

        };
        if (Input.GetKeyDown(KeyCode.Space) && interactnpcid == 1004)
        {
            ;

            GameObject go = GameObject.FindWithTag("Player");
            // 获取ScriptA组件
            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
            // 访问ScriptA中的hp变量
            //scriptA.disablewalk = true;
            BattleController.Inst.StartBattle(1001, 10,
                () =>
                {
                    GameObject.Destroy(GameObject.FindWithTag("door2"));
                    GameObject go = GameObject.FindWithTag("Player");
                    // 获取ScriptA组件
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // 访问ScriptA中的hp变量
                    //scriptA.disablewalk = false;
                    GameObject npc4 = GameObject.Find("npc4");
                    Npc4AnimController npc4AnimController = npc4.GetComponent<Npc4AnimController>();

                    npc4AnimController.PlayAnim("Cry");
                    //GameObject parentObj = GameObject.Find("Canvas");
                    //parentObj.transform.Find("BOOMVideo").gameObject.SetActive(true);
                }, () =>
                {
                    GameObject go = GameObject.FindWithTag("Player");
                    // 获取ScriptA组件
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // 访问ScriptA中的hp变量
                    //scriptA.disablewalk = false;
                }
        );

        };
        if (Input.GetKeyDown(KeyCode.Space) && interactnpcid == 1005)
        {
            ;

            GameObject go = GameObject.FindWithTag("Player");
            // 获取ScriptA组件
            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
            // 访问ScriptA中的hp变量
            //scriptA.disablewalk = true;
            BattleController.Inst.StartBattle(1001, 10,
                () =>
                {
                    GameObject.Destroy(GameObject.FindWithTag("door3"));
                    GameObject go = GameObject.FindWithTag("Player");
                    // 获取ScriptA组件
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // 访问ScriptA中的hp变量
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
                    // 获取ScriptA组件
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // 访问ScriptA中的hp变量
                    //scriptA.disablewalk = false;
                }
        );

        };
        if (Input.GetKeyDown(KeyCode.Space) && interactnpcid == 1006)
        {
            ;

            GameObject go = GameObject.FindWithTag("Player");
            // 获取ScriptA组件
            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
            // 访问ScriptA中的hp变量
            //scriptA.disablewalk = true;
            BattleController.Inst.StartBattle(1001, 10,
                () =>
                {
                    GameObject.Destroy(GameObject.FindWithTag("door1"));
                    GameObject go = GameObject.FindWithTag("Player");
                    // 获取ScriptA组件
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // 访问ScriptA中的hp变量
                   // scriptA.disablewalk = false;
                    //GameObject shit = GameObject.Find("shit");
                    //ShitAnimController ShitAnimController = shit.GetComponent<ShitAnimController>();

                    //ShitAnimController.PlayAnim("Die");

                }, () =>
                {
                    GameObject go = GameObject.FindWithTag("Player");
                    // 获取ScriptA组件
                    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
                    // 访问ScriptA中的hp变量
                   // scriptA.disablewalk = false;
                }
        );

        };
        //if (Input.GetKeyDown(KeyCode.Space) && interactnpcid == 1001)
        //{
        //    ;

        //    GameObject go = GameObject.FindWithTag("Player");
        //    // 获取ScriptA组件
        //    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
        //    // 访问ScriptA中的hp变量
        //    scriptA.disablewalk = true;
        //    BattleController.Inst.StartBattle(1001, 10,
        //        () =>
        //        {
        //            GameObject.Destroy(GameObject.FindWithTag("door1"));
        //            GameObject go = GameObject.FindWithTag("Player");
        //            // 获取ScriptA组件
        //            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
        //            // 访问ScriptA中的hp变量
        //            scriptA.disablewalk = false;
        //            GameObject shit = GameObject.Find("shit");
        //            ShitAnimController ShitAnimController = shit.GetComponent<ShitAnimController>();

        //            ShitAnimController.PlayAnim("Die");
        //            //GameObject parentObj = GameObject.Find("Canvas");
        //            //parentObj.transform.Find("BOOMVideo").gameObject.SetActive(true);
        //        }, () =>
        //        {
        //            GameObject go = GameObject.FindWithTag("Player");
        //            // 获取ScriptA组件
        //            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
        //            // 访问ScriptA中的hp变量
        //            scriptA.disablewalk = false;
        //        }
        //);

        //};
        //if (Input.GetKeyDown(KeyCode.Space) && interactnpcid == 1001)
        //{
        //    ;

        //    GameObject go = GameObject.FindWithTag("Player");
        //    // 获取ScriptA组件
        //    PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
        //    // 访问ScriptA中的hp变量
        //    scriptA.disablewalk = true;
        //    BattleController.Inst.StartBattle(1001, 10,
        //        () =>
        //        {
        //            GameObject.Destroy(GameObject.FindWithTag("door1"));
        //            GameObject go = GameObject.FindWithTag("Player");
        //            // 获取ScriptA组件
        //            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
        //            // 访问ScriptA中的hp变量
        //            scriptA.disablewalk = false;
        //            GameObject shit = GameObject.Find("shit");
        //            ShitAnimController ShitAnimController = shit.GetComponent<ShitAnimController>();

        //            ShitAnimController.PlayAnim("Die");
        //            //GameObject parentObj = GameObject.Find("Canvas");
        //            //parentObj.transform.Find("BOOMVideo").gameObject.SetActive(true);
        //        }, () =>
        //        {
        //            GameObject go = GameObject.FindWithTag("Player");
        //            // 获取ScriptA组件
        //            PlayerCtrl scriptA = go.GetComponent<PlayerCtrl>();
        //            // 访问ScriptA中的hp变量
        //            scriptA.disablewalk = false;
        //        }
        //);

        //};
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 触发NPC交互逻辑
            Debug.Log($"与NPC {npc_BehaviorCtrl.npcID} 交互");
            interactnpcid = npc_BehaviorCtrl.npcID;
            GameObject parentObj = GameObject.FindWithTag("Player");
            parentObj?.transform.Find("space").gameObject.SetActive(true);
            // 可以在这里调用其他方法来处理具体的交互逻辑
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactnpcid = 0;
            // 结束NPC交互逻辑
            Debug.Log($"结束与NPC {npc_BehaviorCtrl.npcID} 的交互");
            GameObject.FindWithTag("INTERACT").SetActive(false);
            // 可以在这里调用其他方法来处理具体的交互结束逻辑
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

using System;
using UnityEngine;

namespace GameLogic.Battle
{
    public class BattleUnitTest : MonoBehaviour
    {
        private void Awake()
        {
            BattleController.Inst.StartBattle(1, 3, () =>
            {
                Debug.Log("Success");
            }, () =>
            {
                Debug.Log("Fail");
            });
        }
    }
}
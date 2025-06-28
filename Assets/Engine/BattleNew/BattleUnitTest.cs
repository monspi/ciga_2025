using System;
using UnityEngine;

namespace GameLogic.Battle
{
    public class BattleUnitTest : MonoBehaviour
    {
        private void Awake()
        {
            BattleController.Inst.StartBattle();
        }
    }
}
using UnityEngine;

namespace GameLogic.Battle
{
    public class BattleController : MonoBehaviour
    {
        public static BattleController Inst
        {
            get
            {
                if (_inst == null)
                {
                    _inst = FindObjectOfType<BattleController>();
                }

                if (_inst == null)
                {
                    var go = new GameObject("[BattleController]");
                    _inst = go.AddComponent<BattleController>();
                }

                return _inst;
            }
        }

        private static BattleController _inst;

        public float MoveDuration;

        [SerializeField] private BattleUIWindow _uiWindow;

        public void StartBattle()
        {
            _uiWindow.Show();
        }
    }
}
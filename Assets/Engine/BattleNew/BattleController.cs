using UnityEngine;
using System;
using SonicBloom.Koreo;
using UnityEditor.PackageManager;

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

        private bool _inBattle;
        
        public void StartBattle(int songId, int missFailCount, Action onSuccess = null, Action onFail = null)
        {
            _uiWindow.Show(songId, missFailCount, onSuccess, onFail);
        }

        public Koreography GetGraphyByID(int id)
        {
            return Resources.Load<Koreography>("Koreography/k" + id);
        }

        public AudioClip GetAudioClipByID(int id)
        {
            return Resources.Load<AudioClip>("Music/m" + id);
        }
    }
}
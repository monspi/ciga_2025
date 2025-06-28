using System;
using DG.Tweening;
using UnityEngine;

namespace GameLogic.Battle
{
    public class Miss : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            transform.DOLocalMoveY(-300, 0.5f);
            _canvasGroup.DOFade(0, 0.5f);
            Invoke(nameof(DestroyFunc), 2f);
        }

        private void DestroyFunc()
        {
            Destroy(gameObject);
        }
    }
}
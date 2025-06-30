
using System;
using DG.Tweening;
using UnityEngine;

namespace GameLogic.Battle
{
    public class NoteBehavior : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private Transform _missParent;
        private Vector2 _velocity;
        private Vector2 _startPos;
        private float _t;
        private float _moveDuration;
        private bool _isMiss;
        private bool _isHit;
        private bool _invokeDestroyFlag;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void StartMove(Vector2 startPos, Vector2 endPos, float moveDuration)
        {
            _startPos = startPos;
            _velocity = (endPos - startPos) / moveDuration;
            _t = 0;
            _moveDuration = moveDuration;
            transform.position = startPos;
        }

        public virtual void OnMiss()
        {
            _isMiss = true;
            _canvasGroup.DOFade(0, 0.2f);
        }

        public virtual void OnBeginHit()
        {
            _isHit = true;
        }

        protected virtual void Update()
        {
            _t += Time.deltaTime;

            if (_t > _moveDuration)
            {
                transform.position = CalculatePosition(_moveDuration);
            }
            
            if (!_isMiss && !_isHit)
            {
                if (_t <= _moveDuration)
                {
                    transform.position = CalculatePosition(_t);
                }
            }
            else if (!_invokeDestroyFlag)
            {
                _invokeDestroyFlag = true;
                Invoke(nameof(DestroyFunc), 0.2f);
            }
        }

        private Vector2 CalculatePosition(float t)
        {
            Vector2 displacement = _velocity * t;
            return _startPos + displacement;
        }

        private void DestroyFunc()
        {
            Destroy(gameObject);
        }
    }
}
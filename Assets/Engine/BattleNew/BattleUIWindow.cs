
using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using SonicBloom.Koreo;

namespace GameLogic.Battle
{
    public class BattleUIWindow : MonoBehaviour
    {
        public GameObject missPF;
        public Transform missParent;
        
        private Koreography graphy;
        private AudioClip clip;

        public Animator _animatorA;
        public Animator _animatorD;
        public Animator _animatorNose;

        //判定范围
        public float detectOffset = 0.1f;

        //铺面偏移
        public float timeOffset = 0f;

        private AudioSource _source;
        private NoteBehaviorGenerator _generator;

        private bool _isStart;

        private List<NoteEvent> _noteEventsLeft;
        private List<NoteEvent> _noteEventsRight;

        private int _leftEmitIndex;
        private int _rightEmitIndex;

        private List<NoteBehavior> _noteBehaviorsLeft;
        private List<NoteBehavior> _noteBehaviorsRight;

        private int _leftDetectIndex;
        private int _rightDetectIndex;

        private int _maxMissCount;
        private int _curMissCount;
        private Action _onSuccess;
        private Action _onFail;
        private bool _finish;
        
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            // Init();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private bool _isInit;
        private void Init()
        {
            _isInit = true;
            
            _source = GetComponent<AudioSource>();
            _generator = GetComponentInChildren<NoteBehaviorGenerator>();

            TransferEvents();

            _curMissCount = 0;
            _leftDetectIndex = 0;
            _rightDetectIndex = 0;
            _leftEmitIndex = 0;
            _rightEmitIndex = 0;
            _finish = false;
        }

        private void TransferEvents()
        {
            var track = graphy.GetTrackAtIndex(0);
            var events = track.GetAllEvents();
            _noteEventsLeft = new List<NoteEvent>(events.Count);
            _noteBehaviorsLeft =  new List<NoteBehavior>(events.Count);

            int sampleRate = graphy.SampleRate;

            for (int i = 0; i < events.Count; ++i)
            {
                float emitTime = 1.0f * events[i].StartSample / sampleRate - BattleController.Inst.MoveDuration;
                _noteEventsLeft.Add(new NoteEvent()
                {
                    emitTime = emitTime,
                    detectTime = emitTime + BattleController.Inst.MoveDuration
                });
            }

            track = graphy.GetTrackAtIndex(1);
            events = track.GetAllEvents();
            _noteEventsRight = new List<NoteEvent>(events.Count);
            _noteBehaviorsRight = new List<NoteBehavior>(events.Count);

            for (int i = 0; i < events.Count; ++i)
            {
                float emitTime = 1.0f * events[i].StartSample / sampleRate - BattleController.Inst.MoveDuration;
                _noteEventsRight.Add(new NoteEvent()
                {
                    emitTime = emitTime,
                    detectTime = emitTime + BattleController.Inst.MoveDuration
                });
            }
        }
        
        public void Show(int songId, int missFailCount, Action onSuccess, Action onFail)
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 0;
            _canvasGroup.DOFade(1, 0.3f);
            
            graphy = BattleController.Inst.GetGraphyByID(songId);
            clip = BattleController.Inst.GetAudioClipByID(songId);
            _maxMissCount = missFailCount;
            _onSuccess = onSuccess;
            _onFail = onFail;
            gameObject.SetActive(true);
            Invoke(nameof(StartPlay), 0.5f);
        }

        private void StartPlay()
        {
            Init();
            _source.clip = clip;
            _source.Play();
            _isStart = true;
        }

        private void Update()
        {
            if (!_isStart || _finish) return;

            float time = _source.time + timeOffset;

            HandleEmitEvent(time);
            HandleInput(time);

            if (!_source.isPlaying)
            {
                Hide(true);
                _finish = true;
            }
        }

        #region 触发逻辑

        private void HandleEmitEvent(float time)
        {
            while (_leftEmitIndex < _noteEventsLeft.Count && time >= _noteEventsLeft[_leftEmitIndex].emitTime)
            {
                _noteBehaviorsLeft.Add(_generator.GenerateNoteBehavior(_noteEventsLeft[_leftEmitIndex], true));
                _leftEmitIndex++;
            }

            while (_rightEmitIndex < _noteEventsRight.Count && time >= _noteEventsRight[_rightEmitIndex].emitTime)
            {
                _noteBehaviorsRight.Add(_generator.GenerateNoteBehavior(_noteEventsRight[_rightEmitIndex], false));
                _rightEmitIndex++;
            }
        }

        #endregion

        #region 判定逻辑

        private void HandleInput(float time)
        {
            _HandleInput(time, ref _leftDetectIndex, true);
            _HandleInput(time, ref _rightDetectIndex, false);
        }

        private void _HandleInput(float time, ref int detectIndex, bool isLeft)
        {
            var keyCode = isLeft ? KeyCode.A : KeyCode.D;
            var noteEvents = isLeft ? _noteEventsLeft : _noteEventsRight;
            var noteBehaviors = isLeft ? _noteBehaviorsLeft : _noteBehaviorsRight;
            var animator = isLeft ? _animatorA : _animatorD;
            
            //处理miss音符
            while (detectIndex < noteEvents.Count)
            {
                var beatEvent = noteEvents[detectIndex];
                if (time - beatEvent.detectTime > detectOffset)
                {
                    if (detectIndex < noteBehaviors.Count)
                    {
                        var behavior = noteBehaviors[detectIndex];
                        behavior.OnMiss();
                        var go = Instantiate(missPF, missParent);
                        Vector2 position = isLeft ? _generator.leftDetectTF.position : _generator.rightDetectTF.position;
                        go.transform.position = position;
                        _curMissCount++;
                        _animatorNose.Play("noseInhale");
                        if (_curMissCount >= _maxMissCount)
                        {
                            Hide(false);
                        }
                    }

                    detectIndex++;
                }
                else break;
            }

            if (Input.GetKeyDown(keyCode))
            {
                //hit判定
                if (detectIndex < noteBehaviors.Count)
                {
                    var beatEvent = noteEvents[detectIndex];
                    if (Math.Abs(time - beatEvent.detectTime) <= detectOffset)
                    {
                        var behavior = noteBehaviors[detectIndex];
                        behavior.OnBeginHit();
                        detectIndex++;
                    }
                }
                animator.Play("Down");
            }
        }

        #endregion

        private void Hide(bool success)
        {
            if (_finish) return;
            
            _finish = true;
            _isStart = false;
            
            if (success)
            {
                _onSuccess?.Invoke();
            }
            else
            {
                _onFail?.Invoke();
            }
            _source.Stop();
            
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(0.5f);
            seq.Append(_canvasGroup.DOFade(0, 0.5f));
            seq.AppendCallback(() =>
            {
                
            });
        }
    }
}

using System;
using UnityEngine;
using System.Collections.Generic;
using SonicBloom.Koreo;

namespace GameLogic.Battle
{
    public class BattleUIWindow : MonoBehaviour
    {
        public GameObject piPF;
        public Koreography graphy;

        public AudioClip clip;

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

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _generator = GetComponentInChildren<NoteBehaviorGenerator>();
            
            TransferEvents();
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

        public void Show()
        {
            gameObject.SetActive(true);
            StartPlay();
        }

        private void StartPlay()
        {
            _source.clip = clip;
            _source.Play();
            _isStart = true;
        }

        private void Update()
        {
            if (!_isStart) return;

            float time = _source.time + timeOffset;

            HandleEmitEvent(time);
            HandleInput(time);
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
                _noteBehaviorsRight.Add(_generator.GenerateNoteBehavior(_noteEventsLeft[_rightEmitIndex], false));
                _rightEmitIndex++;
            }
        }

        #endregion

        #region 判定逻辑

        private void HandleInput(float time)
        {
            _HandleInput(time, KeyCode.A, ref _leftDetectIndex, _noteEventsLeft, _noteBehaviorsLeft);
            _HandleInput(time, KeyCode.D, ref _rightDetectIndex, _noteEventsRight, _noteBehaviorsRight);
        }

        private void _HandleInput(float time, KeyCode keyCode, ref int detectIndex, List<NoteEvent> noteEvents,
            List<NoteBehavior> noteBehaviors)
        {
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
            }
        }

        #endregion
    }
}
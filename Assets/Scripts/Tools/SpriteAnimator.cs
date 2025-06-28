using UnityEngine;
using System.Collections.Generic;
using System;

namespace Tools
{
    [System.Serializable]
    public class SpriteAnimation
    {
        [Header("动画基本设置")]
        public string name = "New Animation";
        public Sprite[] frames;
        
        [Header("播放设置")]
        [Range(1f, 60f)]
        public float frameRate = 12f;
        public bool loop = true;
        
        [Header("高级设置")]
        [Tooltip("动画结束后的回调")]
        public UnityEngine.Events.UnityEvent onAnimationComplete;
        
        [Tooltip("每帧播放时的回调")]
        public UnityEngine.Events.UnityEvent onFrameChanged;
        
        public bool IsValid => frames != null && frames.Length > 0;
        public int FrameCount => frames?.Length ?? 0;
        public float Duration => FrameCount > 0 ? FrameCount / frameRate : 0f;
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : MonoBehaviour
    {
        [Header("动画列表")]
        [SerializeField] private SpriteAnimation[] animations = new SpriteAnimation[0];
        
        [Header("播放控制")]
        [SerializeField] private string defaultAnimation = "";
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool randomStartFrame = false;
        
        [Header("调试信息")]
        [SerializeField] private bool showDebugInfo = false;
        
        // 组件引用
        private SpriteRenderer spriteRenderer;
        
        // 动画状态
        private SpriteAnimation currentAnimation;
        private int currentFrame;
        private float timer;
        private bool isPlaying = false;
        private bool isPaused = false;
        
        // 事件系统
        public event Action<string> OnAnimationStarted;
        public event Action<string> OnAnimationCompleted;
        public event Action<string, int> OnFrameChanged;
        
        // 属性
        public string CurrentAnimationName => currentAnimation?.name ?? "";
        public bool IsPlaying => isPlaying && !isPaused;
        public bool IsPaused => isPaused;
        public int CurrentFrame => currentFrame;
        public float Progress => currentAnimation?.FrameCount > 0 ? (float)currentFrame / currentAnimation.FrameCount : 0f;
        public SpriteAnimation CurrentAnimation => currentAnimation;
        
        #region Unity生命周期
        
        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            ValidateAnimations();
        }
        
        void Start()
        {
            if (playOnStart && !string.IsNullOrEmpty(defaultAnimation))
            {
                PlayAnimation(defaultAnimation);
            }
        }
        
        void Update()
        {
            if (!isPlaying || isPaused || currentAnimation == null) return;
            
            timer += Time.deltaTime;
            
            if (timer >= 1f / currentAnimation.frameRate)
            {
                timer = 0f;
                NextFrame();
            }
        }
        
        void OnValidate()
        {
            ValidateAnimations();
        }
        
        #endregion
        
        #region 动画播放控制
        
        /// <summary>
        /// 播放指定动画
        /// </summary>
        public bool PlayAnimation(string animationName, bool forceRestart = false)
        {
            SpriteAnimation targetAnimation = GetAnimation(animationName);
            
            if (targetAnimation == null)
            {
                if (showDebugInfo)
                    Debug.LogWarning($"[SpriteAnimator] 找不到动画: {animationName}", this);
                return false;
            }
            
            if (!targetAnimation.IsValid)
            {
                if (showDebugInfo)
                    Debug.LogWarning($"[SpriteAnimator] 动画无效: {animationName}", this);
                return false;
            }
            
            // 如果已经在播放相同动画且不强制重启，则返回
            if (currentAnimation == targetAnimation && isPlaying && !forceRestart)
                return true;
            
            // 设置新动画
            currentAnimation = targetAnimation;
            currentFrame = randomStartFrame ? UnityEngine.Random.Range(0, currentAnimation.FrameCount) : 0;
            timer = 0f;
            isPlaying = true;
            isPaused = false;
            
            // 设置第一帧
            UpdateSprite();
            
            // 触发事件
            OnAnimationStarted?.Invoke(animationName);
            
            if (showDebugInfo)
                Debug.Log($"[SpriteAnimator] 开始播放动画: {animationName}", this);
            
            return true;
        }
        
        /// <summary>
        /// 停止当前动画
        /// </summary>
        public void Stop()
        {
            if (currentAnimation != null && isPlaying)
            {
                string animName = currentAnimation.name;
                isPlaying = false;
                isPaused = false;
                currentFrame = 0;
                timer = 0f;
                
                UpdateSprite();
                
                if (showDebugInfo)
                    Debug.Log($"[SpriteAnimator] 停止动画: {animName}", this);
            }
        }
        
        /// <summary>
        /// 暂停当前动画
        /// </summary>
        public void Pause()
        {
            if (isPlaying && !isPaused)
            {
                isPaused = true;
                
                if (showDebugInfo)
                    Debug.Log($"[SpriteAnimator] 暂停动画: {currentAnimation?.name}", this);
            }
        }
        
        /// <summary>
        /// 恢复暂停的动画
        /// </summary>
        public void Resume()
        {
            if (isPlaying && isPaused)
            {
                isPaused = false;
                
                if (showDebugInfo)
                    Debug.Log($"[SpriteAnimator] 恢复动画: {currentAnimation?.name}", this);
            }
        }
        
        /// <summary>
        /// 设置动画到指定帧
        /// </summary>
        public void SetFrame(int frameIndex)
        {
            if (currentAnimation == null || !currentAnimation.IsValid) return;
            
            currentFrame = Mathf.Clamp(frameIndex, 0, currentAnimation.FrameCount - 1);
            UpdateSprite();
            
            // 触发帧变化事件
            OnFrameChanged?.Invoke(currentAnimation.name, currentFrame);
            currentAnimation.onFrameChanged?.Invoke();
        }
        
        #endregion
        
        #region 私有方法
        
        private void NextFrame()
        {
            if (currentAnimation == null || !currentAnimation.IsValid) return;
            
            currentFrame++;
            
            // 检查是否到达动画结尾
            if (currentFrame >= currentAnimation.FrameCount)
            {
                if (currentAnimation.loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = currentAnimation.FrameCount - 1;
                    isPlaying = false;
                    
                    // 触发动画完成事件
                    string animName = currentAnimation.name;
                    OnAnimationCompleted?.Invoke(animName);
                    currentAnimation.onAnimationComplete?.Invoke();
                    
                    if (showDebugInfo)
                        Debug.Log($"[SpriteAnimator] 动画播放完成: {animName}", this);
                    
                    return;
                }
            }
            
            UpdateSprite();
            
            // 触发帧变化事件
            OnFrameChanged?.Invoke(currentAnimation.name, currentFrame);
            currentAnimation.onFrameChanged?.Invoke();
        }
        
        private void UpdateSprite()
        {
            if (currentAnimation == null || !currentAnimation.IsValid || spriteRenderer == null) return;
            
            if (currentFrame >= 0 && currentFrame < currentAnimation.FrameCount)
            {
                spriteRenderer.sprite = currentAnimation.frames[currentFrame];
            }
        }
        
        private SpriteAnimation GetAnimation(string animationName)
        {
            if (string.IsNullOrEmpty(animationName)) return null;
            
            return Array.Find(animations, anim => anim.name == animationName);
        }
        
        private void ValidateAnimations()
        {
            if (animations == null) return;
            
            for (int i = 0; i < animations.Length; i++)
            {
                if (animations[i] == null) continue;
                
                // 确保动画名称不为空
                if (string.IsNullOrEmpty(animations[i].name))
                {
                    animations[i].name = $"Animation_{i}";
                }
                
                // 验证帧率
                if (animations[i].frameRate <= 0)
                {
                    animations[i].frameRate = 12f;
                }
            }
        }
        
        #endregion
        
        #region 公共查询方法
        
        /// <summary>
        /// 检查是否有指定名称的动画
        /// </summary>
        public bool HasAnimation(string animationName)
        {
            return GetAnimation(animationName) != null;
        }
        
        /// <summary>
        /// 检查指定动画是否正在播放
        /// </summary>
        public bool IsAnimationPlaying(string animationName)
        {
            return currentAnimation != null && 
                   currentAnimation.name == animationName && 
                   isPlaying && !isPaused;
        }
        
        /// <summary>
        /// 获取所有动画名称
        /// </summary>
        public string[] GetAnimationNames()
        {
            if (animations == null) return new string[0];
            
            List<string> names = new List<string>();
            for (int i = 0; i < animations.Length; i++)
            {
                if (animations[i] != null && !string.IsNullOrEmpty(animations[i].name))
                {
                    names.Add(animations[i].name);
                }
            }
            return names.ToArray();
        }
        
        /// <summary>
        /// 获取指定动画的信息
        /// </summary>
        public SpriteAnimation GetAnimationInfo(string animationName)
        {
            return GetAnimation(animationName);
        }
        
        #endregion
        
        #region 编辑器辅助方法
        
#if UNITY_EDITOR
        [ContextMenu("播放默认动画")]
        private void PlayDefaultAnimation()
        {
            if (!string.IsNullOrEmpty(defaultAnimation))
            {
                PlayAnimation(defaultAnimation, true);
            }
        }
        
        [ContextMenu("停止动画")]
        private void StopAnimation()
        {
            Stop();
        }
        
        [ContextMenu("验证所有动画")]
        private void ValidateAllAnimations()
        {
            ValidateAnimations();
            Debug.Log($"[SpriteAnimator] 已验证 {animations?.Length ?? 0} 个动画", this);
        }
#endif
        
        #endregion
    }
    
    #region 编辑器扩展
    
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SpriteAnimator))]
    public class SpriteAnimatorEditor : UnityEditor.Editor
    {
        private SpriteAnimator animator;
        private bool showRuntimeInfo = true;
        
        void OnEnable()
        {
            animator = (SpriteAnimator)target;
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (!Application.isPlaying) return;
            
            UnityEditor.EditorGUILayout.Space();
            showRuntimeInfo = UnityEditor.EditorGUILayout.Foldout(showRuntimeInfo, "运行时信息");
            
            if (showRuntimeInfo)
            {
                UnityEditor.EditorGUI.BeginDisabledGroup(true);
                
                UnityEditor.EditorGUILayout.TextField("当前动画", animator.CurrentAnimationName);
                UnityEditor.EditorGUILayout.Toggle("正在播放", animator.IsPlaying);
                UnityEditor.EditorGUILayout.Toggle("已暂停", animator.IsPaused);
                UnityEditor.EditorGUILayout.IntField("当前帧", animator.CurrentFrame);
                UnityEditor.EditorGUILayout.Slider("进度", animator.Progress, 0f, 1f);
                
                UnityEditor.EditorGUI.EndDisabledGroup();
                
                UnityEditor.EditorGUILayout.Space();
                
                UnityEditor.EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("暂停"))
                {
                    animator.Pause();
                }
                
                if (GUILayout.Button("恢复"))
                {
                    animator.Resume();
                }
                
                if (GUILayout.Button("停止"))
                {
                    animator.Stop();
                }
                
                UnityEditor.EditorGUILayout.EndHorizontal();
                
                // 显示所有可用动画的快速播放按钮
                string[] animNames = animator.GetAnimationNames();
                if (animNames.Length > 0)
                {
                    UnityEditor.EditorGUILayout.LabelField("快速播放动画:");
                    for (int i = 0; i < animNames.Length; i++)
                    {
                        if (GUILayout.Button(animNames[i]))
                        {
                            animator.PlayAnimation(animNames[i], true);
                        }
                    }
                }
            }
            
            if (GUI.changed)
            {
                UnityEditor.EditorUtility.SetDirty(target);
            }
        }
    }
#endif
    
    #endregion
}

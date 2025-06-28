using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Tools.UI
{
    [System.Serializable]
    public class UIAnimation
    {
        [Header("动画基本设置")]
        public string name = "New UI Animation";
        public Sprite[] frames;
        
        [Header("播放设置")]
        [Range(1f, 60f)]
        public float frameRate = 12f;
        public bool loop = true;
        
        [Header("UI特殊设置")]
        [Tooltip("保持原生尺寸")]
        public bool preserveAspect = true;
        
        [Tooltip("动画结束后的回调")]
        public UnityEngine.Events.UnityEvent onAnimationComplete;
        
        [Tooltip("每帧播放时的回调")]
        public UnityEngine.Events.UnityEvent onFrameChanged;
        
        public bool IsValid => frames != null && frames.Length > 0;
        public int FrameCount => frames?.Length ?? 0;
        public float Duration => FrameCount > 0 ? FrameCount / frameRate : 0f;
    }

    /// <summary>
    /// UI专用的Sprite动画控制器
    /// 适用于Image组件的帧动画播放
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIAnimator : MonoBehaviour
    {
        [Header("UI动画列表")]
        [SerializeField] private UIAnimation[] animations = new UIAnimation[0];
        
        [Header("播放控制")]
        [SerializeField] private string defaultAnimation = "";
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool playOnEnable = false;
        [SerializeField] private bool randomStartFrame = false;
        
        [Header("UI渲染设置")]
        [SerializeField] private bool raycastTarget = true;
        [SerializeField] private bool preserveAspect = true;
        
        [Header("调试信息")]
        [SerializeField] private bool showDebugInfo = false;
        
        // 组件引用
        private Image imageComponent;
        private RectTransform rectTransform;
        
        // 动画状态
        private UIAnimation currentAnimation;
        private int currentFrame;
        private float timer;
        private bool isPlaying = false;
        private bool isPaused = false;
        
        // 原始设置缓存
        private bool originalPreserveAspect;
        private bool originalRaycastTarget;
        
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
        public UIAnimation CurrentAnimation => currentAnimation;
        public Image ImageComponent => imageComponent;
        
        #region Unity生命周期
        
        void Awake()
        {
            imageComponent = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            
            // 缓存原始设置
            originalPreserveAspect = imageComponent.preserveAspect;
            originalRaycastTarget = imageComponent.raycastTarget;
            
            ValidateAnimations();
        }
        
        void Start()
        {
            if (playOnStart && !string.IsNullOrEmpty(defaultAnimation))
            {
                PlayAnimation(defaultAnimation);
            }
        }
        
        void OnEnable()
        {
            if (playOnEnable && !string.IsNullOrEmpty(defaultAnimation))
            {
                PlayAnimation(defaultAnimation);
            }
        }
        
        void Update()
        {
            if (!isPlaying || isPaused || currentAnimation == null) return;
            
            timer += Time.unscaledDeltaTime; // 使用unscaledDeltaTime以避免Time.timeScale影响
            
            if (timer >= 1f / currentAnimation.frameRate)
            {
                timer = 0f;
                NextFrame();
            }
        }
        
        void OnValidate()
        {
            ValidateAnimations();
            UpdateUISettings();
        }
        
        #endregion
        
        #region 动画播放控制
        
        /// <summary>
        /// 播放指定UI动画
        /// </summary>
        public bool PlayAnimation(string animationName, bool forceRestart = false)
        {
            UIAnimation targetAnimation = GetAnimation(animationName);
            
            if (targetAnimation == null)
            {
                if (showDebugInfo)
                    Debug.LogWarning($"[UIAnimator] 找不到UI动画: {animationName}", this);
                return false;
            }
            
            if (!targetAnimation.IsValid)
            {
                if (showDebugInfo)
                    Debug.LogWarning($"[UIAnimator] UI动画无效: {animationName}", this);
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
            
            // 应用动画特定设置
            ApplyAnimationSettings();
            
            // 设置第一帧
            UpdateSprite();
            
            // 触发事件
            OnAnimationStarted?.Invoke(animationName);
            
            if (showDebugInfo)
                Debug.Log($"[UIAnimator] 开始播放UI动画: {animationName}", this);
            
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
                RestoreOriginalSettings();
                
                if (showDebugInfo)
                    Debug.Log($"[UIAnimator] 停止UI动画: {animName}", this);
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
                    Debug.Log($"[UIAnimator] 暂停UI动画: {currentAnimation?.name}", this);
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
                    Debug.Log($"[UIAnimator] 恢复UI动画: {currentAnimation?.name}", this);
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
        
        /// <summary>
        /// 淡入动画
        /// </summary>
        public void FadeIn(float duration = 0.5f)
        {
            if (imageComponent != null)
            {
                StartCoroutine(FadeCoroutine(imageComponent.color.a, 1f, duration));
            }
        }
        
        /// <summary>
        /// 淡出动画
        /// </summary>
        public void FadeOut(float duration = 0.5f)
        {
            if (imageComponent != null)
            {
                StartCoroutine(FadeCoroutine(imageComponent.color.a, 0f, duration));
            }
        }
        
        /// <summary>
        /// 缩放动画
        /// </summary>
        public void ScaleAnimation(Vector3 targetScale, float duration = 0.3f)
        {
            if (rectTransform != null)
            {
                StartCoroutine(ScaleCoroutine(rectTransform.localScale, targetScale, duration));
            }
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
                    
                    RestoreOriginalSettings();
                    
                    if (showDebugInfo)
                        Debug.Log($"[UIAnimator] UI动画播放完成: {animName}", this);
                    
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
            if (currentAnimation == null || !currentAnimation.IsValid || imageComponent == null) return;
            
            if (currentFrame >= 0 && currentFrame < currentAnimation.FrameCount)
            {
                imageComponent.sprite = currentAnimation.frames[currentFrame];
            }
        }
        
        private UIAnimation GetAnimation(string animationName)
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
                    animations[i].name = $"UI_Animation_{i}";
                }
                
                // 验证帧率
                if (animations[i].frameRate <= 0)
                {
                    animations[i].frameRate = 12f;
                }
            }
        }
        
        private void ApplyAnimationSettings()
        {
            if (currentAnimation == null || imageComponent == null) return;
            
            // 应用preserveAspect设置
            if (currentAnimation.preserveAspect != imageComponent.preserveAspect)
            {
                imageComponent.preserveAspect = currentAnimation.preserveAspect;
            }
        }
        
        private void RestoreOriginalSettings()
        {
            if (imageComponent == null) return;
            
            imageComponent.preserveAspect = originalPreserveAspect;
            imageComponent.raycastTarget = originalRaycastTarget;
        }
        
        private void UpdateUISettings()
        {
            if (imageComponent == null) return;
            
            imageComponent.raycastTarget = raycastTarget;
            imageComponent.preserveAspect = preserveAspect;
        }
        
        private System.Collections.IEnumerator FadeCoroutine(float fromAlpha, float toAlpha, float duration)
        {
            float elapsedTime = 0f;
            Color originalColor = imageComponent.color;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = elapsedTime / duration;
                
                Color newColor = originalColor;
                newColor.a = Mathf.Lerp(fromAlpha, toAlpha, t);
                imageComponent.color = newColor;
                
                yield return null;
            }
            
            // 确保最终值正确
            Color finalColor = originalColor;
            finalColor.a = toAlpha;
            imageComponent.color = finalColor;
        }
        
        private System.Collections.IEnumerator ScaleCoroutine(Vector3 fromScale, Vector3 toScale, float duration)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = elapsedTime / duration;
                
                rectTransform.localScale = Vector3.Lerp(fromScale, toScale, t);
                
                yield return null;
            }
            
            // 确保最终值正确
            rectTransform.localScale = toScale;
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
        public UIAnimation GetAnimationInfo(string animationName)
        {
            return GetAnimation(animationName);
        }
        
        #endregion
        
        #region UI特殊功能
        
        /// <summary>
        /// 设置UI透明度
        /// </summary>
        public void SetAlpha(float alpha)
        {
            if (imageComponent != null)
            {
                Color color = imageComponent.color;
                color.a = Mathf.Clamp01(alpha);
                imageComponent.color = color;
            }
        }
        
        /// <summary>
        /// 设置UI颜色
        /// </summary>
        public void SetColor(Color color)
        {
            if (imageComponent != null)
            {
                imageComponent.color = color;
            }
        }
        
        /// <summary>
        /// 设置是否响应射线检测
        /// </summary>
        public void SetRaycastTarget(bool enabled)
        {
            raycastTarget = enabled;
            if (imageComponent != null)
            {
                imageComponent.raycastTarget = enabled;
            }
        }
        
        /// <summary>
        /// 获取当前UI尺寸
        /// </summary>
        public Vector2 GetSize()
        {
            return rectTransform != null ? rectTransform.sizeDelta : Vector2.zero;
        }
        
        /// <summary>
        /// 设置UI尺寸
        /// </summary>
        public void SetSize(Vector2 size)
        {
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = size;
            }
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
        
        [ContextMenu("验证所有UI动画")]
        private void ValidateAllAnimations()
        {
            ValidateAnimations();
            Debug.Log($"[UIAnimator] 已验证 {animations?.Length ?? 0} 个UI动画", this);
        }
        
        [ContextMenu("适应当前Sprite尺寸")]
        private void FitToCurrentSprite()
        {
            if (imageComponent != null && imageComponent.sprite != null && rectTransform != null)
            {
                imageComponent.SetNativeSize();
                Debug.Log($"[UIAnimator] 已适应Sprite尺寸: {rectTransform.sizeDelta}", this);
            }
        }
#endif
        
        #endregion
    }
    
    #region 编辑器扩展
    
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(UIAnimator))]
    public class UIAnimatorEditor : UnityEditor.Editor
    {
        private UIAnimator animator;
        private bool showRuntimeInfo = true;
        private bool showUIControls = true;
        
        void OnEnable()
        {
            animator = (UIAnimator)target;
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (!Application.isPlaying) return;
            
            UnityEditor.EditorGUILayout.Space();
            
            // 运行时信息
            showRuntimeInfo = UnityEditor.EditorGUILayout.Foldout(showRuntimeInfo, "运行时信息");
            if (showRuntimeInfo)
            {
                UnityEditor.EditorGUI.BeginDisabledGroup(true);
                
                UnityEditor.EditorGUILayout.TextField("当前动画", animator.CurrentAnimationName);
                UnityEditor.EditorGUILayout.Toggle("正在播放", animator.IsPlaying);
                UnityEditor.EditorGUILayout.Toggle("已暂停", animator.IsPaused);
                UnityEditor.EditorGUILayout.IntField("当前帧", animator.CurrentFrame);
                UnityEditor.EditorGUILayout.Slider("进度", animator.Progress, 0f, 1f);
                UnityEditor.EditorGUILayout.Vector2Field("UI尺寸", animator.GetSize());
                
                UnityEditor.EditorGUI.EndDisabledGroup();
            }
            
            UnityEditor.EditorGUILayout.Space();
            
            // UI控制
            showUIControls = UnityEditor.EditorGUILayout.Foldout(showUIControls, "UI控制");
            if (showUIControls)
            {
                // 播放控制
                UnityEditor.EditorGUILayout.LabelField("播放控制", UnityEditor.EditorStyles.boldLabel);
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
                
                // UI特效控制
                UnityEditor.EditorGUILayout.LabelField("UI特效", UnityEditor.EditorStyles.boldLabel);
                UnityEditor.EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("淡入"))
                {
                    animator.FadeIn();
                }
                
                if (GUILayout.Button("淡出"))
                {
                    animator.FadeOut();
                }
                
                if (GUILayout.Button("缩放动画"))
                {
                    animator.ScaleAnimation(Vector3.one * 1.2f);
                }
                
                UnityEditor.EditorGUILayout.EndHorizontal();
                
                // 显示所有可用动画的快速播放按钮
                string[] animNames = animator.GetAnimationNames();
                if (animNames.Length > 0)
                {
                    UnityEditor.EditorGUILayout.LabelField("快速播放动画:", UnityEditor.EditorStyles.boldLabel);
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

using UnityEngine;
using UnityEngine.UI;

namespace Tools.UI
{
    /// <summary>
    /// 简单的UI帧动画播放器
    /// 适用于快速原型开发和简单UI动画需求
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class SimpleUIAnimator : MonoBehaviour
    {
        [Header("UI动画设置")]
        [Tooltip("UI动画帧序列")]
        public Sprite[] frames;
        
        [Tooltip("帧率 (帧/秒)")]
        [Range(1f, 60f)]
        public float frameRate = 12f;
        
        [Tooltip("开始时自动播放")]
        public bool playOnStart = true;
        
        [Tooltip("启用时自动播放")]
        public bool playOnEnable = false;
        
        [Tooltip("循环播放")]
        public bool loop = true;
        
        [Tooltip("随机起始帧")]
        public bool randomStartFrame = false;
        
        [Header("UI渲染设置")]
        [Tooltip("保持原生宽高比")]
        public bool preserveAspect = true;
        
        [Tooltip("响应射线检测")]
        public bool raycastTarget = true;
        
        [Header("调试")]
        [Tooltip("在Console显示播放信息")]
        public bool debugMode = false;
        
        // 私有变量
        private Image imageComponent;
        private RectTransform rectTransform;
        private int currentFrameIndex = 0;
        private float frameTimer = 0f;
        private bool isPlaying = false;
        
        // 属性
        public bool IsPlaying => isPlaying;
        public int CurrentFrame => currentFrameIndex;
        public int TotalFrames => frames?.Length ?? 0;
        public float Progress => TotalFrames > 0 ? (float)currentFrameIndex / TotalFrames : 0f;
        public Image ImageComponent => imageComponent;
        public RectTransform RectTransform => rectTransform;
        
        #region Unity生命周期
        
        void Awake()
        {
            imageComponent = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
        }
        
        void Start()
        {
            ApplyUISettings();
            
            if (frames != null && frames.Length > 0)
            {
                // 设置初始帧
                currentFrameIndex = randomStartFrame ? Random.Range(0, frames.Length) : 0;
                imageComponent.sprite = frames[currentFrameIndex];
                
                if (playOnStart)
                {
                    Play();
                }
            }
            else if (debugMode)
            {
                Debug.LogWarning("[SimpleUIAnimator] 没有设置UI动画帧", this);
            }
        }
        
        void OnEnable()
        {
            if (playOnEnable && frames != null && frames.Length > 0)
            {
                Play();
            }
        }
        
        void Update()
        {
            if (!isPlaying || frames == null || frames.Length <= 1) return;
            
            frameTimer += Time.unscaledDeltaTime; // 使用unscaledDeltaTime避免TimeScale影响
            
            if (frameTimer >= 1f / frameRate)
            {
                frameTimer = 0f;
                NextFrame();
            }
        }
        
        void OnValidate()
        {
            ApplyUISettings();
        }
        
        #endregion
        
        #region 播放控制
        
        /// <summary>
        /// 播放UI动画
        /// </summary>
        public void Play()
        {
            if (frames == null || frames.Length == 0)
            {
                if (debugMode)
                    Debug.LogWarning("[SimpleUIAnimator] 无法播放：没有UI动画帧", this);
                return;
            }
            
            isPlaying = true;
            
            if (debugMode)
                Debug.Log("[SimpleUIAnimator] 开始播放UI动画", this);
        }
        
        /// <summary>
        /// 停止UI动画
        /// </summary>
        public void Stop()
        {
            isPlaying = false;
            currentFrameIndex = 0;
            frameTimer = 0f;
            
            if (frames != null && frames.Length > 0)
            {
                imageComponent.sprite = frames[0];
            }
            
            if (debugMode)
                Debug.Log("[SimpleUIAnimator] 停止UI动画", this);
        }
        
        /// <summary>
        /// 暂停UI动画
        /// </summary>
        public void Pause()
        {
            isPlaying = false;
            
            if (debugMode)
                Debug.Log("[SimpleUIAnimator] 暂停UI动画", this);
        }
        
        /// <summary>
        /// 恢复UI动画
        /// </summary>
        public void Resume()
        {
            if (frames != null && frames.Length > 0)
            {
                isPlaying = true;
                
                if (debugMode)
                    Debug.Log("[SimpleUIAnimator] 恢复UI动画", this);
            }
        }
        
        /// <summary>
        /// 跳转到指定帧
        /// </summary>
        public void SetFrame(int frameIndex)
        {
            if (frames == null || frames.Length == 0) return;
            
            currentFrameIndex = Mathf.Clamp(frameIndex, 0, frames.Length - 1);
            imageComponent.sprite = frames[currentFrameIndex];
            
            if (debugMode)
                Debug.Log($"[SimpleUIAnimator] 跳转到第 {currentFrameIndex} 帧", this);
        }
        
        /// <summary>
        /// 设置新的UI动画帧序列
        /// </summary>
        public void SetFrames(Sprite[] newFrames)
        {
            frames = newFrames;
            currentFrameIndex = 0;
            frameTimer = 0f;
            
            if (frames != null && frames.Length > 0)
            {
                imageComponent.sprite = frames[0];
            }
            
            if (debugMode)
                Debug.Log($"[SimpleUIAnimator] 设置新UI动画帧序列，共 {frames?.Length ?? 0} 帧", this);
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
        /// 淡入效果
        /// </summary>
        public void FadeIn(float duration = 0.5f)
        {
            if (imageComponent != null)
            {
                // 使用简单的协程实现淡入
                StartCoroutine(FadeCoroutine(imageComponent.color.a, 1f, duration));
            }
        }
        
        /// <summary>
        /// 淡出效果
        /// </summary>
        public void FadeOut(float duration = 0.5f)
        {
            if (imageComponent != null)
            {
                StartCoroutine(FadeCoroutine(imageComponent.color.a, 0f, duration));
            }
        }
        
        /// <summary>
        /// 缩放效果
        /// </summary>
        public void ScaleTo(Vector3 targetScale, float duration = 0.3f)
        {
            if (rectTransform != null)
            {
                StartCoroutine(ScaleCoroutine(rectTransform.localScale, targetScale, duration));
            }
        }
        
        /// <summary>
        /// 适应原生尺寸
        /// </summary>
        public void SetNativeSize()
        {
            if (imageComponent != null)
            {
                imageComponent.SetNativeSize();
            }
        }
        
        #endregion
        
        #region 私有方法
        
        private void NextFrame()
        {
            currentFrameIndex++;
            
            if (currentFrameIndex >= frames.Length)
            {
                if (loop)
                {
                    currentFrameIndex = 0;
                }
                else
                {
                    currentFrameIndex = frames.Length - 1;
                    isPlaying = false;
                    
                    if (debugMode)
                        Debug.Log("[SimpleUIAnimator] UI动画播放完成", this);
                    
                    return;
                }
            }
            
            imageComponent.sprite = frames[currentFrameIndex];
        }
        
        private void ApplyUISettings()
        {
            if (imageComponent == null) return;
            
            imageComponent.preserveAspect = preserveAspect;
            imageComponent.raycastTarget = raycastTarget;
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
        
        #region 编辑器辅助方法
        
#if UNITY_EDITOR
        [ContextMenu("播放")]
        private void EditorPlay()
        {
            Play();
        }
        
        [ContextMenu("停止")]
        private void EditorStop()
        {
            Stop();
        }
        
        [ContextMenu("暂停")]
        private void EditorPause()
        {
            Pause();
        }
        
        [ContextMenu("下一帧")]
        private void EditorNextFrame()
        {
            if (frames != null && frames.Length > 0)
            {
                SetFrame((currentFrameIndex + 1) % frames.Length);
            }
        }
        
        [ContextMenu("上一帧")]
        private void EditorPreviousFrame()
        {
            if (frames != null && frames.Length > 0)
            {
                int prevFrame = currentFrameIndex - 1;
                if (prevFrame < 0) prevFrame = frames.Length - 1;
                SetFrame(prevFrame);
            }
        }
        
        [ContextMenu("适应原生尺寸")]
        private void EditorSetNativeSize()
        {
            SetNativeSize();
        }
        
        [ContextMenu("测试淡入")]
        private void EditorTestFadeIn()
        {
            FadeIn();
        }
        
        [ContextMenu("测试淡出")]
        private void EditorTestFadeOut()
        {
            FadeOut();
        }
#endif
        
        #endregion
    }
    
    #region 编辑器扩展
    
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SimpleUIAnimator))]
    public class SimpleUIAnimatorEditor : UnityEditor.Editor
    {
        private SimpleUIAnimator animator;
        private bool showRuntimeInfo = false;
        private bool showUIControls = false;
        
        void OnEnable()
        {
            animator = (SimpleUIAnimator)target;
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
                
                UnityEditor.EditorGUILayout.Toggle("正在播放", animator.IsPlaying);
                UnityEditor.EditorGUILayout.IntField("当前帧", animator.CurrentFrame);
                UnityEditor.EditorGUILayout.IntField("总帧数", animator.TotalFrames);
                UnityEditor.EditorGUILayout.Slider("进度", animator.Progress, 0f, 1f);
                
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
                
                if (GUILayout.Button("播放"))
                {
                    animator.Play();
                }
                
                if (GUILayout.Button("暂停"))
                {
                    animator.Pause();
                }
                
                if (GUILayout.Button("停止"))
                {
                    animator.Stop();
                }
                
                UnityEditor.EditorGUILayout.EndHorizontal();
                
                // 帧控制
                UnityEditor.EditorGUILayout.LabelField("帧控制", UnityEditor.EditorStyles.boldLabel);
                UnityEditor.EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("上一帧"))
                {
                    int prevFrame = animator.CurrentFrame - 1;
                    if (prevFrame < 0) prevFrame = animator.TotalFrames - 1;
                    animator.SetFrame(prevFrame);
                }
                
                if (GUILayout.Button("下一帧"))
                {
                    animator.SetFrame((animator.CurrentFrame + 1) % animator.TotalFrames);
                }
                
                UnityEditor.EditorGUILayout.EndHorizontal();
                
                // UI效果
                UnityEditor.EditorGUILayout.LabelField("UI效果", UnityEditor.EditorStyles.boldLabel);
                UnityEditor.EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("淡入"))
                {
                    animator.FadeIn();
                }
                
                if (GUILayout.Button("淡出"))
                {
                    animator.FadeOut();
                }
                
                if (GUILayout.Button("适应尺寸"))
                {
                    animator.SetNativeSize();
                }
                
                UnityEditor.EditorGUILayout.EndHorizontal();
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

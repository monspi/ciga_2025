using UnityEngine;

namespace Tools
{
    /// <summary>
    /// 简单的帧动画播放器，适合快速原型开发
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SimpleFrameAnimator : MonoBehaviour
    {
        [Header("动画设置")]
        [Tooltip("动画帧序列")]
        public Sprite[] frames;
        
        [Tooltip("帧率 (帧/秒)")]
        [Range(1f, 60f)]
        public float frameRate = 12f;
        
        [Tooltip("开始时自动播放")]
        public bool playOnStart = true;
        
        [Tooltip("循环播放")]
        public bool loop = true;
        
        [Tooltip("随机起始帧")]
        public bool randomStartFrame = false;
        
        [Header("调试")]
        [Tooltip("在Console显示播放信息")]
        public bool debugMode = false;
        
        // 私有变量
        private SpriteRenderer spriteRenderer;
        private int currentFrameIndex = 0;
        private float frameTimer = 0f;
        private bool isPlaying = false;
        
        // 属性
        public bool IsPlaying => isPlaying;
        public int CurrentFrame => currentFrameIndex;
        public int TotalFrames => frames?.Length ?? 0;
        public float Progress => TotalFrames > 0 ? (float)currentFrameIndex / TotalFrames : 0f;
        
        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (frames != null && frames.Length > 0)
            {
                // 设置初始帧
                currentFrameIndex = randomStartFrame ? Random.Range(0, frames.Length) : 0;
                spriteRenderer.sprite = frames[currentFrameIndex];
                
                if (playOnStart)
                {
                    Play();
                }
            }
            else if (debugMode)
            {
                Debug.LogWarning("[SimpleFrameAnimator] 没有设置动画帧", this);
            }
        }
        
        void Update()
        {
            if (!isPlaying || frames == null || frames.Length <= 1) return;
            
            frameTimer += Time.deltaTime;
            
            if (frameTimer >= 1f / frameRate)
            {
                frameTimer = 0f;
                NextFrame();
            }
        }
        
        /// <summary>
        /// 播放动画
        /// </summary>
        public void Play()
        {
            if (frames == null || frames.Length == 0)
            {
                if (debugMode)
                    Debug.LogWarning("[SimpleFrameAnimator] 无法播放：没有动画帧", this);
                return;
            }
            
            isPlaying = true;
            
            if (debugMode)
                Debug.Log("[SimpleFrameAnimator] 开始播放动画", this);
        }
        
        /// <summary>
        /// 停止动画
        /// </summary>
        public void Stop()
        {
            isPlaying = false;
            currentFrameIndex = 0;
            frameTimer = 0f;
            
            if (frames != null && frames.Length > 0)
            {
                spriteRenderer.sprite = frames[0];
            }
            
            if (debugMode)
                Debug.Log("[SimpleFrameAnimator] 停止动画", this);
        }
        
        /// <summary>
        /// 暂停动画
        /// </summary>
        public void Pause()
        {
            isPlaying = false;
            
            if (debugMode)
                Debug.Log("[SimpleFrameAnimator] 暂停动画", this);
        }
        
        /// <summary>
        /// 恢复动画
        /// </summary>
        public void Resume()
        {
            if (frames != null && frames.Length > 0)
            {
                isPlaying = true;
                
                if (debugMode)
                    Debug.Log("[SimpleFrameAnimator] 恢复动画", this);
            }
        }
        
        /// <summary>
        /// 跳转到指定帧
        /// </summary>
        public void SetFrame(int frameIndex)
        {
            if (frames == null || frames.Length == 0) return;
            
            currentFrameIndex = Mathf.Clamp(frameIndex, 0, frames.Length - 1);
            spriteRenderer.sprite = frames[currentFrameIndex];
            
            if (debugMode)
                Debug.Log($"[SimpleFrameAnimator] 跳转到第 {currentFrameIndex} 帧", this);
        }
        
        /// <summary>
        /// 设置新的动画帧序列
        /// </summary>
        public void SetFrames(Sprite[] newFrames)
        {
            frames = newFrames;
            currentFrameIndex = 0;
            frameTimer = 0f;
            
            if (frames != null && frames.Length > 0)
            {
                spriteRenderer.sprite = frames[0];
            }
            
            if (debugMode)
                Debug.Log($"[SimpleFrameAnimator] 设置新动画帧序列，共 {frames?.Length ?? 0} 帧", this);
        }
        
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
                        Debug.Log("[SimpleFrameAnimator] 动画播放完成", this);
                    
                    return;
                }
            }
            
            spriteRenderer.sprite = frames[currentFrameIndex];
        }
        
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
#endif
        
        #endregion
    }
}

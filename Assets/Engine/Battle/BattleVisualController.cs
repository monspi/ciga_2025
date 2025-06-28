using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace FartGame.Battle
{
    public class BattleVisualController : MonoBehaviour
    {
        [Header("UI组件引用")]
        [SerializeField] private Text comboText;
        [SerializeField] private Slider staminaBar;
        [SerializeField] private Text judgementFeedbackText;
        [SerializeField] private Transform arrowContainer;
        [SerializeField] private GameObject arrowPrefab;
        
        [Header("视觉效果")]
        [SerializeField] private Image buttImage;
        [SerializeField] private GameObject emotionHeadPrefab;
        [SerializeField] private GameObject fartBubblePrefab;
        [SerializeField] private Transform beatIndicatorContainer;
        
        [Header("设置")]
        [SerializeField] private float arrowMoveSpeed = 5f;
        [SerializeField] private Color hitColor = Color.green;
        [SerializeField] private Color missColor = Color.red;
        
        private MusicTimeManager timeManager;
        private BattleManager battleManager;
        private List<GameObject> activeArrows = new List<GameObject>();
        private bool isInitialized = false;
        
        // 依赖注入
        public void Initialize(MusicTimeManager timeManager, BattleManager battleManager)
        {
            this.timeManager = timeManager;
            this.battleManager = battleManager;
            isInitialized = true;
            
            Debug.Log("[BattleVisualController] 初始化完成");
        }
        
        // === 箭头显示管理 ===
        public void ShowDirectionArrow(Direction direction, double appearTime)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[BattleVisualController] 尚未初始化");
                return;
            }
            
            if (arrowPrefab == null || arrowContainer == null)
            {
                Debug.LogWarning("[BattleVisualController] 箭头预制体或容器未设置");
                return;
            }
            
            // 创建箭头实例
            GameObject arrow = Instantiate(arrowPrefab, arrowContainer);
            activeArrows.Add(arrow);
            
            // 设置箭头的初始位置（从屏幕右侧开始）
            Vector3 startPosition = new Vector3(Screen.width * 0.6f, 0, 0);
            arrow.transform.position = startPosition;
            
            // 根据方向设置箭头的旋转或图标
            SetArrowDirection(arrow, direction);
            
            Debug.Log($"[BattleVisualController] 显示箭头: {direction} at {appearTime:F3}，移动速度: {arrowMoveSpeed}");
        }
        
        public void UpdateArrowPositions()
        {
            if (!isInitialized || timeManager == null)
                return;
                
            // 实现箭头位置更新逻辑
            double currentTime = timeManager.GetJudgementTime();
            
            foreach (var arrow in activeArrows)
            {
                if (arrow != null)
                {
                    // 根据时间和移动速度更新箭头位置
                    // 这里使用 arrowMoveSpeed 来控制箭头移动
                    Vector3 position = arrow.transform.position;
                    position.x -= arrowMoveSpeed * Time.deltaTime;
                    arrow.transform.position = position;
                    
                    // 如果箭头移出屏幕，销毁它
                    if (position.x < -Screen.width * 0.6f)
                    {
                        Destroy(arrow);
                        activeArrows.Remove(arrow);
                        break; // 避免在迭代时修改集合导致的问题
                    }
                }
            }
        }
        
        // === 透明度系统 ===
        public void UpdateButtTransparency(float transparency)
        {
            if (buttImage != null)
            {
                Color color = buttImage.color;
                color.a = transparency;
                buttImage.color = color;
            }
        }
        
        public void ShowEmotionHead()
        {
            if (emotionHeadPrefab != null)
            {
                // TODO: 实现表情头部显示逻辑
                Debug.Log("[BattleVisualController] 显示表情头部");
            }
        }
        
        public void ShowFartBubbleEffect()
        {
            if (fartBubblePrefab != null)
            {
                // TODO: 实现屁气泡特效逻辑
                Debug.Log("[BattleVisualController] 播放屁气泡特效");
            }
        }
        
        // === 节拍提示 ===
        public void ShowBeatIndicator(double beatTime)
        {
            // TODO: 实现节拍指示器显示逻辑
            Debug.Log($"[BattleVisualController] 显示节拍指示器: {beatTime:F3}");
        }
        
        public void UpdateBeatVisuals()
        {
            if (!isInitialized || timeManager == null)
                return;
                
            // TODO: 实现节拍视觉更新逻辑
            double currentTime = timeManager.GetJudgementTime();
        }
        
        // === UI状态更新 ===
        public void UpdateComboDisplay(int combo)
        {
            if (comboText != null)
            {
                comboText.text = $"连击: {combo}";
            }
        }
        
        public void UpdateStaminaBar(float stamina)
        {
            if (staminaBar != null)
            {
                staminaBar.value = stamina;
            }
        }
        
        public void ShowJudgementFeedback(JudgementResult result)
        {
            if (judgementFeedbackText != null)
            {
                if (result.isHit)
                {
                    judgementFeedbackText.text = "HIT!";
                    judgementFeedbackText.color = hitColor;
                }
                else if (result.isMiss)
                {
                    judgementFeedbackText.text = "MISS!";
                    judgementFeedbackText.color = missColor;
                }
                
                // TODO: 添加反馈文本的淡出动画
            }
        }
        
        // === Unity生命周期 ===
        private void Update()
        {
            if (!isInitialized)
                return;
                
            UpdateArrowPositions();
            UpdateBeatVisuals();
        }
        
        private void OnDestroy()
        {
            // 清理箭头对象
            foreach (var arrow in activeArrows)
            {
                if (arrow != null)
                {
                    Destroy(arrow);
                }
            }
            activeArrows.Clear();
            
            Debug.Log("[BattleVisualController] 组件销毁");
        }
        
        // === 私有辅助方法 ===
        private void SetArrowDirection(GameObject arrow, Direction direction)
        {
            // 根据方向设置箭头的旋转
            switch (direction)
            {
                case Direction.Up:
                    arrow.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case Direction.Down:
                    arrow.transform.rotation = Quaternion.Euler(0, 0, 180);
                    break;
                case Direction.Left:
                    arrow.transform.rotation = Quaternion.Euler(0, 0, 90);
                    break;
                case Direction.Right:
                    arrow.transform.rotation = Quaternion.Euler(0, 0, -90);
                    break;
            }
        }
        
        private void ClearActiveArrows()
        {
            foreach (var arrow in activeArrows)
            {
                if (arrow != null)
                {
                    Destroy(arrow);
                }
            }
            activeArrows.Clear();
        }
        
        // === 调试方法 ===
        public void TestArrowDisplay()
        {
            ShowDirectionArrow(Direction.Up, timeManager?.GetJudgementTime() ?? 0);
        }
        
        public void TestJudgementFeedback()
        {
            ShowJudgementFeedback(new JudgementResult { isHit = true, accuracy = 0.95f });
        }
    }
}

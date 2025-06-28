using UnityEngine;
using System.Collections.Generic;

namespace Tools.UI
{
    /// <summary>
    /// UI动画状态类型
    /// </summary>
    public enum UIAnimationState
    {
        Idle,           // 静止
        Hover,          // 悬停
        Pressed,        // 按下
        Selected,       // 选中
        Disabled,       // 禁用
        Loading,        // 加载中
        Success,        // 成功
        Error,          // 错误
        Warning,        // 警告
        Notification,   // 通知
        Custom1,        // 自定义1
        Custom2,        // 自定义2
        Custom3         // 自定义3
    }
    
    /// <summary>
    /// UI动画控制器，专门管理UI元素的动画状态
    /// </summary>
    [RequireComponent(typeof(UIAnimator))]
    public class UIAnimationController : MonoBehaviour
    {
        [System.Serializable]
        public class UIStateAnimationMapping
        {
            public UIAnimationState state;
            public string animationName;
            public bool canInterrupt = true;
            public float priority = 0f;
            public bool autoReturn = false;         // 动画完成后是否自动返回默认状态
            public float autoReturnDelay = 0f;      // 自动返回延迟
        }
        
        [Header("UI状态动画映射")]
        [SerializeField] private UIStateAnimationMapping[] stateAnimations;
        
        [Header("默认设置")]
        [SerializeField] private UIAnimationState defaultState = UIAnimationState.Idle;
        [SerializeField] private bool autoPlayDefault = true;
        [SerializeField] private bool respondToUIEvents = true;
        
        [Header("交互设置")]
        [SerializeField] private bool enableHoverAnimation = true;
        [SerializeField] private bool enableClickAnimation = true;
        [SerializeField] private bool enableSelectionAnimation = true;
        
        [Header("调试")]
        [SerializeField] private bool showDebugInfo = false;
        
        // 组件引用
        private UIAnimator uiAnimator;
        private UnityEngine.UI.Button button;
        private UnityEngine.UI.Toggle toggle;
        private UnityEngine.UI.Selectable selectable;
        
        // 状态管理
        private UIAnimationState currentState;
        private UIAnimationState previousState;
        private Dictionary<UIAnimationState, UIStateAnimationMapping> stateMappingDict;
        
        // 自动返回计时器
        private float autoReturnTimer = 0f;
        private bool hasAutoReturn = false;
        
        // 属性
        public UIAnimationState CurrentState => currentState;
        public UIAnimationState PreviousState => previousState;
        public bool IsInState(UIAnimationState state) => currentState == state;
        
        #region Unity生命周期
        
        void Awake()
        {
            uiAnimator = GetComponent<UIAnimator>();
            button = GetComponent<UnityEngine.UI.Button>();
            toggle = GetComponent<UnityEngine.UI.Toggle>();
            selectable = GetComponent<UnityEngine.UI.Selectable>();
            
            BuildStateMappingDictionary();
        }
        
        void Start()
        {
            // 监听UI动画完成事件
            if (uiAnimator != null)
            {
                uiAnimator.OnAnimationCompleted += OnAnimationCompleted;
            }
            
            // 设置UI事件监听
            if (respondToUIEvents)
            {
                SetupUIEventListeners();
            }
            
            if (autoPlayDefault)
            {
                SetUIAnimationState(defaultState);
            }
        }
        
        void Update()
        {
            // 处理自动返回
            if (hasAutoReturn)
            {
                autoReturnTimer -= Time.unscaledDeltaTime;
                if (autoReturnTimer <= 0f)
                {
                    hasAutoReturn = false;
                    SetUIAnimationState(defaultState);
                }
            }
        }
        
        void OnDestroy()
        {
            if (uiAnimator != null)
            {
                uiAnimator.OnAnimationCompleted -= OnAnimationCompleted;
            }
        }
        
        #endregion
        
        #region UI状态控制
        
        /// <summary>
        /// 设置UI动画状态
        /// </summary>
        public bool SetUIAnimationState(UIAnimationState newState, bool force = false)
        {
            // 检查是否为相同状态
            if (currentState == newState && !force) return true;
            
            // 获取当前状态映射
            UIStateAnimationMapping currentMapping = GetStateMapping(currentState);
            UIStateAnimationMapping newMapping = GetStateMapping(newState);
            
            if (newMapping == null)
            {
                if (showDebugInfo)
                    Debug.LogWarning($"[UIAnimationController] 未找到UI状态映射: {newState}", this);
                return false;
            }
            
            // 检查是否可以打断当前动画
            if (currentMapping != null && !currentMapping.canInterrupt && !force)
            {
                // 检查优先级
                if (newMapping.priority <= currentMapping.priority)
                {
                    if (showDebugInfo)
                        Debug.Log($"[UIAnimationController] UI动画 {currentState} 不可打断，优先级不足", this);
                    return false;
                }
            }
            
            // 更新状态
            previousState = currentState;
            currentState = newState;
            
            // 重置自动返回
            hasAutoReturn = false;
            
            // 播放对应动画
            bool success = uiAnimator.PlayAnimation(newMapping.animationName, force);
            
            if (success && showDebugInfo)
            {
                Debug.Log($"[UIAnimationController] UI状态切换: {previousState} -> {currentState}", this);
            }
            
            return success;
        }
        
        /// <summary>
        /// 设置加载状态
        /// </summary>
        public void SetLoading(bool isLoading)
        {
            if (isLoading)
            {
                SetUIAnimationState(UIAnimationState.Loading);
            }
            else
            {
                SetUIAnimationState(defaultState);
            }
        }
        
        /// <summary>
        /// 显示成功状态
        /// </summary>
        public void ShowSuccess(float duration = 2f)
        {
            SetUIAnimationState(UIAnimationState.Success);
            SetAutoReturn(duration);
        }
        
        /// <summary>
        /// 显示错误状态
        /// </summary>
        public void ShowError(float duration = 2f)
        {
            SetUIAnimationState(UIAnimationState.Error);
            SetAutoReturn(duration);
        }
        
        /// <summary>
        /// 显示警告状态
        /// </summary>
        public void ShowWarning(float duration = 2f)
        {
            SetUIAnimationState(UIAnimationState.Warning);
            SetAutoReturn(duration);
        }
        
        /// <summary>
        /// 显示通知状态
        /// </summary>
        public void ShowNotification(float duration = 3f)
        {
            SetUIAnimationState(UIAnimationState.Notification);
            SetAutoReturn(duration);
        }
        
        /// <summary>
        /// 设置自动返回
        /// </summary>
        public void SetAutoReturn(float delay)
        {
            autoReturnTimer = delay;
            hasAutoReturn = true;
        }
        
        /// <summary>
        /// 回到默认状态
        /// </summary>
        public void ReturnToDefault()
        {
            SetUIAnimationState(defaultState);
        }
        
        #endregion
        
        #region UI事件处理
        
        private void SetupUIEventListeners()
        {
            // Button事件
            if (button != null && enableClickAnimation)
            {
                button.onClick.AddListener(OnButtonClick);
            }
            
            // Toggle事件
            if (toggle != null && enableSelectionAnimation)
            {
                toggle.onValueChanged.AddListener(OnToggleValueChanged);
            }
            
            // 通用Selectable事件（需要EventTrigger）
            if (selectable != null)
            {
                var eventTrigger = GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (eventTrigger == null && (enableHoverAnimation || enableClickAnimation))
                {
                    eventTrigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                }
                
                if (eventTrigger != null)
                {
                    SetupEventTrigger(eventTrigger);
                }
            }
        }
        
        private void SetupEventTrigger(UnityEngine.EventSystems.EventTrigger eventTrigger)
        {
            // 悬停进入
            if (enableHoverAnimation)
            {
                var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry
                {
                    eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
                };
                pointerEnter.callback.AddListener(OnPointerEnter);
                eventTrigger.triggers.Add(pointerEnter);
                
                // 悬停退出
                var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry
                {
                    eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
                };
                pointerExit.callback.AddListener(OnPointerExit);
                eventTrigger.triggers.Add(pointerExit);
            }
            
            // 按下
            if (enableClickAnimation)
            {
                var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry
                {
                    eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown
                };
                pointerDown.callback.AddListener(OnPointerDown);
                eventTrigger.triggers.Add(pointerDown);
                
                // 释放
                var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry
                {
                    eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp
                };
                pointerUp.callback.AddListener(OnPointerUp);
                eventTrigger.triggers.Add(pointerUp);
            }
        }
        
        private void OnButtonClick()
        {
            if (showDebugInfo)
                Debug.Log("[UIAnimationController] Button clicked", this);
            // Button点击通常已经有按下效果，这里可以添加额外逻辑
        }
        
        private void OnToggleValueChanged(bool value)
        {
            if (showDebugInfo)
                Debug.Log($"[UIAnimationController] Toggle changed to: {value}", this);
            
            SetUIAnimationState(value ? UIAnimationState.Selected : UIAnimationState.Idle);
        }
        
        private void OnPointerEnter(UnityEngine.EventSystems.BaseEventData eventData)
        {
            if (selectable != null && selectable.interactable)
            {
                SetUIAnimationState(UIAnimationState.Hover);
            }
        }
        
        private void OnPointerExit(UnityEngine.EventSystems.BaseEventData eventData)
        {
            if (selectable != null && selectable.interactable)
            {
                SetUIAnimationState(UIAnimationState.Idle);
            }
        }
        
        private void OnPointerDown(UnityEngine.EventSystems.BaseEventData eventData)
        {
            if (selectable != null && selectable.interactable)
            {
                SetUIAnimationState(UIAnimationState.Pressed);
            }
        }
        
        private void OnPointerUp(UnityEngine.EventSystems.BaseEventData eventData)
        {
            if (selectable != null && selectable.interactable)
            {
                SetUIAnimationState(UIAnimationState.Hover);
            }
        }
        
        #endregion
        
        #region 私有方法
        
        private void BuildStateMappingDictionary()
        {
            stateMappingDict = new Dictionary<UIAnimationState, UIStateAnimationMapping>();
            
            if (stateAnimations != null)
            {
                foreach (var mapping in stateAnimations)
                {
                    if (!stateMappingDict.ContainsKey(mapping.state))
                    {
                        stateMappingDict[mapping.state] = mapping;
                    }
                    else if (showDebugInfo)
                    {
                        Debug.LogWarning($"[UIAnimationController] 重复的UI状态映射: {mapping.state}", this);
                    }
                }
            }
        }
        
        private UIStateAnimationMapping GetStateMapping(UIAnimationState state)
        {
            stateMappingDict.TryGetValue(state, out UIStateAnimationMapping mapping);
            return mapping;
        }
        
        private void OnAnimationCompleted(string animationName)
        {
            if (showDebugInfo)
                Debug.Log($"[UIAnimationController] UI动画完成: {animationName}", this);
            
            // 检查是否需要自动返回
            UIStateAnimationMapping currentMapping = GetStateMapping(currentState);
            if (currentMapping != null && currentMapping.autoReturn)
            {
                if (currentMapping.autoReturnDelay > 0f)
                {
                    SetAutoReturn(currentMapping.autoReturnDelay);
                }
                else
                {
                    SetUIAnimationState(defaultState);
                }
            }
        }
        
        #endregion
        
        #region 公共查询方法
        
        /// <summary>
        /// 检查是否有指定状态的映射
        /// </summary>
        public bool HasStateMapping(UIAnimationState state)
        {
            return stateMappingDict.ContainsKey(state);
        }
        
        /// <summary>
        /// 获取状态对应的动画名称
        /// </summary>
        public string GetAnimationNameForState(UIAnimationState state)
        {
            UIStateAnimationMapping mapping = GetStateMapping(state);
            return mapping?.animationName ?? "";
        }
        
        /// <summary>
        /// 设置UI元素是否可交互
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (selectable != null)
            {
                selectable.interactable = interactable;
                
                if (!interactable)
                {
                    SetUIAnimationState(UIAnimationState.Disabled);
                }
                else
                {
                    SetUIAnimationState(defaultState);
                }
            }
        }
        
        #endregion
        
        #region 编辑器辅助方法
        
#if UNITY_EDITOR
        [ContextMenu("播放默认状态")]
        private void PlayDefaultState()
        {
            SetUIAnimationState(defaultState, true);
        }
        
        [ContextMenu("重建UI状态映射")]
        private void RebuildStateMappings()
        {
            BuildStateMappingDictionary();
            Debug.Log($"[UIAnimationController] 已重建UI状态映射，共 {stateMappingDict.Count} 个映射", this);
        }
        
        [ContextMenu("测试悬停效果")]
        private void TestHoverEffect()
        {
            SetUIAnimationState(UIAnimationState.Hover, true);
        }
        
        [ContextMenu("测试按下效果")]
        private void TestPressedEffect()
        {
            SetUIAnimationState(UIAnimationState.Pressed, true);
        }
        
        void OnValidate()
        {
            if (stateMappingDict != null)
            {
                BuildStateMappingDictionary();
            }
        }
#endif
        
        #endregion
    }
    
    #region 编辑器扩展
    
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(UIAnimationController))]
    public class UIAnimationControllerEditor : UnityEditor.Editor
    {
        private UIAnimationController controller;
        private bool showRuntimeInfo = true;
        private bool showQuickControls = true;
        private bool showStatusControls = true;
        
        void OnEnable()
        {
            controller = (UIAnimationController)target;
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (!Application.isPlaying) return;
            
            UnityEditor.EditorGUILayout.Space();
            
            // 运行时信息
            showRuntimeInfo = UnityEditor.EditorGUILayout.Foldout(showRuntimeInfo, "运行时UI状态信息");
            if (showRuntimeInfo)
            {
                UnityEditor.EditorGUI.BeginDisabledGroup(true);
                UnityEditor.EditorGUILayout.EnumPopup("当前状态", controller.CurrentState);
                UnityEditor.EditorGUILayout.EnumPopup("上一状态", controller.PreviousState);
                UnityEditor.EditorGUI.EndDisabledGroup();
            }
            
            UnityEditor.EditorGUILayout.Space();
            
            // 快速控制
            showQuickControls = UnityEditor.EditorGUILayout.Foldout(showQuickControls, "快速UI状态切换");
            if (showQuickControls)
            {
                UnityEditor.EditorGUILayout.BeginVertical("box");
                
                // 基本UI状态
                UnityEditor.EditorGUILayout.LabelField("基本状态", UnityEditor.EditorStyles.boldLabel);
                UnityEditor.EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Idle")) controller.SetUIAnimationState(UIAnimationState.Idle);
                if (GUILayout.Button("Hover")) controller.SetUIAnimationState(UIAnimationState.Hover);
                if (GUILayout.Button("Pressed")) controller.SetUIAnimationState(UIAnimationState.Pressed);
                UnityEditor.EditorGUILayout.EndHorizontal();
                
                UnityEditor.EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Selected")) controller.SetUIAnimationState(UIAnimationState.Selected);
                if (GUILayout.Button("Disabled")) controller.SetUIAnimationState(UIAnimationState.Disabled);
                if (GUILayout.Button("Loading")) controller.SetUIAnimationState(UIAnimationState.Loading);
                UnityEditor.EditorGUILayout.EndHorizontal();
                
                UnityEditor.EditorGUILayout.EndVertical();
            }
            
            // 状态通知控制
            showStatusControls = UnityEditor.EditorGUILayout.Foldout(showStatusControls, "状态通知控制");
            if (showStatusControls)
            {
                UnityEditor.EditorGUILayout.BeginVertical("box");
                
                UnityEditor.EditorGUILayout.LabelField("通知状态", UnityEditor.EditorStyles.boldLabel);
                UnityEditor.EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("成功")) controller.ShowSuccess();
                if (GUILayout.Button("错误")) controller.ShowError();
                if (GUILayout.Button("警告")) controller.ShowWarning();
                UnityEditor.EditorGUILayout.EndHorizontal();
                
                UnityEditor.EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("通知")) controller.ShowNotification();
                if (GUILayout.Button("回到默认")) controller.ReturnToDefault();
                UnityEditor.EditorGUILayout.EndHorizontal();
                
                UnityEditor.EditorGUILayout.EndVertical();
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

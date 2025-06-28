using UnityEngine;
using System.Collections.Generic;

namespace Tools
{
    /// <summary>
    /// 动画状态类型
    /// </summary>
    public enum AnimationState
    {
        Idle,       // 待机
        Walk,       // 行走
        Run,        // 奔跑
        Jump,       // 跳跃
        Fall,       // 下落
        Attack,     // 攻击
        Hurt,       // 受伤
        Die,        // 死亡
        Custom1,    // 自定义1
        Custom2,    // 自定义2
        Custom3     // 自定义3
    }
    
    /// <summary>
    /// 角色动画控制器，管理不同状态的动画切换
    /// </summary>
    [RequireComponent(typeof(SpriteAnimator))]
    public class CharacterAnimationController : MonoBehaviour
    {
        [System.Serializable]
        public class StateAnimationMapping
        {
            public AnimationState state;
            public string animationName;
            public bool canInterrupt = true;  // 是否可以被其他动画打断
            public float priority = 0f;       // 优先级，数值越高优先级越高
        }
        
        [Header("状态动画映射")]
        [SerializeField] private StateAnimationMapping[] stateAnimations;
        
        [Header("默认设置")]
        [SerializeField] private AnimationState defaultState = AnimationState.Idle;
        [SerializeField] private bool autoPlayDefault = true;
        
        [Header("调试")]
        [SerializeField] private bool showDebugInfo = false;
        
        // 组件引用
        private SpriteAnimator spriteAnimator;
        
        // 状态管理
        private AnimationState currentState;
        private AnimationState previousState;
        private Dictionary<AnimationState, StateAnimationMapping> stateMappingDict;
        
        // 队列系统
        private Queue<AnimationState> animationQueue = new Queue<AnimationState>();
        private bool processQueue = true;
        
        // 属性
        public AnimationState CurrentState => currentState;
        public AnimationState PreviousState => previousState;
        public bool IsInState(AnimationState state) => currentState == state;
        
        #region Unity生命周期
        
        void Awake()
        {
            spriteAnimator = GetComponent<SpriteAnimator>();
            BuildStateMappingDictionary();
        }
        
        void Start()
        {
            // 监听动画完成事件
            if (spriteAnimator != null)
            {
                spriteAnimator.OnAnimationCompleted += OnAnimationCompleted;
            }
            
            if (autoPlayDefault)
            {
                SetAnimationState(defaultState);
            }
        }
        
        void OnDestroy()
        {
            if (spriteAnimator != null)
            {
                spriteAnimator.OnAnimationCompleted -= OnAnimationCompleted;
            }
        }
        
        #endregion
        
        #region 状态控制
        
        /// <summary>
        /// 设置动画状态
        /// </summary>
        public bool SetAnimationState(AnimationState newState, bool force = false)
        {
            // 检查是否为相同状态
            if (currentState == newState && !force) return true;
            
            // 获取当前状态映射
            StateAnimationMapping currentMapping = GetStateMapping(currentState);
            StateAnimationMapping newMapping = GetStateMapping(newState);
            
            if (newMapping == null)
            {
                if (showDebugInfo)
                    Debug.LogWarning($"[CharacterAnimationController] 未找到状态映射: {newState}", this);
                return false;
            }
            
            // 检查是否可以打断当前动画
            if (currentMapping != null && !currentMapping.canInterrupt && !force)
            {
                // 检查优先级
                if (newMapping.priority <= currentMapping.priority)
                {
                    if (showDebugInfo)
                        Debug.Log($"[CharacterAnimationController] 动画 {currentState} 不可打断，优先级不足", this);
                    return false;
                }
            }
            
            // 更新状态
            previousState = currentState;
            currentState = newState;
            
            // 播放对应动画
            bool success = spriteAnimator.PlayAnimation(newMapping.animationName, force);
            
            if (success && showDebugInfo)
            {
                Debug.Log($"[CharacterAnimationController] 状态切换: {previousState} -> {currentState}", this);
            }
            
            return success;
        }
        
        /// <summary>
        /// 排队播放动画状态
        /// </summary>
        public void QueueAnimationState(AnimationState state)
        {
            animationQueue.Enqueue(state);
            
            if (showDebugInfo)
                Debug.Log($"[CharacterAnimationController] 动画状态加入队列: {state}", this);
        }
        
        /// <summary>
        /// 清空动画队列
        /// </summary>
        public void ClearAnimationQueue()
        {
            animationQueue.Clear();
            
            if (showDebugInfo)
                Debug.Log("[CharacterAnimationController] 动画队列已清空", this);
        }
        
        /// <summary>
        /// 回到上一个状态
        /// </summary>
        public bool RevertToPreviousState()
        {
            return SetAnimationState(previousState);
        }
        
        /// <summary>
        /// 强制播放动画（忽略打断限制）
        /// </summary>
        public bool ForceAnimationState(AnimationState state)
        {
            return SetAnimationState(state, true);
        }
        
        #endregion
        
        #region 便捷方法
        
        /// <summary>
        /// 播放待机动画
        /// </summary>
        public void PlayIdle() => SetAnimationState(AnimationState.Idle);
        
        /// <summary>
        /// 播放行走动画
        /// </summary>
        public void PlayWalk() => SetAnimationState(AnimationState.Walk);
        
        /// <summary>
        /// 播放奔跑动画
        /// </summary>
        public void PlayRun() => SetAnimationState(AnimationState.Run);
        
        /// <summary>
        /// 播放跳跃动画
        /// </summary>
        public void PlayJump() => SetAnimationState(AnimationState.Jump);
        
        /// <summary>
        /// 播放攻击动画
        /// </summary>
        public void PlayAttack() => SetAnimationState(AnimationState.Attack);
        
        /// <summary>
        /// 播放受伤动画
        /// </summary>
        public void PlayHurt() => SetAnimationState(AnimationState.Hurt);
        
        /// <summary>
        /// 播放死亡动画
        /// </summary>
        public void PlayDie() => SetAnimationState(AnimationState.Die);
        
        #endregion
        
        #region 私有方法
        
        private void BuildStateMappingDictionary()
        {
            stateMappingDict = new Dictionary<AnimationState, StateAnimationMapping>();
            
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
                        Debug.LogWarning($"[CharacterAnimationController] 重复的状态映射: {mapping.state}", this);
                    }
                }
            }
        }
        
        private StateAnimationMapping GetStateMapping(AnimationState state)
        {
            stateMappingDict.TryGetValue(state, out StateAnimationMapping mapping);
            return mapping;
        }
        
        private void OnAnimationCompleted(string animationName)
        {
            if (showDebugInfo)
                Debug.Log($"[CharacterAnimationController] 动画完成: {animationName}", this);
            
            // 处理队列中的下一个动画
            if (processQueue && animationQueue.Count > 0)
            {
                AnimationState nextState = animationQueue.Dequeue();
                SetAnimationState(nextState);
            }
            else
            {
                // 某些状态完成后自动回到默认状态
                HandleAnimationCompletion();
            }
        }
        
        private void HandleAnimationCompletion()
        {
            switch (currentState)
            {
                case AnimationState.Attack:
                case AnimationState.Hurt:
                case AnimationState.Jump:
                case AnimationState.Fall:
                    // 这些动画完成后通常回到待机状态
                    SetAnimationState(defaultState);
                    break;
                    
                case AnimationState.Die:
                    // 死亡动画完成后保持在死亡状态
                    break;
                    
                default:
                    // 其他情况保持当前状态
                    break;
            }
        }
        
        #endregion
        
        #region 查询方法
        
        /// <summary>
        /// 检查是否有指定状态的映射
        /// </summary>
        public bool HasStateMapping(AnimationState state)
        {
            return stateMappingDict.ContainsKey(state);
        }
        
        /// <summary>
        /// 获取状态对应的动画名称
        /// </summary>
        public string GetAnimationNameForState(AnimationState state)
        {
            StateAnimationMapping mapping = GetStateMapping(state);
            return mapping?.animationName ?? "";
        }
        
        /// <summary>
        /// 检查当前动画是否可以被打断
        /// </summary>
        public bool CanCurrentAnimationBeInterrupted()
        {
            StateAnimationMapping currentMapping = GetStateMapping(currentState);
            return currentMapping?.canInterrupt ?? true;
        }
        
        #endregion
        
        #region 编辑器辅助方法
        
#if UNITY_EDITOR
        [ContextMenu("播放默认状态")]
        private void PlayDefaultState()
        {
            SetAnimationState(defaultState, true);
        }
        
        [ContextMenu("重建状态映射")]
        private void RebuildStateMappings()
        {
            BuildStateMappingDictionary();
            Debug.Log($"[CharacterAnimationController] 已重建状态映射，共 {stateMappingDict.Count} 个映射", this);
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
    [UnityEditor.CustomEditor(typeof(CharacterAnimationController))]
    public class CharacterAnimationControllerEditor : UnityEditor.Editor
    {
        private CharacterAnimationController controller;
        private bool showRuntimeInfo = true;
        private bool showQuickControls = true;
        
        void OnEnable()
        {
            controller = (CharacterAnimationController)target;
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (!Application.isPlaying) return;
            
            UnityEditor.EditorGUILayout.Space();
            
            // 运行时信息
            showRuntimeInfo = UnityEditor.EditorGUILayout.Foldout(showRuntimeInfo, "运行时状态信息");
            if (showRuntimeInfo)
            {
                UnityEditor.EditorGUI.BeginDisabledGroup(true);
                UnityEditor.EditorGUILayout.EnumPopup("当前状态", controller.CurrentState);
                UnityEditor.EditorGUILayout.EnumPopup("上一状态", controller.PreviousState);
                UnityEditor.EditorGUI.EndDisabledGroup();
            }
            
            UnityEditor.EditorGUILayout.Space();
            
            // 快速控制
            showQuickControls = UnityEditor.EditorGUILayout.Foldout(showQuickControls, "快速状态切换");
            if (showQuickControls)
            {
                UnityEditor.EditorGUILayout.BeginVertical("box");
                
                // 基本状态
                UnityEditor.EditorGUILayout.LabelField("基本状态", UnityEditor.EditorStyles.boldLabel);
                UnityEditor.EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Idle")) controller.PlayIdle();
                if (GUILayout.Button("Walk")) controller.PlayWalk();
                if (GUILayout.Button("Run")) controller.PlayRun();
                UnityEditor.EditorGUILayout.EndHorizontal();
                
                // 动作状态
                UnityEditor.EditorGUILayout.LabelField("动作状态", UnityEditor.EditorStyles.boldLabel);
                UnityEditor.EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Jump")) controller.PlayJump();
                if (GUILayout.Button("Attack")) controller.PlayAttack();
                if (GUILayout.Button("Hurt")) controller.PlayHurt();
                UnityEditor.EditorGUILayout.EndHorizontal();
                
                // 其他控制
                UnityEditor.EditorGUILayout.LabelField("其他控制", UnityEditor.EditorStyles.boldLabel);
                UnityEditor.EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("回到上一状态")) controller.RevertToPreviousState();
                if (GUILayout.Button("清空队列")) controller.ClearAnimationQueue();
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

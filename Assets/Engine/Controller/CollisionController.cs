using UnityEngine;

namespace FartGame
{
    /// <summary>
    /// 单个对象的碰撞和渲染层级控制组件
    /// </summary>
    public class CollisionController : MonoBehaviour
    {
        [Header("对象类型")]
        [Tooltip("标记为玩家对象")]
        public bool isPlayer = false;
        
        [Tooltip("标记为敌人对象")]
        public bool isEnemy = false;
        
        [Header("渲染设置")]
        [Tooltip("要控制层级的渲染器")]
        public SpriteRenderer spriteRenderer;
        
        [Tooltip("手动设置的渲染层级偏移")]
        public int manualSortingOrderOffset = 0;
        
        [Tooltip("渲染层级缩放倍数")]
        public float sortingOrderScale = 100f;
        
        [Header("分层设置")]
        [Tooltip("手动分层优先级（数值越大越在上层）")]
        public int layerPriority = 0;
        
        private int baseSortingOrder = 0;
        
        void Awake()
        {
            // 自动查找SpriteRenderer
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }
            }
            
            if (spriteRenderer != null)
            {
                baseSortingOrder = spriteRenderer.sortingOrder;
            }
        }
        
        void Start()
        {
            // 注册到CollisionManager
            if (CollisionManager.Instance != null)
            {
                if (isPlayer)
                {
                    CollisionManager.Instance.RegisterPlayer(this);
                }
                else if (isEnemy)
                {
                    CollisionManager.Instance.RegisterEnemy(this);
                }
                else
                {
                    CollisionManager.Instance.RegisterObject(this);
                }
            }
            
            // 初始化渲染层级
            UpdateSortingOrder();
        }
        
        void Update()
        {
            // 持续更新渲染层级
            UpdateSortingOrder();
        }
        
        /// <summary>
        /// 更新渲染层级
        /// </summary>
        public void UpdateSortingOrder()
        {
            if (spriteRenderer == null) return;
            
            // 计算基础层级：Y值越小，sortingOrder越大（显示在上方）
            int positionBasedOrder = Mathf.RoundToInt(-transform.position.y * sortingOrderScale);
            
            // 加上手动分层优先级（优先级更高的在上层）
            int layerOffset = layerPriority * 1000; // 给分层足够大的间隔
            
            // 最终的sortingOrder
            int finalSortingOrder = baseSortingOrder + positionBasedOrder + layerOffset + manualSortingOrderOffset;
            
            spriteRenderer.sortingOrder = finalSortingOrder;
        }
        
        /// <summary>
        /// 设置手动分层优先级
        /// </summary>
        public void SetLayerPriority(int priority)
        {
            layerPriority = priority;
            UpdateSortingOrder();
        }
        
        /// <summary>
        /// 获取当前分层优先级
        /// </summary>
        public int GetLayerPriority()
        {
            return layerPriority;
        }
        
        void OnDestroy()
        {
            // 从CollisionManager注销
            if (CollisionManager.Instance != null)
            {
                CollisionManager.Instance.UnregisterObject(this);
            }
        }
    }
}

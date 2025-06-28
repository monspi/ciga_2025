using QFramework;
using UnityEngine;

namespace FartGame
{
    /// <summary>
    /// 简化版碰撞控制器，主要处理渲染层级
    /// </summary>
    public class CollisionController : MonoBehaviour, IController
    {
        [Header("渲染层级控制")]
        [Tooltip("精灵渲染器")]
        public SpriteRenderer spriteRenderer;
        
        [Header("对象类型")]
        [Tooltip("是否为玩家对象")]
        public bool isPlayer = false;
        
        [Tooltip("是否为敌人对象")]
        public bool isEnemy = false;
        
        [Header("层级设置")]
        [Tooltip("渲染层级缩放倍数")]
        public float sortingOrderScale = 100f;
        
        void Start()
        {
            // 如果没有指定spriteRenderer，尝试自动获取
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                
                // 如果还是没有，尝试从子对象获取
                if (spriteRenderer == null)
                {
                    spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }
            }
            
            // 如果是敌人，注册到CollisionManager
            if (isEnemy && CollisionManager.Instance != null)
            {
                EnemyController enemyController = GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    CollisionManager.Instance.OnEnemySpawned(enemyController);
                }
            }
        }
        
        void Update()
        {
            // 持续更新渲染层级
            UpdateSortingOrder();
        }
        
        /// <summary>
        /// 根据Y坐标更新渲染层级，Y值越小显示在越上方
        /// </summary>
        private void UpdateSortingOrder()
        {
            if (spriteRenderer != null)
            {
                // 将Y坐标转换为排序层级，Y值越小，sortingOrder越大（显示在上方）
                int sortingOrder = Mathf.RoundToInt(-transform.position.y * sortingOrderScale);
                spriteRenderer.sortingOrder = sortingOrder;
            }
        }
        
        /// <summary>
        /// 手动设置排序层级
        /// </summary>
        public void SetSortingOrder(int order)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = order;
            }
        }
        
        /// <summary>
        /// 获取当前排序层级
        /// </summary>
        public int GetSortingOrder()
        {
            return spriteRenderer != null ? spriteRenderer.sortingOrder : 0;
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameArchitecture.Interface;
        }
    }
}

using QFramework;
using UnityEngine;
using System.Collections.Generic;

namespace FartGame
{
    /// <summary>
    /// 全局碰撞管理器，统一管理所有碰撞相关的逻辑
    /// </summary>
    public class CollisionManager : MonoBehaviour, IController
    {
        [Header("碰撞设置")]
        [Tooltip("玩家与敌人的最小距离")]
        public float minPlayerEnemyDistance = 0.8f;
        
        [Tooltip("渲染层级缩放倍数")]
        public float sortingOrderScale = 100f;
        
        private PlayerController player;
        private List<EnemyController> enemies = new List<EnemyController>();
        private Dictionary<Transform, SpriteRenderer> renderers = new Dictionary<Transform, SpriteRenderer>();
        
        // 单例相关
        public static CollisionManager Instance { get; private set; }
        
        void Awake()
        {
            // 设置单例
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        void Start()
        {
            // 查找所有相关对象
            RefreshGameObjects();
        }
        
        void Update()
        {
            // 持续更新渲染层级
            UpdateAllSortingOrders();
            
            // 检查和限制玩家移动
            if (player != null)
            {
                CheckPlayerMovementRestrictions();
            }
        }
        
        /// <summary>
        /// 刷新游戏对象列表
        /// </summary>
        public void RefreshGameObjects()
        {
            // 查找玩家
            player = FindObjectOfType<PlayerController>();
            
            // 查找所有敌人
            enemies.Clear();
            enemies.AddRange(FindObjectsOfType<EnemyController>());
            
            // 收集所有需要管理渲染层级的对象
            renderers.Clear();
            
            if (player != null)
            {
                SpriteRenderer playerRenderer = GetSpriteRenderer(player.transform);
                if (playerRenderer != null)
                {
                    renderers[player.transform] = playerRenderer;
                }
            }
            
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    SpriteRenderer enemyRenderer = GetSpriteRenderer(enemy.transform);
                    if (enemyRenderer != null)
                    {
                        renderers[enemy.transform] = enemyRenderer;
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取对象的SpriteRenderer
        /// </summary>
        private SpriteRenderer GetSpriteRenderer(Transform target)
        {
            // 先检查自身
            SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
            if (renderer != null) return renderer;
            
            // 检查子对象
            renderer = target.GetComponentInChildren<SpriteRenderer>();
            return renderer;
        }
        
        /// <summary>
        /// 更新所有对象的渲染层级
        /// </summary>
        private void UpdateAllSortingOrders()
        {
            foreach (var kvp in renderers)
            {
                if (kvp.Key != null && kvp.Value != null)
                {
                    UpdateSortingOrder(kvp.Key, kvp.Value);
                }
            }
        }
        
        /// <summary>
        /// 更新单个对象的渲染层级
        /// </summary>
        private void UpdateSortingOrder(Transform target, SpriteRenderer renderer)
        {
            // Y值越小，sortingOrder越大（显示在上方）
            int sortingOrder = Mathf.RoundToInt(-target.position.y * sortingOrderScale);
            renderer.sortingOrder = sortingOrder;
        }
        
        /// <summary>
        /// 检查玩家移动限制
        /// </summary>
        private void CheckPlayerMovementRestrictions()
        {
            if (player == null) return;
            
            Vector3 playerPos = player.transform.position;
            bool needsPositionCorrection = false;
            Vector3 correctedPosition = playerPos;
            
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.CurrentStamina <= 0) continue;
                
                Vector3 enemyPos = enemy.transform.position;
                float distance = Vector2.Distance(playerPos, enemyPos);
                
                // 如果玩家距离敌人太近且试图移动到敌人上方，限制移动
                if (distance < minPlayerEnemyDistance && playerPos.y > enemyPos.y)
                {
                    // 将玩家推到敌人下方
                    correctedPosition.y = enemyPos.y - minPlayerEnemyDistance * 0.5f;
                    needsPositionCorrection = true;
                    break;
                }
            }
            
            if (needsPositionCorrection)
            {
                // 应用位置修正
                player.transform.position = correctedPosition;
                
                // 尝试更新模型中的位置
                try
                {
                    var playerModel = this.GetModel<PlayerModel>();
                    if (playerModel != null)
                    {
                        playerModel.Position.Value = correctedPosition;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to update player model position: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// 检查玩家是否可以移动到指定位置
        /// </summary>
        public bool CanPlayerMoveTo(Vector3 targetPosition)
        {
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.CurrentStamina <= 0) continue;
                
                Vector3 enemyPos = enemy.transform.position;
                float distance = Vector2.Distance(targetPosition, enemyPos);
                
                // 如果目标位置距离敌人太近且在敌人上方，禁止移动
                if (distance < minPlayerEnemyDistance && targetPosition.y > enemyPos.y)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取玩家在指定位置的修正位置
        /// </summary>
        public Vector3 GetCorrectedPlayerPosition(Vector3 targetPosition)
        {
            Vector3 correctedPosition = targetPosition;
            
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.CurrentStamina <= 0) continue;
                
                Vector3 enemyPos = enemy.transform.position;
                float distance = Vector2.Distance(targetPosition, enemyPos);
                
                // 如果距离太近且在敌人上方，修正位置
                if (distance < minPlayerEnemyDistance && targetPosition.y > enemyPos.y)
                {
                    correctedPosition.y = enemyPos.y - minPlayerEnemyDistance * 0.5f;
                    break;
                }
            }
            
            return correctedPosition;
        }
        
        /// <summary>
        /// 当敌人被击败时调用，更新敌人列表
        /// </summary>
        public void OnEnemyDefeated(EnemyController enemy)
        {
            // 移除被击败的敌人的渲染器引用
            if (renderers.ContainsKey(enemy.transform))
            {
                renderers.Remove(enemy.transform);
            }
        }
        
        /// <summary>
        /// 当新敌人生成时调用
        /// </summary>
        public void OnEnemySpawned(EnemyController enemy)
        {
            if (!enemies.Contains(enemy))
            {
                enemies.Add(enemy);
                
                // 添加渲染器引用
                SpriteRenderer renderer = GetSpriteRenderer(enemy.transform);
                if (renderer != null)
                {
                    renderers[enemy.transform] = renderer;
                }
            }
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameArchitecture.Interface;
        }
        
        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}

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
        
        [Tooltip("位置修正的平滑度")]
        public float positionCorrectionSmoothing = 0.1f;
        
        private PlayerController player;
        private CollisionController playerCollision;
        private List<EnemyController> enemies = new List<EnemyController>();
        private List<CollisionController> enemyCollisions = new List<CollisionController>();
        private List<CollisionController> allObjects = new List<CollisionController>();
        
        // 玩家位置追踪
        private Vector3 lastValidPlayerPosition;
        private bool isPlayerPositionValid = true;
        
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
            
            // 初始化玩家位置
            if (player != null)
            {
                lastValidPlayerPosition = player.transform.position;
            }
        }
        
        void Update()
        {
            // 持续更新渲染层级
            UpdateAllSortingOrders();
            
            // 检查玩家位置有效性（但不强制修正）
            if (player != null)
            {
                ValidatePlayerPosition();
            }
        }
        
        /// <summary>
        /// 注册玩家对象
        /// </summary>
        public void RegisterPlayer(CollisionController playerCollisionController)
        {
            playerCollision = playerCollisionController;
            player = playerCollisionController.GetComponent<PlayerController>();
            
            if (!allObjects.Contains(playerCollisionController))
            {
                allObjects.Add(playerCollisionController);
            }
            
            Debug.Log("玩家已注册到碰撞管理器");
        }
        
        /// <summary>
        /// 注册敌人对象
        /// </summary>
        public void RegisterEnemy(CollisionController enemyCollisionController)
        {
            if (!enemyCollisions.Contains(enemyCollisionController))
            {
                enemyCollisions.Add(enemyCollisionController);
                
                EnemyController enemyController = enemyCollisionController.GetComponent<EnemyController>();
                if (enemyController != null && !enemies.Contains(enemyController))
                {
                    enemies.Add(enemyController);
                }
            }
            
            if (!allObjects.Contains(enemyCollisionController))
            {
                allObjects.Add(enemyCollisionController);
            }
            
            Debug.Log($"敌人 {enemyCollisionController.gameObject.name} 已注册到碰撞管理器");
        }
        
        /// <summary>
        /// 注册普通对象
        /// </summary>
        public void RegisterObject(CollisionController collisionController)
        {
            if (!allObjects.Contains(collisionController))
            {
                allObjects.Add(collisionController);
            }
            
            Debug.Log($"对象 {collisionController.gameObject.name} 已注册到碰撞管理器");
        }
        
        /// <summary>
        /// 注销对象
        /// </summary>
        public void UnregisterObject(CollisionController collisionController)
        {
            allObjects.Remove(collisionController);
            enemyCollisions.Remove(collisionController);
            
            if (collisionController == playerCollision)
            {
                playerCollision = null;
                player = null;
            }
            
            EnemyController enemyController = collisionController.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemies.Remove(enemyController);
            }
        }
        
        /// <summary>
        /// 刷新游戏对象列表
        /// </summary>
        public void RefreshGameObjects()
        {
            // 清空现有列表
            allObjects.Clear();
            enemyCollisions.Clear();
            enemies.Clear();
            playerCollision = null;
            player = null;
            
            // 查找所有CollisionController
            CollisionController[] allControllers = FindObjectsOfType<CollisionController>();
            
            foreach (var controller in allControllers)
            {
                if (controller.isPlayer)
                {
                    RegisterPlayer(controller);
                }
                else if (controller.isEnemy)
                {
                    RegisterEnemy(controller);
                }
                else
                {
                    RegisterObject(controller);
                }
            }
            
            Debug.Log($"刷新完成：找到 {allObjects.Count} 个对象，其中玩家 {(playerCollision != null ? 1 : 0)} 个，敌人 {enemyCollisions.Count} 个");
        }
        
        /// <summary>
        /// 更新所有对象的渲染层级
        /// </summary>
        private void UpdateAllSortingOrders()
        {
            foreach (var obj in allObjects)
            {
                if (obj != null)
                {
                    obj.UpdateSortingOrder();
                }
            }
        }
        
        /// <summary>
        /// 验证玩家位置有效性（不强制修正）
        /// </summary>
        private void ValidatePlayerPosition()
        {
            if (player == null) return;
            
            Vector3 playerPos = player.transform.position;
            isPlayerPositionValid = IsPositionValid(playerPos);
            
            // 如果位置有效，更新最后有效位置
            if (isPlayerPositionValid)
            {
                lastValidPlayerPosition = playerPos;
            }
        }
        
        /// <summary>
        /// 检查位置是否有效（不与敌人重叠）
        /// </summary>
        private bool IsPositionValid(Vector3 position)
        {
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.IsDefeated) continue;
                
                Vector3 enemyPos = enemy.transform.position;
                float distance = Vector2.Distance(position, enemyPos);
                
                // 检查是否在禁止区域内
                if (distance < minPlayerEnemyDistance)
                {
                    // 阻挡规则：玩家不能越过敌人的Y值
                    // 无论从上往下还是从下往上，都不能进入敌人的Y值区域
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 检查玩家是否可以移动到指定位置
        /// </summary>
        public bool CanPlayerMoveTo(Vector3 targetPosition)
        {
            return IsPositionValid(targetPosition);
        }
        
        /// <summary>
        /// 获取玩家在指定位置的修正位置
        /// </summary>
        public Vector3 GetCorrectedPlayerPosition(Vector3 targetPosition)
        {
            Vector3 currentPos = player != null ? player.transform.position : Vector3.zero;
            
            // 如果目标位置有效，直接返回
            if (IsPositionValid(targetPosition))
            {
                return targetPosition;
            }
            
            // 如果目标位置无效，尝试找到最近的有效位置
            Vector3 correctedPosition = FindNearestValidPosition(targetPosition, currentPos);
            
            return correctedPosition;
        }
        
        /// <summary>
        /// 找到最近的有效位置
        /// </summary>
        private Vector3 FindNearestValidPosition(Vector3 targetPosition, Vector3 currentPosition)
        {
            // 找到阻挡的敌人
            EnemyController blockingEnemy = null;
            float minDistance = float.MaxValue;
            
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.IsDefeated) continue;
                
                Vector3 enemyPos = enemy.transform.position;
                float distance = Vector2.Distance(targetPosition, enemyPos);
                
                if (distance < minPlayerEnemyDistance)
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        blockingEnemy = enemy;
                    }
                }
            }
            
            if (blockingEnemy != null)
            {
                Vector3 enemyPos = blockingEnemy.transform.position;
                Vector3 correctedPos = currentPosition; // 使用当前位置作为基础，避免跳跃
                
                // 计算从当前位置到目标位置的方向
                Vector3 moveDirection = (targetPosition - currentPosition).normalized;
                Vector3 toEnemy = (enemyPos - currentPosition).normalized;
                
                // 计算安全位置：在敌人周围保持最小距离
                Vector3 safeOffset = -toEnemy * minPlayerEnemyDistance;
                Vector3 safePosition = enemyPos + safeOffset;
                
                // 如果可能，保持尽可能接近目标位置的移动
                // 只在移动方向上进行限制，避免不必要的位置修正
                float dotProduct = Vector3.Dot(moveDirection, toEnemy);
                if (dotProduct > 0) // 正在朝向敌人移动
                {
                    // 计算在当前移动路径上的最远安全位置
                    Vector3 projectedDirection = Vector3.Project(targetPosition - currentPosition, toEnemy);
                    float maxAllowedDistance = Vector3.Distance(currentPosition, enemyPos) - minPlayerEnemyDistance;
                    
                    if (projectedDirection.magnitude > maxAllowedDistance)
                    {
                        correctedPos = currentPosition + toEnemy * maxAllowedDistance;
                    }
                    else
                    {
                        correctedPos = targetPosition;
                    }
                }
                else
                {
                    // 不是朝向敌人移动，允许移动
                    correctedPos = targetPosition;
                }
                
                return correctedPos;
            }
            
            // 如果没有找到阻挡的敌人，返回目标位置
            return targetPosition;
        }
        
        /// <summary>
        /// 当敌人被击败时调用，更新敌人列表
        /// </summary>
        public void OnEnemyDefeated(EnemyController enemy)
        {
            Debug.Log($"敌人 {enemy.gameObject.name} 已被击败，从碰撞检测中移除");
        }
        
        /// <summary>
        /// 当新敌人生成时调用
        /// </summary>
        public void OnEnemySpawned(EnemyController enemy)
        {
            if (!enemies.Contains(enemy))
            {
                enemies.Add(enemy);
                
                // 查找对应的CollisionController
                CollisionController collisionController = enemy.GetComponent<CollisionController>();
                if (collisionController != null)
                {
                    RegisterEnemy(collisionController);
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

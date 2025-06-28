using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FartGame
{
    /// <summary>
    /// 敌人设置工具 - 专门用于批量配置敌人对象
    /// </summary>
    public class EnemySetupTool : MonoBehaviour
    {
        [Header("敌人配置")]
        [Tooltip("敌人配置数据")]
        public EnemyConfigSO enemyConfig;
        
        [Header("搜索设置")]
        [Tooltip("候选敌人对象（如果为空，会自动搜索）")]
        public List<GameObject> candidateEnemies = new List<GameObject>();
        
        [Tooltip("仅搜索子对象")]
        public bool searchChildrenOnly = true;
        
        [Tooltip("排除已配置的敌人")]
        public bool excludeConfiguredEnemies = true;
        
        [Header("组件设置")]
        [Tooltip("自动添加碰撞控制组件")]
        public bool addCollisionComponents = true;
        
        [Tooltip("自动添加2D物理组件")]
        public bool addPhysicsComponents = true;
        
        [Tooltip("设置为Trigger碰撞")]
        public bool setAsTrigger = true;
        
        [Header("分层设置")]
        [Tooltip("启用自动分层")]
        public bool enableAutoLayering = true;
        
        [Tooltip("基础分层优先级")]
        public int baseLayerPriority = 0;
        
        [Tooltip("分层间隔")]
        public int layerInterval = 100;
        
        private int processedCount = 0;
        
        /// <summary>
        /// 查找敌人对象
        /// </summary>
        [ContextMenu("查找敌人对象")]
        public void FindEnemies()
        {
            candidateEnemies.Clear();
            processedCount = 0;
            
            SpriteRenderer[] renderers;
            
            if (searchChildrenOnly)
            {
                renderers = GetComponentsInChildren<SpriteRenderer>();
            }
            else
            {
                renderers = FindObjectsOfType<SpriteRenderer>();
            }
            
            foreach (var renderer in renderers)
            {
                GameObject obj = renderer.gameObject;
                
                // 排除自身
                if (obj == gameObject) continue;
                
                // 如果启用排除已配置的敌人
                if (excludeConfiguredEnemies && obj.GetComponent<EnemyController>() != null)
                {
                    continue;
                }
                
                candidateEnemies.Add(obj);
            }
            
            Debug.Log($"找到 {candidateEnemies.Count} 个候选敌人对象");
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        
        /// <summary>
        /// 批量设置敌人
        /// </summary>
        [ContextMenu("批量设置敌人")]
        public void BatchSetupEnemies()
        {
            if (candidateEnemies.Count == 0)
            {
                Debug.LogWarning("没有候选敌人对象，请先查找敌人对象");
                return;
            }
            
            if (enemyConfig == null)
            {
                Debug.LogError("请先设置EnemyConfig配置文件");
                return;
            }
            
            processedCount = 0;
            int currentLayerPriority = baseLayerPriority;
            
            foreach (var enemy in candidateEnemies)
            {
                if (enemy == null) continue;
                
                try
                {
                    SetupSingleEnemy(enemy, currentLayerPriority);
                    processedCount++;
                    
                    if (enableAutoLayering)
                    {
                        currentLayerPriority += layerInterval;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"设置敌人 {enemy.name} 时出错: {e.Message}");
                }
            }
            
            Debug.Log($"敌人设置完成：成功配置 {processedCount}/{candidateEnemies.Count} 个敌人");
            
            // 确保CollisionManager存在
            EnsureCollisionManager();
        }
        
        /// <summary>
        /// 设置单个敌人
        /// </summary>
        private void SetupSingleEnemy(GameObject enemy, int layerPriority)
        {
            // 1. 设置标签
            enemy.tag = "Enemy";
            
            // 2. 添加或更新EnemyController
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController == null)
            {
                enemyController = enemy.AddComponent<EnemyController>();
            }
            enemyController.enemyConfig = enemyConfig;
            
            // 3. 添加碰撞控制组件
            if (addCollisionComponents)
            {
                SetupCollisionController(enemy, layerPriority);
            }
            
            // 4. 添加2D物理组件
            if (addPhysicsComponents)
            {
                SetupPhysicsComponents(enemy);
            }
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(enemy);
#endif
        }
        
        /// <summary>
        /// 设置碰撞控制器
        /// </summary>
        private void SetupCollisionController(GameObject enemy, int layerPriority)
        {
            CollisionController controller = enemy.GetComponent<CollisionController>();
            if (controller == null)
            {
                controller = enemy.AddComponent<CollisionController>();
            }
            
            controller.isEnemy = true;
            controller.isPlayer = false;
            controller.layerPriority = layerPriority;
            
            // 设置SpriteRenderer
            SpriteRenderer spriteRenderer = enemy.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                controller.spriteRenderer = spriteRenderer;
            }
        }
        
        /// <summary>
        /// 设置2D物理组件
        /// </summary>
        private void SetupPhysicsComponents(GameObject enemy)
        {
            // 添加Collider2D
            Collider2D collider = enemy.GetComponent<Collider2D>();
            if (collider == null)
            {
                BoxCollider2D boxCollider = enemy.AddComponent<BoxCollider2D>();
                boxCollider.isTrigger = setAsTrigger;
                
                // 自动调整大小
                SpriteRenderer spriteRenderer = enemy.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    boxCollider.size = spriteRenderer.sprite.bounds.size;
                }
            }
            else
            {
                collider.isTrigger = setAsTrigger;
            }
            
            // 添加Rigidbody2D
            Rigidbody2D rigidbody = enemy.GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                rigidbody = enemy.AddComponent<Rigidbody2D>();
                rigidbody.bodyType = RigidbodyType2D.Kinematic;
                rigidbody.freezeRotation = true;
                rigidbody.gravityScale = 0f;
            }
        }
        
        /// <summary>
        /// 确保CollisionManager存在
        /// </summary>
        private void EnsureCollisionManager()
        {
            if (CollisionManager.Instance == null)
            {
                GameObject managerObj = new GameObject("CollisionManager");
                managerObj.AddComponent<CollisionManager>();
                Debug.Log("已创建CollisionManager");
            }
            else
            {
                // 刷新CollisionManager的对象列表
                CollisionManager.Instance.RefreshGameObjects();
            }
        }
        
        /// <summary>
        /// 清理敌人设置
        /// </summary>
        [ContextMenu("清理敌人设置")]
        public void CleanupEnemies()
        {
            if (candidateEnemies.Count == 0)
            {
                Debug.LogWarning("没有要清理的敌人对象");
                return;
            }
            
            int cleanedCount = 0;
            
            foreach (var enemy in candidateEnemies)
            {
                if (enemy == null) continue;
                
                // 移除组件
                var enemyController = enemy.GetComponent<EnemyController>();
                var collisionController = enemy.GetComponent<CollisionController>();
                var collider = enemy.GetComponent<Collider2D>();
                var rigidbody = enemy.GetComponent<Rigidbody2D>();
                
                if (enemyController != null) DestroyImmediate(enemyController);
                if (collisionController != null) DestroyImmediate(collisionController);
                if (collider != null) DestroyImmediate(collider);
                if (rigidbody != null) DestroyImmediate(rigidbody);
                
                // 重置标签
                enemy.tag = "Untagged";
                
                cleanedCount++;
                
#if UNITY_EDITOR
                EditorUtility.SetDirty(enemy);
#endif
            }
            
            Debug.Log($"清理完成：处理了 {cleanedCount} 个敌人对象");
        }
        
        /// <summary>
        /// 重新设置分层
        /// </summary>
        [ContextMenu("重新设置分层")]
        public void ResetLayers()
        {
            if (!enableAutoLayering) return;
            
            int currentLayerPriority = baseLayerPriority;
            int updatedCount = 0;
            
            foreach (var enemy in candidateEnemies)
            {
                if (enemy == null) continue;
                
                CollisionController controller = enemy.GetComponent<CollisionController>();
                if (controller != null)
                {
                    controller.SetLayerPriority(currentLayerPriority);
                    currentLayerPriority += layerInterval;
                    updatedCount++;
                    
#if UNITY_EDITOR
                    EditorUtility.SetDirty(controller);
#endif
                }
            }
            
            Debug.Log($"重新设置了 {updatedCount} 个敌人的分层");
        }
        
        /// <summary>
        /// 显示当前状态
        /// </summary>
        [ContextMenu("显示状态信息")]
        public void ShowStatus()
        {
            Debug.Log($"敌人设置工具状态:\n" +
                     $"- 候选对象: {candidateEnemies.Count}\n" +
                     $"- 已处理: {processedCount}\n" +
                     $"- 配置文件: {(enemyConfig != null ? enemyConfig.name : "未设置")}\n" +
                     $"- CollisionManager: {(CollisionManager.Instance != null ? "存在" : "不存在")}");
        }
    }
}

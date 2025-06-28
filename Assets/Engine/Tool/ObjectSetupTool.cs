using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FartGame
{
    /// <summary>
    /// 对象设置工具 - 用于给图片素材配置Collider、Rigidbody和对应的EnemyController
    /// 支持手动分层以避免遮挡
    /// </summary>
    public class ObjectSetupTool : MonoBehaviour
    {
        [Header("批量设置选项")]
        [Tooltip("要处理的对象列表（留空则自动查找有SpriteRenderer的对象）")]
        public List<GameObject> targetObjects = new List<GameObject>();
        
        [Tooltip("敌人配置数据")]
        public EnemyConfigSO enemyConfig;
        
        [Header("组件设置")]
        [Tooltip("自动添加Collider2D")]
        public bool addCollider = true;
        
        [Tooltip("Collider类型")]
        public ColliderType colliderType = ColliderType.Box;
        
        [Tooltip("设置为Trigger")]
        public bool isTrigger = true;
        
        [Tooltip("自动添加Rigidbody2D")]
        public bool addRigidbody = true;
        
        [Tooltip("Rigidbody类型")]
        public RigidbodyType2D bodyType = RigidbodyType2D.Kinematic;
        
        [Tooltip("冻结旋转")]
        public bool freezeRotation = true;
        
        [Header("敌人控制器设置")]
        [Tooltip("自动添加EnemyController")]
        public bool addEnemyController = true;
        
        [Tooltip("设置敌人标签")]
        public bool setEnemyTag = true;
        
        [Header("碰撞控制设置")]
        [Tooltip("自动添加CollisionController")]
        public bool addCollisionController = true;
        
        [Tooltip("所有对象标记为敌人")]
        public bool markAsEnemy = true;
        
        [Header("分层设置")]
        [Tooltip("启用手动分层")]
        public bool enableManualLayering = true;
        
        [Tooltip("分层间隔（建议使用较大的数值）")]
        public int layerInterval = 1000;
        
        [Tooltip("基础分层优先级")]
        public int baseLayerPriority = 0;
        
        [Header("自动查找设置")]
        [Tooltip("查找范围：仅子对象")]
        public bool searchChildrenOnly = true;
        
        [Tooltip("排除已经配置过的对象")]
        public bool excludeConfiguredObjects = true;
        
        public enum ColliderType
        {
            Box,
            Circle,
            Capsule
        }
        
        /// <summary>
        /// 查找候选对象
        /// </summary>
        [ContextMenu("查找候选对象")]
        public void FindCandidateObjects()
        {
            targetObjects.Clear();
            
            GameObject[] searchObjects;
            
            if (searchChildrenOnly)
            {
                // 只搜索子对象
                SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>();
                searchObjects = childRenderers.Select(sr => sr.gameObject).ToArray();
            }
            else
            {
                // 搜索场景中所有对象
                SpriteRenderer[] allRenderers = FindObjectsOfType<SpriteRenderer>();
                searchObjects = allRenderers.Select(sr => sr.gameObject).ToArray();
            }
            
            foreach (var obj in searchObjects)
            {
                // 排除自身
                if (obj == gameObject) continue;
                
                // 如果启用了排除已配置的对象
                if (excludeConfiguredObjects)
                {
                    if (obj.GetComponent<EnemyController>() != null) continue;
                    if (obj.GetComponent<CollisionController>() != null) continue;
                }
                
                targetObjects.Add(obj);
            }
            
            Debug.Log($"找到 {targetObjects.Count} 个候选对象");
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        
        /// <summary>
        /// 批量设置所有对象
        /// </summary>
        [ContextMenu("批量设置对象")]
        public void BatchSetupObjects()
        {
            if (targetObjects.Count == 0)
            {
                Debug.LogWarning("没有要处理的对象，请先查找候选对象");
                return;
            }
            
            if (enemyConfig == null && addEnemyController)
            {
                Debug.LogError("请先设置EnemyConfig配置文件");
                return;
            }
            
            int successCount = 0;
            int currentLayerPriority = baseLayerPriority;
            
            for (int i = 0; i < targetObjects.Count; i++)
            {
                GameObject obj = targetObjects[i];
                if (obj == null) continue;
                
                try
                {
                    SetupSingleObject(obj, currentLayerPriority);
                    successCount++;
                    
                    // 增加分层优先级
                    if (enableManualLayering)
                    {
                        currentLayerPriority += layerInterval;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"设置对象 {obj.name} 时出错: {e.Message}");
                }
            }
            
            Debug.Log($"批量设置完成：成功配置 {successCount}/{targetObjects.Count} 个对象");
            
            // 确保CollisionManager存在
            EnsureCollisionManager();
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        
        /// <summary>
        /// 设置单个对象
        /// </summary>
        private void SetupSingleObject(GameObject obj, int layerPriority)
        {
            Debug.Log($"正在设置对象: {obj.name}");
            
            // 设置标签
            if (setEnemyTag)
            {
                obj.tag = "Enemy";
            }
            
            // 添加Collider2D
            if (addCollider)
            {
                SetupCollider(obj);
            }
            
            // 添加Rigidbody2D
            if (addRigidbody)
            {
                SetupRigidbody(obj);
            }
            
            // 添加EnemyController
            if (addEnemyController)
            {
                SetupEnemyController(obj);
            }
            
            // 添加CollisionController
            if (addCollisionController)
            {
                SetupCollisionController(obj, layerPriority);
            }
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(obj);
#endif
        }
        
        /// <summary>
        /// 设置Collider2D
        /// </summary>
        private void SetupCollider(GameObject obj)
        {
            Collider2D existingCollider = obj.GetComponent<Collider2D>();
            if (existingCollider != null) return;
            
            Collider2D collider = null;
            
            switch (colliderType)
            {
                case ColliderType.Box:
                    collider = obj.AddComponent<BoxCollider2D>();
                    break;
                case ColliderType.Circle:
                    collider = obj.AddComponent<CircleCollider2D>();
                    break;
                case ColliderType.Capsule:
                    collider = obj.AddComponent<CapsuleCollider2D>();
                    break;
            }
            
            if (collider != null)
            {
                collider.isTrigger = isTrigger;
                
                // 自动调整碰撞体大小
                AutoSizeCollider(collider, obj);
            }
        }
        
        /// <summary>
        /// 自动调整碰撞体大小
        /// </summary>
        private void AutoSizeCollider(Collider2D collider, GameObject obj)
        {
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null) return;
            
            Bounds spriteBounds = spriteRenderer.sprite.bounds;
            
            if (collider is BoxCollider2D boxCollider)
            {
                boxCollider.size = spriteBounds.size;
            }
            else if (collider is CircleCollider2D circleCollider)
            {
                circleCollider.radius = Mathf.Max(spriteBounds.size.x, spriteBounds.size.y) * 0.5f;
            }
            else if (collider is CapsuleCollider2D capsuleCollider)
            {
                capsuleCollider.size = spriteBounds.size;
            }
        }
        
        /// <summary>
        /// 设置Rigidbody2D
        /// </summary>
        private void SetupRigidbody(GameObject obj)
        {
            Rigidbody2D existingRigidbody = obj.GetComponent<Rigidbody2D>();
            if (existingRigidbody != null) return;
            
            Rigidbody2D rigidbody = obj.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = bodyType;
            rigidbody.freezeRotation = freezeRotation;
            rigidbody.gravityScale = 0f; // 2D游戏通常不需要重力
        }
        
        /// <summary>
        /// 设置EnemyController
        /// </summary>
        private void SetupEnemyController(GameObject obj)
        {
            EnemyController existingController = obj.GetComponent<EnemyController>();
            if (existingController != null) return;
            
            EnemyController controller = obj.AddComponent<EnemyController>();
            controller.enemyConfig = enemyConfig;
        }
        
        /// <summary>
        /// 设置CollisionController
        /// </summary>
        private void SetupCollisionController(GameObject obj, int layerPriority)
        {
            CollisionController existingController = obj.GetComponent<CollisionController>();
            if (existingController != null) return;
            
            CollisionController controller = obj.AddComponent<CollisionController>();
            controller.isEnemy = markAsEnemy;
            controller.layerPriority = layerPriority;
            
            // 设置SpriteRenderer
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                controller.spriteRenderer = spriteRenderer;
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
        }
        
        /// <summary>
        /// 清理所有设置
        /// </summary>
        [ContextMenu("清理所有设置")]
        public void CleanupAllSettings()
        {
            if (targetObjects.Count == 0)
            {
                Debug.LogWarning("没有要清理的对象");
                return;
            }
            
            int cleanupCount = 0;
            
            foreach (var obj in targetObjects)
            {
                if (obj == null) continue;
                
                // 移除组件
                DestroyImmediate(obj.GetComponent<EnemyController>());
                DestroyImmediate(obj.GetComponent<CollisionController>());
                DestroyImmediate(obj.GetComponent<Collider2D>());
                DestroyImmediate(obj.GetComponent<Rigidbody2D>());
                
                // 重置标签
                obj.tag = "Untagged";
                
                cleanupCount++;
                
#if UNITY_EDITOR
                EditorUtility.SetDirty(obj);
#endif
            }
            
            Debug.Log($"清理完成：处理了 {cleanupCount} 个对象");
        }
        
        /// <summary>
        /// 重新排列分层
        /// </summary>
        [ContextMenu("重新排列分层")]
        public void RearrangeLayers()
        {
            if (!enableManualLayering) return;
            
            var collisionControllers = targetObjects
                .Where(obj => obj != null)
                .Select(obj => obj.GetComponent<CollisionController>())
                .Where(controller => controller != null)
                .ToList();
            
            for (int i = 0; i < collisionControllers.Count; i++)
            {
                int newLayerPriority = baseLayerPriority + (i * layerInterval);
                collisionControllers[i].SetLayerPriority(newLayerPriority);
                
#if UNITY_EDITOR
                EditorUtility.SetDirty(collisionControllers[i]);
#endif
            }
            
            Debug.Log($"重新排列了 {collisionControllers.Count} 个对象的分层");
        }
        
        /// <summary>
        /// 获取当前对象分层信息
        /// </summary>
        [ContextMenu("显示分层信息")]
        public void ShowLayerInfo()
        {
            if (targetObjects.Count == 0) return;
            
            var layerInfo = new List<string>();
            
            foreach (var obj in targetObjects)
            {
                if (obj == null) continue;
                
                CollisionController controller = obj.GetComponent<CollisionController>();
                if (controller != null)
                {
                    layerInfo.Add($"{obj.name}: 分层优先级 {controller.GetLayerPriority()}");
                }
                else
                {
                    layerInfo.Add($"{obj.name}: 无CollisionController");
                }
            }
            
            Debug.Log("分层信息:\n" + string.Join("\n", layerInfo));
        }
    }
}

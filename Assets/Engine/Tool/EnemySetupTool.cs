using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FartGame
{
    /// <summary>
    /// 敌人批量设置工具
    /// 提供一站式敌人对象配置功能
    /// </summary>
    public class EnemySetupTool : MonoBehaviour
    {
        [Header("敌人配置")]
        [Tooltip("要应用到所有敌人的配置数据")]
        public EnemyConfigSO enemyConfig;
        
        [Header("批量设置选项")]
        [Tooltip("是否自动修改对象标签为Enemy")]
        public bool autoSetEnemyTag = true;
        
        [Tooltip("是否自动添加EnemyController组件")]
        public bool autoAddEnemyController = true;
        
        [Tooltip("是否自动添加碰撞控制组件")]
        public bool autoAddCollisionController = true;
        
        [Tooltip("是否自动设置2D物理组件")]
        public bool autoSetup2DPhysics = true;
        
        [Header("物理设置")]
        [Tooltip("碰撞器类型")]
        public ColliderType colliderType = ColliderType.BoxCollider2D;
        
        [Tooltip("是否设置为触发器")]
        public bool isTrigger = false;
        
        [Tooltip("是否添加Rigidbody2D")]
        public bool addRigidbody2D = false;
        
        [Tooltip("Rigidbody2D类型")]
        public RigidbodyType2D rigidbodyType = RigidbodyType2D.Kinematic;
        
        [Header("查找设置")]
        [Tooltip("按名称模式查找敌人对象")]
        public string enemyNamePattern = "Enemy";
        
        [Tooltip("是否包含子字符串匹配")]
        public bool useContainsMatch = true;
        
        [Tooltip("手动指定的敌人对象列表")]
        public List<GameObject> manualEnemyList = new List<GameObject>();
        
        [Header("渲染设置")]
        [Tooltip("是否自动设置排序层")]
        public bool autoSetSortingLayer = true;
        
        [Tooltip("排序层名称")]
        public string sortingLayerName = "Default";
        
        [Tooltip("基础排序顺序")]
        public int baseSortingOrder = 0;
        
        public enum ColliderType
        {
            BoxCollider2D,
            CircleCollider2D,
            PolygonCollider2D,
            CapsuleCollider2D
        }
        
        /// <summary>
        /// 查找场景中的所有敌人候选对象
        /// </summary>
        [ContextMenu("查找敌人对象")]
        public void FindEnemyObjects()
        {
            List<GameObject> foundEnemies = new List<GameObject>();
            
            // 首先通过标签查找
            GameObject[] taggedEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            foundEnemies.AddRange(taggedEnemies);
            
            // 然后通过名称模式查找
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (MatchesNamePattern(obj.name) && !foundEnemies.Contains(obj))
                {
                    foundEnemies.Add(obj);
                }
            }
            
            // 更新手动列表（保留现有的，添加新找到的）
            foreach (GameObject enemy in foundEnemies)
            {
                if (!manualEnemyList.Contains(enemy))
                {
                    manualEnemyList.Add(enemy);
                }
            }
            
            Debug.Log($"找到 {foundEnemies.Count} 个敌人对象，总共 {manualEnemyList.Count} 个待配置对象");
        }
        
        /// <summary>
        /// 检查名称是否匹配模式
        /// </summary>
        private bool MatchesNamePattern(string objectName)
        {
            if (string.IsNullOrEmpty(enemyNamePattern)) return false;
            
            if (useContainsMatch)
            {
                return objectName.ToLower().Contains(enemyNamePattern.ToLower());
            }
            else
            {
                return objectName.ToLower().StartsWith(enemyNamePattern.ToLower());
            }
        }
        
        /// <summary>
        /// 批量设置所有敌人对象
        /// </summary>
        [ContextMenu("批量设置敌人")]
        public void SetupAllEnemies()
        {
            if (enemyConfig == null)
            {
                Debug.LogError("请先指定EnemyConfig配置文件！");
                return;
            }
            
            // 先查找敌人对象
            FindEnemyObjects();
            
            if (manualEnemyList.Count == 0)
            {
                Debug.LogWarning("没有找到要配置的敌人对象！");
                return;
            }
            
            int successCount = 0;
            int errorCount = 0;
            
            foreach (GameObject enemyObj in manualEnemyList)
            {
                if (enemyObj == null) continue;
                
                try
                {
                    SetupSingleEnemy(enemyObj);
                    successCount++;
                    Debug.Log($"成功配置敌人: {enemyObj.name}");
                }
                catch (System.Exception e)
                {
                    errorCount++;
                    Debug.LogError($"配置敌人 {enemyObj.name} 时出错: {e.Message}");
                }
            }
            
            // 确保有CollisionManager
            if (autoAddCollisionController)
            {
                EnsureCollisionManager();
            }
            
            Debug.Log($"批量设置完成！成功: {successCount}, 失败: {errorCount}");
        }
        
        /// <summary>
        /// 配置单个敌人对象
        /// </summary>
        private void SetupSingleEnemy(GameObject enemyObj)
        {
            // 1. 设置标签
            if (autoSetEnemyTag)
            {
                enemyObj.tag = "Enemy";
            }
            
            // 2. 添加/配置EnemyController
            if (autoAddEnemyController)
            {
                EnemyController enemyController = enemyObj.GetComponent<EnemyController>();
                if (enemyController == null)
                {
                    enemyController = enemyObj.AddComponent<EnemyController>();
                }
                enemyController.enemyConfig = enemyConfig;
            }
            
            // 3. 添加/配置CollisionController
            if (autoAddCollisionController)
            {
                CollisionController collisionController = enemyObj.GetComponent<CollisionController>();
                if (collisionController == null)
                {
                    collisionController = enemyObj.AddComponent<CollisionController>();
                }
                collisionController.isEnemy = true;
                collisionController.isPlayer = false;
                
                // 自动设置SpriteRenderer
                if (collisionController.spriteRenderer == null)
                {
                    collisionController.spriteRenderer = enemyObj.GetComponent<SpriteRenderer>();
                    if (collisionController.spriteRenderer == null)
                    {
                        collisionController.spriteRenderer = enemyObj.GetComponentInChildren<SpriteRenderer>();
                    }
                }
            }
            
            // 4. 设置2D物理组件
            if (autoSetup2DPhysics)
            {
                Setup2DPhysics(enemyObj);
            }
            
            // 5. 设置渲染层级
            if (autoSetSortingLayer)
            {
                SetupSpriteRenderer(enemyObj);
            }
        }
        
        /// <summary>
        /// 设置2D物理组件
        /// </summary>
        private void Setup2DPhysics(GameObject enemyObj)
        {
            // 添加Rigidbody2D（如果需要）
            if (addRigidbody2D)
            {
                Rigidbody2D rb2d = enemyObj.GetComponent<Rigidbody2D>();
                if (rb2d == null)
                {
                    rb2d = enemyObj.AddComponent<Rigidbody2D>();
                }
                rb2d.bodyType = rigidbodyType;
            }
            
            // 添加Collider2D
            Collider2D collider = enemyObj.GetComponent<Collider2D>();
            if (collider == null)
            {
                switch (colliderType)
                {
                    case ColliderType.BoxCollider2D:
                        collider = enemyObj.AddComponent<BoxCollider2D>();
                        break;
                    case ColliderType.CircleCollider2D:
                        collider = enemyObj.AddComponent<CircleCollider2D>();
                        break;
                    case ColliderType.PolygonCollider2D:
                        collider = enemyObj.AddComponent<PolygonCollider2D>();
                        break;
                    case ColliderType.CapsuleCollider2D:
                        collider = enemyObj.AddComponent<CapsuleCollider2D>();
                        break;
                }
            }
            
            if (collider != null)
            {
                collider.isTrigger = isTrigger;
            }
        }
        
        /// <summary>
        /// 设置SpriteRenderer
        /// </summary>
        private void SetupSpriteRenderer(GameObject enemyObj)
        {
            SpriteRenderer spriteRenderer = enemyObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = enemyObj.GetComponentInChildren<SpriteRenderer>();
            }
            
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingLayerName = sortingLayerName;
                spriteRenderer.sortingOrder = baseSortingOrder;
            }
        }
        
        /// <summary>
        /// 确保场景中有CollisionManager
        /// </summary>
        private void EnsureCollisionManager()
        {
            CollisionManager existingManager = FindObjectOfType<CollisionManager>();
            if (existingManager == null)
            {
                GameObject managerObj = new GameObject("CollisionManager");
                managerObj.AddComponent<CollisionManager>();
                Debug.Log("自动创建了CollisionManager");
            }
        }
        
        /// <summary>
        /// 清理配置（移除组件）
        /// </summary>
        [ContextMenu("清理敌人配置")]
        public void CleanupEnemies()
        {
            if (manualEnemyList.Count == 0)
            {
                Debug.LogWarning("没有要清理的敌人对象！");
                return;
            }
            
            foreach (GameObject enemyObj in manualEnemyList)
            {
                if (enemyObj == null) continue;
                
                // 移除EnemyController
                EnemyController enemyController = enemyObj.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    DestroyImmediate(enemyController);
                }
                
                // 移除CollisionController
                CollisionController collisionController = enemyObj.GetComponent<CollisionController>();
                if (collisionController != null)
                {
                    DestroyImmediate(collisionController);
                }
                
                // 重置标签
                if (enemyObj.tag == "Enemy")
                {
                    enemyObj.tag = "Untagged";
                }
            }
            
            Debug.Log($"清理了 {manualEnemyList.Count} 个敌人对象的配置");
        }
        
        /// <summary>
        /// 重置列表
        /// </summary>
        [ContextMenu("清空敌人列表")]
        public void ClearEnemyList()
        {
            manualEnemyList.Clear();
            Debug.Log("已清空敌人列表");
        }
        
        /// <summary>
        /// 显示当前配置状态
        /// </summary>
        [ContextMenu("显示配置状态")]
        public void ShowConfigStatus()
        {
            Debug.Log("=== 敌人设置工具状态 ===");
            Debug.Log($"敌人配置文件: {(enemyConfig != null ? enemyConfig.name : "未设置")}");
            Debug.Log($"找到的敌人对象数量: {manualEnemyList.Count}");
            Debug.Log($"名称匹配模式: {enemyNamePattern}");
            Debug.Log($"自动设置标签: {autoSetEnemyTag}");
            Debug.Log($"自动添加控制器: {autoAddEnemyController}");
            Debug.Log($"自动添加碰撞控制: {autoAddCollisionController}");
            Debug.Log($"自动设置2D物理: {autoSetup2DPhysics}");
            
            // 检查每个敌人的配置状态
            for (int i = 0; i < manualEnemyList.Count; i++)
            {
                GameObject enemy = manualEnemyList[i];
                if (enemy == null) continue;
                
                bool hasTag = enemy.tag == "Enemy";
                bool hasController = enemy.GetComponent<EnemyController>() != null;
                bool hasCollision = enemy.GetComponent<CollisionController>() != null;
                bool hasCollider = enemy.GetComponent<Collider2D>() != null;
                
                Debug.Log($"敌人 [{i}] {enemy.name}: 标签✓{hasTag} 控制器✓{hasController} 碰撞✓{hasCollision} 物理✓{hasCollider}");
            }
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// 在Inspector中创建自定义按钮
        /// </summary>
        [CustomEditor(typeof(EnemySetupTool))]
        public class EnemySetupToolEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                
                EnemySetupTool tool = (EnemySetupTool)target;
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("快速操作", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("查找敌人对象", GUILayout.Height(30)))
                {
                    tool.FindEnemyObjects();
                }
                if (GUILayout.Button("批量设置敌人", GUILayout.Height(30)))
                {
                    tool.SetupAllEnemies();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("显示配置状态"))
                {
                    tool.ShowConfigStatus();
                }
                if (GUILayout.Button("清理配置"))
                {
                    if (EditorUtility.DisplayDialog("确认清理", "确定要清理所有敌人的配置吗？此操作不可撤销。", "确定", "取消"))
                    {
                        tool.CleanupEnemies();
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                if (GUILayout.Button("清空敌人列表"))
                {
                    tool.ClearEnemyList();
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("使用步骤：\n1. 设置EnemyConfig配置文件\n2. 调整查找和设置选项\n3. 点击'查找敌人对象'\n4. 点击'批量设置敌人'", MessageType.Info);
            }
        }
        #endif
    }
}

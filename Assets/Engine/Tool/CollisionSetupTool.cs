using UnityEngine;
using UnityEditor;

namespace FartGame
{
    /// <summary>
    /// 编辑器工具，用于快速设置碰撞系统
    /// </summary>
    public class CollisionSetupTool : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("快速设置")]
        [Tooltip("点击此按钮为场景中的所有对象自动添加碰撞组件")]
        public bool setupAllObjects = false;
        
        [Header("自动检测设置")]
        [Tooltip("自动为Player标签的对象添加玩家组件")]
        public bool autoSetupPlayer = true;
        
        [Tooltip("自动为Enemy标签的对象添加敌人组件")]
        public bool autoSetupEnemies = true;
        
        [Header("手动设置")]
        [Tooltip("手动指定的玩家对象")]
        public GameObject playerObject;
        
        [Tooltip("手动指定的敌人对象列表")]
        public GameObject[] enemyObjects;
        
        void OnValidate()
        {
            if (setupAllObjects)
            {
                setupAllObjects = false;
                SetupAllObjects();
            }
        }
        
        [ContextMenu("设置所有对象")]
        public void SetupAllObjects()
        {
            Debug.Log("开始设置碰撞系统...");
            
            // 创建CollisionManager（如果不存在）
            CreateCollisionManager();
            
            // 设置玩家
            if (autoSetupPlayer || playerObject != null)
            {
                SetupPlayer();
            }
            
            // 设置敌人
            if (autoSetupEnemies || (enemyObjects != null && enemyObjects.Length > 0))
            {
                SetupEnemies();
            }
            
            Debug.Log("碰撞系统设置完成！");
        }
        
        private void CreateCollisionManager()
        {
            CollisionManager existingManager = FindObjectOfType<CollisionManager>();
            if (existingManager == null)
            {
                GameObject managerObj = new GameObject("CollisionManager");
                managerObj.AddComponent<CollisionManager>();
                Debug.Log("已创建CollisionManager");
            }
            else
            {
                Debug.Log("CollisionManager已存在");
            }
        }
        
        private void SetupPlayer()
        {
            GameObject player = playerObject;
            
            if (player == null && autoSetupPlayer)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }
            
            if (player != null)
            {
                SetupPlayerObject(player);
                Debug.Log($"已设置玩家对象: {player.name}");
            }
            else
            {
                Debug.LogWarning("未找到玩家对象！");
            }
        }
        
        private void SetupPlayerObject(GameObject player)
        {
            // 添加CollisionController
            CollisionController collisionController = player.GetComponent<CollisionController>();
            if (collisionController == null)
            {
                collisionController = player.AddComponent<CollisionController>();
            }
            
            collisionController.isPlayer = true;
            collisionController.isEnemy = false;
            
            // 设置SpriteRenderer
            if (collisionController.spriteRenderer == null)
            {
                collisionController.spriteRenderer = player.GetComponent<SpriteRenderer>();
                if (collisionController.spriteRenderer == null)
                {
                    collisionController.spriteRenderer = player.GetComponentInChildren<SpriteRenderer>();
                }
            }
            
            // 添加AutoSetupCollision
            AutoSetupCollision autoSetup = player.GetComponent<AutoSetupCollision>();
            if (autoSetup == null)
            {
                autoSetup = player.AddComponent<AutoSetupCollision>();
            }
            autoSetup.isPlayer = true;
            autoSetup.isEnemy = false;
        }
        
        private void SetupEnemies()
        {
            GameObject[] enemies = null;
            
            if (enemyObjects != null && enemyObjects.Length > 0)
            {
                enemies = enemyObjects;
            }
            else if (autoSetupEnemies)
            {
                enemies = GameObject.FindGameObjectsWithTag("Enemy");
            }
            
            if (enemies != null && enemies.Length > 0)
            {
                foreach (var enemy in enemies)
                {
                    if (enemy != null)
                    {
                        SetupEnemyObject(enemy);
                        Debug.Log($"已设置敌人对象: {enemy.name}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("未找到敌人对象！");
            }
        }
        
        private void SetupEnemyObject(GameObject enemy)
        {
            // 添加CollisionController
            CollisionController collisionController = enemy.GetComponent<CollisionController>();
            if (collisionController == null)
            {
                collisionController = enemy.AddComponent<CollisionController>();
            }
            
            collisionController.isPlayer = false;
            collisionController.isEnemy = true;
            
            // 设置SpriteRenderer
            if (collisionController.spriteRenderer == null)
            {
                collisionController.spriteRenderer = enemy.GetComponent<SpriteRenderer>();
                if (collisionController.spriteRenderer == null)
                {
                    collisionController.spriteRenderer = enemy.GetComponentInChildren<SpriteRenderer>();
                }
            }
            
            // 添加AutoSetupCollision
            AutoSetupCollision autoSetup = enemy.GetComponent<AutoSetupCollision>();
            if (autoSetup == null)
            {
                autoSetup = enemy.AddComponent<AutoSetupCollision>();
            }
            autoSetup.isPlayer = false;
            autoSetup.isEnemy = true;
        }
        
        [ContextMenu("清除所有碰撞组件")]
        public void ClearAllCollisionComponents()
        {
            // 移除所有CollisionController
            CollisionController[] controllers = FindObjectsOfType<CollisionController>();
            foreach (var controller in controllers)
            {
                DestroyImmediate(controller);
            }
            
            // 移除所有AutoSetupCollision
            AutoSetupCollision[] autoSetups = FindObjectsOfType<AutoSetupCollision>();
            foreach (var autoSetup in autoSetups)
            {
                DestroyImmediate(autoSetup);
            }
            
            // 移除CollisionManager
            CollisionManager manager = FindObjectOfType<CollisionManager>();
            if (manager != null)
            {
                DestroyImmediate(manager.gameObject);
            }
            
            Debug.Log("已清除所有碰撞组件");
        }
#endif
    }
}

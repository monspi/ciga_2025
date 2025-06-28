using UnityEngine;
using QFramework;

namespace FartGame
{
    /// <summary>
    /// 碰撞系统测试脚本
    /// </summary>
    public class CollisionSystemTest : MonoBehaviour, IController
    {
        [Header("测试设置")]
        [Tooltip("是否启用调试日志")]
        public bool enableDebugLog = true;
        
        [Tooltip("测试移动速度")]
        public float testMoveSpeed = 2f;
        
        [Header("测试对象")]
        [Tooltip("测试用的玩家对象")]
        public GameObject testPlayer;
        
        [Tooltip("测试用的敌人对象")]
        public GameObject testEnemy;
        
        private bool isTestingMovement = false;
        private Vector3 originalPlayerPos;
        
        void Start()
        {
            if (enableDebugLog)
            {
                Debug.Log("碰撞系统测试开始");
                LogSystemStatus();
            }
        }
        
        void Update()
        {
            // 按T键开始/停止移动测试
            if (Input.GetKeyDown(KeyCode.T))
            {
                ToggleMovementTest();
            }
            
            // 按L键输出系统状态
            if (Input.GetKeyDown(KeyCode.L))
            {
                LogSystemStatus();
            }
            
            // 执行移动测试
            if (isTestingMovement && testPlayer != null && testEnemy != null)
            {
                PerformMovementTest();
            }
        }
        
        private void ToggleMovementTest()
        {
            isTestingMovement = !isTestingMovement;
            
            if (isTestingMovement)
            {
                if (testPlayer != null)
                {
                    originalPlayerPos = testPlayer.transform.position;
                    Debug.Log("开始移动测试 - 玩家将尝试移动到敌人上方");
                }
            }
            else
            {
                if (testPlayer != null)
                {
                    testPlayer.transform.position = originalPlayerPos;
                    Debug.Log("移动测试结束 - 玩家位置已重置");
                }
            }
        }
        
        private void PerformMovementTest()
        {
            // 让玩家尝试移动到敌人上方
            Vector3 enemyPos = testEnemy.transform.position;
            Vector3 targetPos = new Vector3(enemyPos.x, enemyPos.y + 1f, enemyPos.z);
            
            testPlayer.transform.position = Vector3.MoveTowards(
                testPlayer.transform.position, 
                targetPos, 
                testMoveSpeed * Time.deltaTime
            );
            
            // 检查是否被限制
            float actualDistance = Vector3.Distance(testPlayer.transform.position, targetPos);
            if (actualDistance > 0.1f && enableDebugLog)
            {
                Debug.Log($"移动被限制 - 距离目标还有: {actualDistance:F2}");
            }
        }
        
        private void LogSystemStatus()
        {
            Debug.Log("=== 碰撞系统状态 ===");
            
            // 检查CollisionManager
            CollisionManager manager = CollisionManager.Instance;
            if (manager != null)
            {
                Debug.Log("✓ CollisionManager 存在");
                Debug.Log($"  - 最小距离: {manager.minPlayerEnemyDistance}");
                Debug.Log($"  - 层级缩放: {manager.sortingOrderScale}");
            }
            else
            {
                Debug.LogWarning("✗ CollisionManager 不存在");
            }
            
            // 检查玩家对象
            if (testPlayer != null)
            {
                CollisionController playerCollision = testPlayer.GetComponent<CollisionController>();
                if (playerCollision != null)
                {
                    Debug.Log($"✓ 玩家碰撞控制器存在 - 层级: {playerCollision.GetSortingOrder()}");
                }
                else
                {
                    Debug.LogWarning("✗ 玩家缺少碰撞控制器");
                }
            }
            
            // 检查敌人对象
            if (testEnemy != null)
            {
                CollisionController enemyCollision = testEnemy.GetComponent<CollisionController>();
                if (enemyCollision != null)
                {
                    Debug.Log($"✓ 敌人碰撞控制器存在 - 层级: {enemyCollision.GetSortingOrder()}");
                }
                else
                {
                    Debug.LogWarning("✗ 敌人缺少碰撞控制器");
                }
            }
            
            // 检查层级排序
            CheckSortingOrders();
            
            Debug.Log("==================");
        }
        
        private void CheckSortingOrders()
        {
            CollisionController[] controllers = FindObjectsOfType<CollisionController>();
            Debug.Log($"场景中共有 {controllers.Length} 个碰撞控制器");
            
            foreach (var controller in controllers)
            {
                if (controller.spriteRenderer != null)
                {
                    float yPos = controller.transform.position.y;
                    int sortingOrder = controller.spriteRenderer.sortingOrder;
                    string type = controller.isPlayer ? "玩家" : controller.isEnemy ? "敌人" : "其他";
                    
                    Debug.Log($"  {type} [{controller.name}] Y:{yPos:F2} 层级:{sortingOrder}");
                }
            }
        }
        
        [ContextMenu("自动查找测试对象")]
        public void FindTestObjects()
        {
            if (testPlayer == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    testPlayer = player;
                    Debug.Log($"找到玩家对象: {player.name}");
                }
            }
            
            if (testEnemy == null)
            {
                GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
                if (enemy != null)
                {
                    testEnemy = enemy;
                    Debug.Log($"找到敌人对象: {enemy.name}");
                }
            }
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameArchitecture.Interface;
        }
        
        void OnGUI()
        {
            if (!enableDebugLog) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("碰撞系统测试控制");
            
            if (GUILayout.Button("T - 切换移动测试"))
            {
                ToggleMovementTest();
            }
            
            if (GUILayout.Button("L - 输出系统状态"))
            {
                LogSystemStatus();
            }
            
            if (GUILayout.Button("查找测试对象"))
            {
                FindTestObjects();
            }
            
            GUILayout.Label($"移动测试: {(isTestingMovement ? "进行中" : "已停止")}");
            
            if (CollisionManager.Instance != null)
            {
                GUILayout.Label("✓ CollisionManager 正常");
            }
            else
            {
                GUILayout.Label("✗ CollisionManager 缺失");
            }
            
            GUILayout.EndArea();
        }
    }
}

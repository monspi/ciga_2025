using UnityEngine;

namespace FartGame
{
    /// <summary>
    /// 自动设置组件，为游戏对象添加必要的碰撞控制组件
    /// </summary>
    public class AutoSetupCollision : MonoBehaviour
    {
        [Header("自动设置")]
        [Tooltip("是否为玩家")]
        public bool isPlayer = false;
        
        [Tooltip("是否为敌人")]
        public bool isEnemy = false;
        
        [Tooltip("是否自动添加CollisionController")]
        public bool autoAddCollisionController = true;
        
        void Awake()
        {
            if (autoAddCollisionController)
            {
                SetupCollisionController();
            }
        }
        
        private void SetupCollisionController()
        {
            // 检查是否已经有CollisionController
            CollisionController collisionController = GetComponent<CollisionController>();
            
            if (collisionController == null)
            {
                // 添加CollisionController
                collisionController = gameObject.AddComponent<CollisionController>();
            }
            
            // 配置CollisionController
            collisionController.isPlayer = isPlayer;
            collisionController.isEnemy = isEnemy;
            
            // 自动查找SpriteRenderer
            if (collisionController.spriteRenderer == null)
            {
                collisionController.spriteRenderer = GetComponent<SpriteRenderer>();
                
                if (collisionController.spriteRenderer == null)
                {
                    collisionController.spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }
            }
        }
    }
}

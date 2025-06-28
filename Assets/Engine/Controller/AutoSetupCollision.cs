using UnityEngine;

namespace FartGame
{
    /// <summary>
    /// 自动为对象添加必要的碰撞组件
    /// </summary>
    public class AutoSetupCollision : MonoBehaviour
    {
        [Header("自动设置选项")]
        [Tooltip("自动添加CollisionController")]
        public bool addCollisionController = true;
        
        [Tooltip("自动添加Collider2D")]
        public bool addCollider2D = true;
        
        [Tooltip("自动添加Rigidbody2D")]
        public bool addRigidbody2D = true;
        
        [Header("Collider设置")]
        [Tooltip("Collider类型")]
        public ColliderType colliderType = ColliderType.Box;
        
        [Tooltip("是否设置为Trigger")]
        public bool isTrigger = true;
        
        [Header("Rigidbody设置")]
        [Tooltip("Rigidbody类型")]
        public RigidbodyType2D bodyType = RigidbodyType2D.Kinematic;
        
        [Tooltip("是否冻结旋转")]
        public bool freezeRotation = true;
        
        [Header("CollisionController设置")]
        [Tooltip("是否标记为敌人")]
        public bool markAsEnemy = false;
        
        [Tooltip("是否标记为玩家")]
        public bool markAsPlayer = false;
        
        [Tooltip("手动分层优先级")]
        public int layerPriority = 0;
        
        public enum ColliderType
        {
            Box,
            Circle,
            Capsule
        }
        
        void Awake()
        {
            SetupComponents();
        }
        
        /// <summary>
        /// 设置所有组件
        /// </summary>
        public void SetupComponents()
        {
            if (addCollider2D)
            {
                SetupCollider();
            }
            
            if (addRigidbody2D)
            {
                SetupRigidbody();
            }
            
            if (addCollisionController)
            {
                SetupCollisionController();
            }
        }
        
        /// <summary>
        /// 设置Collider2D
        /// </summary>
        private void SetupCollider()
        {
            Collider2D existingCollider = GetComponent<Collider2D>();
            if (existingCollider != null) return;
            
            Collider2D collider = null;
            
            switch (colliderType)
            {
                case ColliderType.Box:
                    collider = gameObject.AddComponent<BoxCollider2D>();
                    break;
                case ColliderType.Circle:
                    collider = gameObject.AddComponent<CircleCollider2D>();
                    break;
                case ColliderType.Capsule:
                    collider = gameObject.AddComponent<CapsuleCollider2D>();
                    break;
            }
            
            if (collider != null)
            {
                collider.isTrigger = isTrigger;
                
                // 自动调整碰撞体大小基于SpriteRenderer
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
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
            }
        }
        
        /// <summary>
        /// 设置Rigidbody2D
        /// </summary>
        private void SetupRigidbody()
        {
            Rigidbody2D existingRigidbody = GetComponent<Rigidbody2D>();
            if (existingRigidbody != null) return;
            
            Rigidbody2D rigidbody = gameObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = bodyType;
            
            if (freezeRotation)
            {
                rigidbody.freezeRotation = true;
            }
            
            // 一般设置
            rigidbody.gravityScale = 0f; // 2D游戏通常不需要重力
        }
        
        /// <summary>
        /// 设置CollisionController
        /// </summary>
        private void SetupCollisionController()
        {
            CollisionController existingController = GetComponent<CollisionController>();
            if (existingController != null) return;
            
            CollisionController controller = gameObject.AddComponent<CollisionController>();
            controller.isEnemy = markAsEnemy;
            controller.isPlayer = markAsPlayer;
            controller.layerPriority = layerPriority;
            
            // 自动设置SpriteRenderer
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                controller.spriteRenderer = spriteRenderer;
            }
        }
        
        /// <summary>
        /// 手动触发设置（用于编辑器按钮）
        /// </summary>
        [ContextMenu("Setup Components")]
        public void ManualSetup()
        {
            SetupComponents();
            Debug.Log($"已为 {gameObject.name} 设置碰撞组件");
        }
    }
}

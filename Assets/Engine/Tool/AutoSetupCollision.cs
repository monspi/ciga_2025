using UnityEngine;
using System.Collections.Generic;

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
        
        [Tooltip("启用精确碰撞体（基于PNG透明度）")]
        public bool enablePreciseCollider = true;
        
        [Tooltip("透明度阈值（0-1，低于此值视为透明）")]
        [Range(0f, 1f)]
        public float alphaThreshold = 0.1f;
        
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
            Capsule,
            Polygon
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
                case ColliderType.Polygon:
                    collider = gameObject.AddComponent<PolygonCollider2D>();
                    break;
            }
            
            if (collider != null)
            {
                collider.isTrigger = isTrigger;
                
                // 自动调整碰撞体大小基于SpriteRenderer
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    if (enablePreciseCollider && colliderType == ColliderType.Polygon)
                    {
                        // 使用精确的多边形碰撞体
                        SetupPrecisePolygonCollider(spriteRenderer.sprite);
                    }
                    else
                    {
                        // 使用传统的碰撞体调整
                        AdjustColliderSize(collider, spriteRenderer.sprite);
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
        
        /// <summary>
        /// 设置精确的多边形碰撞体
        /// </summary>
        private void SetupPrecisePolygonCollider(Sprite sprite)
        {
            PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
            if (polygonCollider == null) return;
            
            try
            {
                // 使用Unity内置的物理形状
                List<Vector2[]> paths = new List<Vector2[]>();
                
                for (int i = 0; i < sprite.GetPhysicsShapeCount(); i++)
                {
                    List<Vector2> shapePoints = new List<Vector2>();
                    sprite.GetPhysicsShape(i, shapePoints);
                    
                    if (shapePoints.Count > 2)
                    {
                        paths.Add(shapePoints.ToArray());
                    }
                }
                
                if (paths.Count > 0)
                {
                    polygonCollider.pathCount = paths.Count;
                    for (int i = 0; i < paths.Count; i++)
                    {
                        polygonCollider.SetPath(i, paths[i]);
                    }
                    
                    Debug.Log($"为 {gameObject.name} 生成了 {paths.Count} 个多边形路径");
                }
                else
                {
                    Debug.LogWarning($"无法为 {gameObject.name} 生成多边形路径，使用默认形状");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"设置精确多边形碰撞体时出错: {e.Message}");
            }
        }
        
        /// <summary>
        /// 调整碰撞体大小
        /// </summary>
        private void AdjustColliderSize(Collider2D collider, Sprite sprite)
        {
            if (enablePreciseCollider)
            {
                // 基于非透明区域计算精确边界
                Rect preciseBounds = GetPreciseBounds(sprite);
                
                if (collider is BoxCollider2D boxCollider)
                {
                    boxCollider.size = preciseBounds.size;
                    boxCollider.offset = preciseBounds.center;
                }
                else if (collider is CircleCollider2D circleCollider)
                {
                    circleCollider.radius = Mathf.Max(preciseBounds.size.x, preciseBounds.size.y) * 0.5f;
                    circleCollider.offset = preciseBounds.center;
                }
                else if (collider is CapsuleCollider2D capsuleCollider)
                {
                    capsuleCollider.size = preciseBounds.size;
                    capsuleCollider.offset = preciseBounds.center;
                }
            }
            else
            {
                // 使用传统的sprite边界
                Bounds spriteBounds = sprite.bounds;
                
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
        
        /// <summary>
        /// 获取精确的边界矩形（排除透明像素）
        /// </summary>
        private Rect GetPreciseBounds(Sprite sprite)
        {
            if (!sprite.texture.isReadable)
            {
                Debug.LogWarning($"纹理 {sprite.texture.name} 不可读，使用默认边界");
                return new Rect(-sprite.bounds.size.x * 0.5f, -sprite.bounds.size.y * 0.5f, 
                              sprite.bounds.size.x, sprite.bounds.size.y);
            }
            
            Color32[] pixels = sprite.texture.GetPixels32();
            int textureWidth = sprite.texture.width;
            int textureHeight = sprite.texture.height;
            
            // 获取sprite在纹理中的区域
            Rect textureRect = sprite.textureRect;
            int startX = Mathf.FloorToInt(textureRect.x);
            int startY = Mathf.FloorToInt(textureRect.y);
            int endX = Mathf.CeilToInt(textureRect.x + textureRect.width);
            int endY = Mathf.CeilToInt(textureRect.y + textureRect.height);
            
            int minX = endX, maxX = startX;
            int minY = endY, maxY = startY;
            bool foundOpaque = false;
            
            // 扫描非透明像素
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    if (x >= 0 && x < textureWidth && y >= 0 && y < textureHeight)
                    {
                        Color32 pixel = pixels[y * textureWidth + x];
                        if (pixel.a > alphaThreshold * 255)
                        {
                            minX = Mathf.Min(minX, x);
                            maxX = Mathf.Max(maxX, x);
                            minY = Mathf.Min(minY, y);
                            maxY = Mathf.Max(maxY, y);
                            foundOpaque = true;
                        }
                    }
                }
            }
            
            if (!foundOpaque)
            {
                // 没有找到非透明像素，使用原始边界
                return new Rect(-sprite.bounds.size.x * 0.5f, -sprite.bounds.size.y * 0.5f, 
                              sprite.bounds.size.x, sprite.bounds.size.y);
            }
            
            // 转换为Unity坐标系
            float pixelsPerUnit = sprite.pixelsPerUnit;
            Vector2 pivot = sprite.pivot;
            
            float left = (minX - pivot.x) / pixelsPerUnit;
            float right = (maxX - pivot.x) / pixelsPerUnit;
            float bottom = (minY - pivot.y) / pixelsPerUnit;
            float top = (maxY - pivot.y) / pixelsPerUnit;
            
            float centerX = (left + right) * 0.5f;
            float centerY = (bottom + top) * 0.5f;
            float width = right - left;
            float height = top - bottom;
            
            return new Rect(centerX - width * 0.5f, centerY - height * 0.5f, width, height);
        }
    }
}

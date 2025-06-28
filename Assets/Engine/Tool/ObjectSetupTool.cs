using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FartGame
{
    /// <summary>
    /// 通用对象设置工具 - 智能识别和配置Player/Background/Enemy对象
    /// </summary>
    public class ObjectSetupTool : MonoBehaviour
    {
        [Header("批量设置选项")]
        [Tooltip("要处理的对象列表（留空则自动查找有SpriteRenderer的对象）")]
        public List<GameObject> targetObjects = new List<GameObject>();
        
        [Header("配置资源")]
        [Tooltip("敌人配置数据")]
        public EnemyConfigSO enemyConfig;
        
        [Header("对象类型识别")]
        [Tooltip("Player对象关键词")]
        public List<string> playerKeywords = new List<string> { "player", "主角", "角色", "hero", "character" };
        
        [Tooltip("Background对象关键词")]
        public List<string> backgroundKeywords = new List<string> { "background", "bg", "地图", "scene", "wall", "obstacle", "建筑", "terrain" };
        
        [Tooltip("Enemy对象关键词")]
        public List<string> enemyKeywords = new List<string> { "enemy", "敌人", "monster", "boss", "npc", "mob" };
        
        [Header("强制类型设定")]
        [Tooltip("强制设置为特定类型（覆盖自动识别）")]
        public ObjectType forceObjectType = ObjectType.Auto;
        
        [Header("通用组件设置")]
        [Tooltip("自动添加Collider2D")]
        public bool addCollider = true;
        
        [Tooltip("启用精确碰撞体（基于PNG透明度）")]
        public bool enablePreciseCollider = true;
        
        [Tooltip("透明度阈值（0-1，低于此值视为透明）")]
        [Range(0f, 1f)]
        public float alphaThreshold = 0.1f;
        
        [Header("Player专用设置")]
        [Tooltip("Player碰撞体类型")]
        public ColliderType playerColliderType = ColliderType.Box;
        
        [Tooltip("Player使用Trigger")]
        public bool playerIsTrigger = false;
        
        [Tooltip("添加PlayerController")]
        public bool addPlayerController = true;
        
        [Header("Background专用设置")]
        [Tooltip("Background碰撞体类型")]
        public ColliderType backgroundColliderType = ColliderType.Box;
        
        [Tooltip("Background使用Trigger")]
        public bool backgroundIsTrigger = false;
        
        [Tooltip("Background Rigidbody类型")]
        public RigidbodyType2D backgroundBodyType = RigidbodyType2D.Static;
        
        [Header("Enemy专用设置")]
        [Tooltip("Enemy碰撞体类型")]
        public ColliderType enemyColliderType = ColliderType.Box;
        
        [Tooltip("Enemy使用Trigger")]
        public bool enemyIsTrigger = true;
        
        [Tooltip("Enemy Rigidbody类型")]
        public RigidbodyType2D enemyBodyType = RigidbodyType2D.Kinematic;
        
        [Tooltip("添加EnemyController")]
        public bool addEnemyController = true;
        
        [Tooltip("添加战斗交互组件")]
        public bool addBattleInteraction = true;
        
        [Header("通用Rigidbody设置")]
        [Tooltip("冻结旋转")]
        public bool freezeRotation = true;
        
        [Header("分层和碰撞控制")]
        [Tooltip("自动添加CollisionController")]
        public bool addCollisionController = true;
        
        [Tooltip("启用手动分层")]
        public bool enableManualLayering = true;
        
        [Tooltip("分层间隔")]
        public int layerInterval = 1000;
        
        [Tooltip("基础分层优先级")]
        public int baseLayerPriority = 0;
        
        [Header("自动查找设置")]
        [Tooltip("查找范围：仅子对象")]
        public bool searchChildrenOnly = true;
        
        [Tooltip("排除已经配置过的对象")]
        public bool excludeConfiguredObjects = true;
        
        [Header("调试信息")]
        [Tooltip("显示详细日志")]
        public bool showDetailedLogs = true;
        
        public enum ObjectType
        {
            Auto,      // 自动识别
            Player,    // 玩家
            Background,// 背景/障碍物
            Enemy      // 敌人
        }
        
        public enum ColliderType
        {
            Box,
            Circle,
            Capsule,
            Polygon
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
                    ObjectType objType = IdentifyObjectType(obj);
                    if (IsObjectAlreadyConfigured(obj, objType)) continue;
                }
                
                targetObjects.Add(obj);
            }
            
            if (showDetailedLogs)
            {
                Debug.Log($"找到 {targetObjects.Count} 个候选对象");
                foreach (var obj in targetObjects)
                {
                    ObjectType type = IdentifyObjectType(obj);
                    Debug.Log($"- {obj.name}: {type}");
                }
            }
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        
        /// <summary>
        /// 识别对象类型
        /// </summary>
        private ObjectType IdentifyObjectType(GameObject obj)
        {
            if (forceObjectType != ObjectType.Auto)
            {
                return forceObjectType;
            }
            
            string objName = obj.name.ToLower();
            
            // 检查Player关键词
            foreach (string keyword in playerKeywords)
            {
                if (objName.Contains(keyword.ToLower()))
                {
                    return ObjectType.Player;
                }
            }
            
            // 检查Background关键词
            foreach (string keyword in backgroundKeywords)
            {
                if (objName.Contains(keyword.ToLower()))
                {
                    return ObjectType.Background;
                }
            }
            
            // 检查Enemy关键词
            foreach (string keyword in enemyKeywords)
            {
                if (objName.Contains(keyword.ToLower()))
                {
                    return ObjectType.Enemy;
                }
            }
            
            // 默认为背景对象
            return ObjectType.Background;
        }
        
        /// <summary>
        /// 检查对象是否已经配置过
        /// </summary>
        private bool IsObjectAlreadyConfigured(GameObject obj, ObjectType objType)
        {
            switch (objType)
            {
                case ObjectType.Player:
                    return obj.GetComponent<PlayerController>() != null;
                case ObjectType.Enemy:
                    return obj.GetComponent<EnemyController>() != null;
                case ObjectType.Background:
                default:
                    return obj.GetComponent<Collider2D>() != null && obj.GetComponent<Rigidbody2D>() != null;
            }
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
            
            int successCount = 0;
            int currentLayerPriority = baseLayerPriority;
            
            Dictionary<ObjectType, int> typeCount = new Dictionary<ObjectType, int>();
            
            for (int i = 0; i < targetObjects.Count; i++)
            {
                GameObject obj = targetObjects[i];
                if (obj == null) continue;
                
                try
                {
                    ObjectType objType = IdentifyObjectType(obj);
                    SetupObjectByType(obj, objType, currentLayerPriority);
                    successCount++;
                    
                    // 统计类型
                    if (!typeCount.ContainsKey(objType))
                        typeCount[objType] = 0;
                    typeCount[objType]++;
                    
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
            
            if (showDetailedLogs)
            {
                foreach (var kvp in typeCount)
                {
                    Debug.Log($"- {kvp.Key}: {kvp.Value} 个对象");
                }
            }
            
            // 确保CollisionManager存在
            EnsureCollisionManager();
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        
        /// <summary>
        /// 根据类型设置对象
        /// </summary>
        private void SetupObjectByType(GameObject obj, ObjectType objType, int layerPriority)
        {
            if (showDetailedLogs)
            {
                Debug.Log($"正在设置对象: {obj.name} (类型: {objType})");
            }
            
            switch (objType)
            {
                case ObjectType.Player:
                    SetupPlayerObject(obj, layerPriority);
                    break;
                case ObjectType.Background:
                    SetupBackgroundObject(obj, layerPriority);
                    break;
                case ObjectType.Enemy:
                    SetupEnemyObject(obj, layerPriority);
                    break;
            }
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(obj);
#endif
        }
        
        /// <summary>
        /// 设置玩家对象
        /// </summary>
        private void SetupPlayerObject(GameObject obj, int layerPriority)
        {
            // 设置标签
            obj.tag = "Player";
            
            // 添加Collider2D
            if (addCollider)
            {
                SetupCollider(obj, playerColliderType, playerIsTrigger);
            }
            
            // 添加Rigidbody2D (Player通常使用Dynamic或Kinematic)
            SetupRigidbody(obj, RigidbodyType2D.Dynamic);
            
            // 添加PlayerController
            if (addPlayerController)
            {
                PlayerController playerController = obj.GetComponent<PlayerController>();
                if (playerController == null)
                {
                    playerController = obj.AddComponent<PlayerController>();
                }
            }
            
            // 添加CollisionController
            if (addCollisionController)
            {
                SetupCollisionController(obj, layerPriority, true, false);
            }
        }
        
        /// <summary>
        /// 设置背景/障碍物对象
        /// </summary>
        private void SetupBackgroundObject(GameObject obj, int layerPriority)
        {
            // 设置标签
            obj.tag = "Background";
            
            // 添加Collider2D (背景通常用于阻挡，不是Trigger)
            if (addCollider)
            {
                SetupCollider(obj, backgroundColliderType, backgroundIsTrigger);
            }
            
            // 添加Rigidbody2D (背景通常是Static)
            SetupRigidbody(obj, backgroundBodyType);
            
            // 添加CollisionController (背景不需要特殊标记)
            if (addCollisionController)
            {
                SetupCollisionController(obj, layerPriority, false, false);
            }
        }
        
        /// <summary>
        /// 设置敌人对象
        /// </summary>
        private void SetupEnemyObject(GameObject obj, int layerPriority)
        {
            // 设置标签
            obj.tag = "Enemy";
            
            // 添加Collider2D
            if (addCollider)
            {
                SetupCollider(obj, enemyColliderType, enemyIsTrigger);
            }
            
            // 添加Rigidbody2D
            SetupRigidbody(obj, enemyBodyType);
            
            // 添加EnemyController
            if (addEnemyController)
            {
                SetupEnemyController(obj);
            }
            
            // 添加战斗交互组件
            if (addBattleInteraction)
            {
                SetupBattleInteraction(obj);
            }
            
            // 添加CollisionController
            if (addCollisionController)
            {
                SetupCollisionController(obj, layerPriority, false, true);
            }
        }
        
        /// <summary>
        /// 设置Collider2D
        /// </summary>
        private void SetupCollider(GameObject obj, ColliderType colliderType, bool isTrigger)
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
                case ColliderType.Polygon:
                    collider = obj.AddComponent<PolygonCollider2D>();
                    break;
            }
            
            if (collider != null)
            {
                collider.isTrigger = isTrigger;
                
                // 自动调整碰撞体大小基于SpriteRenderer
                SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    if (enablePreciseCollider && colliderType == ColliderType.Polygon)
                    {
                        SetupPrecisePolygonCollider(obj, spriteRenderer.sprite);
                    }
                    else
                    {
                        AdjustColliderSize(collider, spriteRenderer.sprite);
                    }
                }
            }
        }
        
        /// <summary>
        /// 设置Rigidbody2D
        /// </summary>
        private void SetupRigidbody(GameObject obj, RigidbodyType2D bodyType)
        {
            Rigidbody2D existingRigidbody = obj.GetComponent<Rigidbody2D>();
            if (existingRigidbody != null) return;
            
            Rigidbody2D rigidbody = obj.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = bodyType;
            
            if (freezeRotation)
            {
                rigidbody.freezeRotation = true;
            }
            
            // 2D游戏通常不需要重力
            rigidbody.gravityScale = 0f;
        }
        
        /// <summary>
        /// 设置EnemyController
        /// </summary>
        private void SetupEnemyController(GameObject obj)
        {
            EnemyController existingController = obj.GetComponent<EnemyController>();
            if (existingController != null) return;
            
            EnemyController controller = obj.AddComponent<EnemyController>();
            
            // 设置敌人配置
            if (enemyConfig != null)
            {
                controller.enemyConfig = enemyConfig;
            }
        }
        
        /// <summary>
        /// 设置战斗交互组件
        /// </summary>
        private void SetupBattleInteraction(GameObject obj)
        {
            // 检查是否已经有BattleInteraction组件
            BattleInteraction existingInteraction = obj.GetComponent<BattleInteraction>();
            if (existingInteraction != null) return;
            
            // 添加BattleInteraction组件
            BattleInteraction interaction = obj.AddComponent<BattleInteraction>();
            
            if (showDetailedLogs)
            {
                Debug.Log($"为 {obj.name} 添加了战斗交互组件");
            }
        }
        
        /// <summary>
        /// 设置CollisionController
        /// </summary>
        private void SetupCollisionController(GameObject obj, int layerPriority, bool isPlayer, bool isEnemy)
        {
            CollisionController existingController = obj.GetComponent<CollisionController>();
            if (existingController != null) return;
            
            CollisionController controller = obj.AddComponent<CollisionController>();
            controller.isPlayer = isPlayer;
            controller.isEnemy = isEnemy;
            controller.layerPriority = layerPriority;
            
            // 自动设置SpriteRenderer
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                controller.spriteRenderer = spriteRenderer;
            }
        }
        
        /// <summary>
        /// 设置精确的多边形碰撞体
        /// </summary>
        private void SetupPrecisePolygonCollider(GameObject obj, Sprite sprite)
        {
            PolygonCollider2D polygonCollider = obj.GetComponent<PolygonCollider2D>();
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
                    
                    if (showDetailedLogs)
                    {
                        Debug.Log($"为 {obj.name} 生成了 {paths.Count} 个多边形路径");
                    }
                }
                else
                {
                    Debug.LogWarning($"无法为 {obj.name} 生成多边形路径，使用默认形状");
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
        
        /// <summary>
        /// 确保场景中存在CollisionManager
        /// </summary>
        private void EnsureCollisionManager()
        {
            CollisionManager existingManager = FindObjectOfType<CollisionManager>();
            if (existingManager == null)
            {
                GameObject managerObj = new GameObject("CollisionManager");
                managerObj.AddComponent<CollisionManager>();
                Debug.Log("已自动创建CollisionManager");
            }
        }
        
        /// <summary>
        /// 清理所有对象的配置
        /// </summary>
        [ContextMenu("清理所有配置")]
        public void CleanupAllObjects()
        {
            if (targetObjects.Count == 0)
            {
                Debug.LogWarning("没有要清理的对象");
                return;
            }
            
            int cleanedCount = 0;
            
            foreach (var obj in targetObjects)
            {
                if (obj == null) continue;
                
                try
                {
                    // 移除相关组件
                    DestroyImmediate(obj.GetComponent<CollisionController>());
                    DestroyImmediate(obj.GetComponent<EnemyController>());
                    DestroyImmediate(obj.GetComponent<BattleInteraction>());
                    DestroyImmediate(obj.GetComponent<Collider2D>());
                    DestroyImmediate(obj.GetComponent<Rigidbody2D>());
                    
                    cleanedCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"清理对象 {obj.name} 时出错: {e.Message}");
                }
            }
            
            Debug.Log($"清理完成：成功清理 {cleanedCount}/{targetObjects.Count} 个对象");
        }
        
        /// <summary>
        /// 手动触发单个对象设置（用于Inspector按钮）
        /// </summary>
        [ContextMenu("设置选中对象")]
        public void SetupSelectedObject()
        {
#if UNITY_EDITOR
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("请先选中要设置的对象");
                return;
            }
            
            int currentLayerPriority = baseLayerPriority;
            
            foreach (var obj in selectedObjects)
            {
                ObjectType objType = IdentifyObjectType(obj);
                SetupObjectByType(obj, objType, currentLayerPriority);
                currentLayerPriority += layerInterval;
            }
            
            Debug.Log($"已为 {selectedObjects.Length} 个选中对象设置组件");
#endif
        }
    }
    
    /// <summary>
    /// 战斗交互组件 - 处理敌人的战斗交互逻辑
    /// </summary>
    public class BattleInteraction : MonoBehaviour
    {
        [Header("交互设置")]
        [Tooltip("交互键")]
        public KeyCode interactionKey = KeyCode.Space;
        
        [Tooltip("交互距离")]
        public float interactionDistance = 2f;
        
        [Tooltip("战斗场景名称")]
        public string battleSceneName = "BattleScene";
        
        [Header("UI提示")]
        [Tooltip("显示交互提示")]
        public bool showInteractionHint = true;
        
        [Tooltip("提示文本")]
        public string hintText = "按空格键进入战斗";
        
        private GameObject player;
        private bool playerInRange = false;
        private bool uiVisible = false;
        
        void Start()
        {
            // 查找玩家对象
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning($"{gameObject.name}: 未找到Player对象");
            }
        }
        
        void Update()
        {
            if (player == null) return;
            
            // 检查玩家距离
            float distance = Vector2.Distance(transform.position, player.transform.position);
            bool inRange = distance <= interactionDistance;
            
            if (inRange != playerInRange)
            {
                playerInRange = inRange;
                OnPlayerRangeChanged(inRange);
            }
            
            // 处理交互输入
            if (playerInRange && Input.GetKeyDown(interactionKey))
            {
                StartBattle();
            }
        }
        
        /// <summary>
        /// 玩家进入/离开交互范围时调用
        /// </summary>
        private void OnPlayerRangeChanged(bool inRange)
        {
            if (showInteractionHint)
            {
                if (inRange && !uiVisible)
                {
                    ShowInteractionUI();
                }
                else if (!inRange && uiVisible)
                {
                    HideInteractionUI();
                }
            }
        }
        
        /// <summary>
        /// 显示交互UI
        /// </summary>
        private void ShowInteractionUI()
        {
            uiVisible = true;
            Debug.Log($"[UI] {hintText}");
            // 这里可以调用UI系统显示交互提示
        }
        
        /// <summary>
        /// 隐藏交互UI
        /// </summary>
        private void HideInteractionUI()
        {
            uiVisible = false;
            Debug.Log("[UI] 隐藏交互提示");
            // 这里可以调用UI系统隐藏交互提示
        }
        
        /// <summary>
        /// 开始战斗
        /// </summary>
        private void StartBattle()
        {
            Debug.Log($"开始与 {gameObject.name} 的战斗！");
            
            // 保存当前场景状态
            SaveCurrentGameState();
            
            // 加载战斗场景
            LoadBattleScene();
        }
        
        /// <summary>
        /// 保存当前游戏状态
        /// </summary>
        private void SaveCurrentGameState()
        {
            // 保存玩家位置
            if (player != null)
            {
                PlayerPrefs.SetFloat("PlayerPosX", player.transform.position.x);
                PlayerPrefs.SetFloat("PlayerPosY", player.transform.position.y);
            }
            
            // 保存敌人信息
            EnemyController enemyController = GetComponent<EnemyController>();
            if (enemyController != null && enemyController.enemyConfig != null)
            {
                PlayerPrefs.SetString("BattleEnemyName", enemyController.enemyConfig.displayName);
            }
            
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 加载战斗场景
        /// </summary>
        private void LoadBattleScene()
        {
            if (!string.IsNullOrEmpty(battleSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(battleSceneName);
            }
            else
            {
                Debug.LogError("战斗场景名称未设置！");
            }
        }
        
        /// <summary>
        /// 在Scene视图中绘制交互范围
        /// </summary>
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}

#if UNITY_EDITOR
/// <summary>
/// ObjectSetupTool的自定义编辑器界面
/// </summary>
[CustomEditor(typeof(ObjectSetupTool)]
public class ObjectSetupToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ObjectSetupTool setupTool = (ObjectSetupTool)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("快速操作", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // 查找候选对象按钮
        if (GUILayout.Button("🔍 查找候选对象", GUILayout.Height(30)))
        {
            setupTool.FindCandidateObjects();
            EditorUtility.SetDirty(setupTool);
        }
        
        // 批量设置对象按钮
        GUI.enabled = setupTool.targetObjects.Count > 0;
        if (GUILayout.Button("⚙️ 批量设置对象", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("确认操作", 
                $"即将为 {setupTool.targetObjects.Count} 个对象设置组件，确定继续吗？", 
                "确定", "取消"))
            {
                setupTool.BatchSetupObjects();
                EditorUtility.SetDirty(setupTool);
            }
        }
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        // 设置选中对象按钮
        GUI.enabled = Selection.gameObjects.Length > 0;
        if (GUILayout.Button("🎯 设置选中对象", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("确认操作", 
                $"即将为 {Selection.gameObjects.Length} 个选中对象设置组件，确定继续吗？", 
                "确定", "取消"))
            {
                setupTool.SetupSelectedObject();
                EditorUtility.SetDirty(setupTool);
            }
        }
        GUI.enabled = true;
        
        // 清理所有配置按钮
        GUI.enabled = setupTool.targetObjects.Count > 0;
        GUI.color = Color.red;
        if (GUILayout.Button("🗑️ 清理所有配置", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("警告", 
                $"即将清理 {setupTool.targetObjects.Count} 个对象的所有配置，此操作不可撤销！确定继续吗？", 
                "确定", "取消"))
            {
                setupTool.CleanupAllObjects();
                EditorUtility.SetDirty(setupTool);
            }
        }
        GUI.color = Color.white;
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
        
        // 显示统计信息
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("统计信息", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"候选对象数量: {setupTool.targetObjects.Count}");
        EditorGUILayout.LabelField($"选中对象数量: {Selection.gameObjects.Length}");
        
        if (setupTool.targetObjects.Count > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("对象类型预览:", EditorStyles.miniBoldLabel);
            
            Dictionary<ObjectSetupTool.ObjectType, int> typeCount = new Dictionary<ObjectSetupTool.ObjectType, int>();
            
            foreach (var obj in setupTool.targetObjects)
            {
                if (obj == null) continue;
                
                ObjectSetupTool.ObjectType objType = GetObjectType(setupTool, obj);
                if (!typeCount.ContainsKey(objType))
                    typeCount[objType] = 0;
                typeCount[objType]++;
            }
            
            foreach (var kvp in typeCount)
            {
                EditorGUILayout.LabelField($"  {GetTypeIcon(kvp.Key)} {kvp.Key}: {kvp.Value} 个");
            }
        }
        EditorGUILayout.EndVertical();
        
        // 快速配置区域
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("快速配置", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Player配置"))
        {
            setupTool.forceObjectType = ObjectSetupTool.ObjectType.Player;
            setupTool.addPlayerController = true;
            setupTool.playerIsTrigger = false;
            setupTool.playerColliderType = ObjectSetupTool.ColliderType.Box;
            EditorUtility.SetDirty(setupTool);
        }
        
        if (GUILayout.Button("Enemy配置"))
        {
            setupTool.forceObjectType = ObjectSetupTool.ObjectType.Enemy;
            setupTool.addEnemyController = true;
            setupTool.addBattleInteraction = true;
            setupTool.enemyIsTrigger = true;
            setupTool.enemyColliderType = ObjectSetupTool.ColliderType.Box;
            EditorUtility.SetDirty(setupTool);
        }
        
        if (GUILayout.Button("Background配置"))
        {
            setupTool.forceObjectType = ObjectSetupTool.ObjectType.Background;
            setupTool.backgroundIsTrigger = false;
            setupTool.backgroundBodyType = RigidbodyType2D.Static;
            setupTool.backgroundColliderType = ObjectSetupTool.ColliderType.Box;
            EditorUtility.SetDirty(setupTool);
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("重置为自动识别"))
        {
            setupTool.forceObjectType = ObjectSetupTool.ObjectType.Auto;
            EditorUtility.SetDirty(setupTool);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// 获取对象类型（使用反射调用私有方法）
    /// </summary>
    private ObjectSetupTool.ObjectType GetObjectType(ObjectSetupTool setupTool, GameObject obj)
    {
        var method = typeof(ObjectSetupTool).GetMethod("IdentifyObjectType", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (ObjectSetupTool.ObjectType)method.Invoke(setupTool, new object[] { obj });
    }
    
    /// <summary>
    /// 获取类型图标
    /// </summary>
    private string GetTypeIcon(ObjectSetupTool.ObjectType type)
    {
        switch (type)
        {
            case ObjectSetupTool.ObjectType.Player: return "🎮";
            case ObjectSetupTool.ObjectType.Enemy: return "👾";
            case ObjectSetupTool.ObjectType.Background: return "🏗️";
            default: return "❓";
        }
    }
}
#endif
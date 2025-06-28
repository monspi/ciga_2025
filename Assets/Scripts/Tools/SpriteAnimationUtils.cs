using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tools
{
    /// <summary>
    /// Sprite动画工具类，提供批量导入和设置功能
    /// </summary>
    public static class SpriteAnimationUtils
    {
        /// <summary>
        /// 从Resources文件夹加载Sprite序列
        /// </summary>
        public static Sprite[] LoadSpritesFromResources(string path)
        {
            return Resources.LoadAll<Sprite>(path);
        }
        
        /// <summary>
        /// 根据名称模式过滤Sprite数组
        /// </summary>
        public static Sprite[] FilterSpritesByNamePattern(Sprite[] sprites, string pattern)
        {
            if (sprites == null) return new Sprite[0];
            
            return sprites.Where(sprite => 
                sprite != null && sprite.name.Contains(pattern)
            ).ToArray();
        }
        
        /// <summary>
        /// 按名称排序Sprite数组
        /// </summary>
        public static Sprite[] SortSpritesByName(Sprite[] sprites)
        {
            if (sprites == null) return new Sprite[0];
            
            return sprites.OrderBy(sprite => sprite.name).ToArray();
        }
        
        /// <summary>
        /// 创建动画配置
        /// </summary>
        public static SpriteAnimation CreateAnimationConfig(string name, Sprite[] frames, float frameRate = 12f, bool loop = true)
        {
            SpriteAnimation animation = new SpriteAnimation();
            animation.name = name;
            animation.frames = frames;
            animation.frameRate = frameRate;
            animation.loop = loop;
            return animation;
        }
        
        /// <summary>
        /// 自动设置GameObject的动画组件
        /// </summary>
        public static void SetupGameObjectAnimation(GameObject target, Sprite[] frames, float frameRate = 12f)
        {
            if (target == null) return;
            
            // 确保有SpriteRenderer
            SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = target.AddComponent<SpriteRenderer>();
            }
            
            // 添加SimpleFrameAnimator
            SimpleFrameAnimator animator = target.GetComponent<SimpleFrameAnimator>();
            if (animator == null)
            {
                animator = target.AddComponent<SimpleFrameAnimator>();
            }
            
            // 设置动画
            animator.frames = frames;
            animator.frameRate = frameRate;
            
            if (frames != null && frames.Length > 0)
            {
                spriteRenderer.sprite = frames[0];
            }
        }
        
        /// <summary>
        /// 批量创建动画GameObject
        /// </summary>
        public static GameObject[] CreateAnimationGameObjects(string[] animationNames, Sprite[][] frameArrays, Transform parent = null)
        {
            if (animationNames == null || frameArrays == null || animationNames.Length != frameArrays.Length)
            {
                Debug.LogError("[SpriteAnimationUtils] 动画名称和帧数组长度不匹配");
                return new GameObject[0];
            }
            
            List<GameObject> createdObjects = new List<GameObject>();
            
            for (int i = 0; i < animationNames.Length; i++)
            {
                GameObject obj = new GameObject(animationNames[i]);
                if (parent != null)
                {
                    obj.transform.SetParent(parent);
                }
                
                SetupGameObjectAnimation(obj, frameArrays[i]);
                createdObjects.Add(obj);
            }
            
            return createdObjects.ToArray();
        }
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// 编辑器工具：Sprite动画设置助手
    /// </summary>
    public class SpriteAnimationSetupTool : EditorWindow
    {
        private enum SetupMode
        {
            SimpleAnimator,
            AdvancedAnimator,
            CharacterController
        }
        
        [SerializeField] private SetupMode setupMode = SetupMode.SimpleAnimator;
        [SerializeField] private GameObject targetObject;
        [SerializeField] private Sprite[] selectedSprites;
        [SerializeField] private float frameRate = 12f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private string animationName = "NewAnimation";
        
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/Sprite Animation Setup")]
        public static void ShowWindow()
        {
            GetWindow<SpriteAnimationSetupTool>("Sprite Animation Setup");
        }
        
        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.LabelField("Sprite Animation Setup Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 模式选择
            setupMode = (SetupMode)EditorGUILayout.EnumPopup("Setup Mode", setupMode);
            EditorGUILayout.Space();
            
            // 目标对象
            targetObject = EditorGUILayout.ObjectField("Target GameObject", targetObject, typeof(GameObject), true) as GameObject;
            
            // Sprite选择
            EditorGUILayout.LabelField("Animation Sprites", EditorStyles.boldLabel);
            
            // 显示选中的Sprites
            if (Selection.objects != null && Selection.objects.Length > 0)
            {
                var sprites = Selection.objects.OfType<Sprite>().ToArray();
                if (sprites.Length > 0)
                {
                    EditorGUILayout.HelpBox($"当前选中 {sprites.Length} 个Sprite", MessageType.Info);
                    
                    if (GUILayout.Button("使用选中的Sprites"))
                    {
                        selectedSprites = sprites.OrderBy(s => s.name).ToArray();
                    }
                }
            }
            
            // 显示当前设置的Sprites
            if (selectedSprites != null && selectedSprites.Length > 0)
            {
                EditorGUILayout.LabelField($"设置的Sprites ({selectedSprites.Length}个):");
                for (int i = 0; i < Mathf.Min(selectedSprites.Length, 5); i++)
                {
                    EditorGUILayout.ObjectField($"Frame {i}", selectedSprites[i], typeof(Sprite), false);
                }
                if (selectedSprites.Length > 5)
                {
                    EditorGUILayout.LabelField($"... 还有 {selectedSprites.Length - 5} 个");
                }
            }
            
            EditorGUILayout.Space();
            
            // 动画设置
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
            animationName = EditorGUILayout.TextField("Animation Name", animationName);
            frameRate = EditorGUILayout.FloatField("Frame Rate", frameRate);
            loop = EditorGUILayout.Toggle("Loop", loop);
            playOnStart = EditorGUILayout.Toggle("Play On Start", playOnStart);
            
            EditorGUILayout.Space();
            
            // 设置按钮
            EditorGUI.BeginDisabledGroup(targetObject == null || selectedSprites == null || selectedSprites.Length == 0);
            
            if (GUILayout.Button("Setup Animation", GUILayout.Height(30)))
            {
                SetupAnimation();
            }
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space();
            
            // 快速操作
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Create New Animation GameObject"))
            {
                CreateNewAnimationObject();
            }
            
            if (GUILayout.Button("从文件夹批量导入"))
            {
                BatchImportFromFolder();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void SetupAnimation()
        {
            if (targetObject == null || selectedSprites == null || selectedSprites.Length == 0)
                return;
            
            // 确保有SpriteRenderer
            SpriteRenderer spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = targetObject.AddComponent<SpriteRenderer>();
            }
            
            switch (setupMode)
            {
                case SetupMode.SimpleAnimator:
                    SetupSimpleAnimator();
                    break;
                    
                case SetupMode.AdvancedAnimator:
                    SetupAdvancedAnimator();
                    break;
                    
                case SetupMode.CharacterController:
                    SetupCharacterController();
                    break;
            }
            
            // 设置初始Sprite
            spriteRenderer.sprite = selectedSprites[0];
            
            EditorUtility.SetDirty(targetObject);
            Debug.Log($"[SpriteAnimationSetupTool] 已为 {targetObject.name} 设置动画组件");
        }
        
        private void SetupSimpleAnimator()
        {
            SimpleFrameAnimator animator = targetObject.GetComponent<SimpleFrameAnimator>();
            if (animator == null)
            {
                animator = targetObject.AddComponent<SimpleFrameAnimator>();
            }
            
            animator.frames = selectedSprites;
            animator.frameRate = frameRate;
            animator.loop = loop;
            animator.playOnStart = playOnStart;
        }
        
        private void SetupAdvancedAnimator()
        {
            SpriteAnimator animator = targetObject.GetComponent<SpriteAnimator>();
            if (animator == null)
            {
                animator = targetObject.AddComponent<SpriteAnimator>();
            }
            
            // 使用反射设置私有字段（在实际项目中应该提供公共API）
            // 这里只是示例，实际使用时应该添加公共方法来设置动画
            Debug.LogWarning("高级动画器需要通过Inspector手动设置动画数组");
        }
        
        private void SetupCharacterController()
        {
            CharacterAnimationController controller = targetObject.GetComponent<CharacterAnimationController>();
            if (controller == null)
            {
                controller = targetObject.AddComponent<CharacterAnimationController>();
            }
            
            SpriteAnimator animator = targetObject.GetComponent<SpriteAnimator>();
            if (animator == null)
            {
                animator = targetObject.AddComponent<SpriteAnimator>();
            }
            
            Debug.LogWarning("角色动画控制器需要通过Inspector手动配置状态映射");
        }
        
        private void CreateNewAnimationObject()
        {
            if (selectedSprites == null || selectedSprites.Length == 0)
            {
                EditorUtility.DisplayDialog("错误", "请先选择Sprite序列", "确定");
                return;
            }
            
            GameObject newObj = new GameObject(string.IsNullOrEmpty(animationName) ? "New Animation" : animationName);
            
            // 如果有选中的对象作为父级
            if (Selection.activeGameObject != null && Selection.activeGameObject.scene.IsValid())
            {
                newObj.transform.SetParent(Selection.activeGameObject.transform);
            }
            
            targetObject = newObj;
            SetupAnimation();
            
            Selection.activeGameObject = newObj;
        }
        
        private void BatchImportFromFolder()
        {
            string folderPath = EditorUtility.OpenFolderPanel("选择包含Sprite的文件夹", "Assets", "");
            if (string.IsNullOrEmpty(folderPath))
                return;
            
            // 转换为相对路径
            if (folderPath.StartsWith(Application.dataPath))
            {
                folderPath = "Assets" + folderPath.Substring(Application.dataPath.Length);
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "请选择项目内的文件夹", "确定");
                return;
            }
            
            // 查找文件夹中的所有Sprite
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
            List<Sprite> sprites = new List<Sprite>();
            
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }
            
            if (sprites.Count > 0)
            {
                selectedSprites = sprites.OrderBy(s => s.name).ToArray();
                Debug.Log($"[SpriteAnimationSetupTool] 从文件夹导入了 {sprites.Count} 个Sprite");
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "在选中文件夹中没有找到Sprite", "确定");
            }
        }
    }
#endif
}

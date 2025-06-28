using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tools.UI
{
    /// <summary>
    /// UI动画工具类，提供批量设置和管理功能
    /// </summary>
    public static class UIAnimationUtils
    {
        /// <summary>
        /// 为UI对象设置简单帧动画
        /// </summary>
        public static SimpleUIAnimator SetupSimpleUIAnimation(GameObject target, Sprite[] frames, float frameRate = 12f)
        {
            if (target == null) return null;
            
            // 确保有Image组件
            Image imageComponent = target.GetComponent<Image>();
            if (imageComponent == null)
            {
                imageComponent = target.AddComponent<Image>();
            }
            
            // 添加SimpleUIAnimator
            SimpleUIAnimator animator = target.GetComponent<SimpleUIAnimator>();
            if (animator == null)
            {
                animator = target.AddComponent<SimpleUIAnimator>();
            }
            
            // 设置动画
            animator.frames = frames;
            animator.frameRate = frameRate;
            
            if (frames != null && frames.Length > 0)
            {
                imageComponent.sprite = frames[0];
            }
            
            return animator;
        }
        
        /// <summary>
        /// 为UI对象设置高级动画控制器
        /// </summary>
        public static UIAnimator SetupAdvancedUIAnimation(GameObject target)
        {
            if (target == null) return null;
            
            // 确保有Image组件
            Image imageComponent = target.GetComponent<Image>();
            if (imageComponent == null)
            {
                imageComponent = target.AddComponent<Image>();
            }
            
            // 添加UIAnimator
            UIAnimator animator = target.GetComponent<UIAnimator>();
            if (animator == null)
            {
                animator = target.AddComponent<UIAnimator>();
            }
            
            return animator;
        }
        
        /// <summary>
        /// 批量为UI对象设置动画
        /// </summary>
        public static void BatchSetupUIAnimations(GameObject[] targets, Sprite[] frames, float frameRate = 12f)
        {
            if (targets == null || frames == null) return;
            
            foreach (var target in targets)
            {
                if (target != null)
                {
                    SetupSimpleUIAnimation(target, frames, frameRate);
                }
            }
        }
        
        /// <summary>
        /// 创建UI动画配置
        /// </summary>
        public static UIAnimation CreateUIAnimationConfig(string name, Sprite[] frames, float frameRate = 12f, bool loop = true)
        {
            UIAnimation animation = new UIAnimation();
            animation.name = name;
            animation.frames = frames;
            animation.frameRate = frameRate;
            animation.loop = loop;
            return animation;
        }
        
        /// <summary>
        /// 在Canvas中创建动画UI对象
        /// </summary>
        public static GameObject CreateUIAnimationObject(string name, Canvas canvas, Sprite[] frames)
        {
            if (canvas == null) return null;
            
            GameObject uiObject = new GameObject(name);
            uiObject.transform.SetParent(canvas.transform, false);
            
            // 添加RectTransform
            RectTransform rectTransform = uiObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 100);
            
            // 设置动画
            SetupSimpleUIAnimation(uiObject, frames);
            
            return uiObject;
        }
        
        /// <summary>
        /// 查找场景中所有的UI动画组件
        /// </summary>
        public static List<SimpleUIAnimator> FindAllSimpleUIAnimators()
        {
            return Object.FindObjectsOfType<SimpleUIAnimator>().ToList();
        }
        
        /// <summary>
        /// 查找场景中所有的高级UI动画组件
        /// </summary>
        public static List<UIAnimator> FindAllUIAnimators()
        {
            return Object.FindObjectsOfType<UIAnimator>().ToList();
        }
        
        /// <summary>
        /// 批量控制UI动画播放
        /// </summary>
        public static void BatchControlUIAnimations(List<SimpleUIAnimator> animators, string action)
        {
            foreach (var animator in animators)
            {
                if (animator == null) continue;
                
                switch (action.ToLower())
                {
                    case "play":
                        animator.Play();
                        break;
                    case "stop":
                        animator.Stop();
                        break;
                    case "pause":
                        animator.Pause();
                        break;
                    case "resume":
                        animator.Resume();
                        break;
                }
            }
        }
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// UI动画设置工具的编辑器窗口
    /// </summary>
    public class UIAnimationSetupTool : EditorWindow
    {
        private enum UISetupMode
        {
            SimpleUIAnimator,
            AdvancedUIAnimator,
            UIAnimationController
        }
        
        [SerializeField] private UISetupMode setupMode = UISetupMode.SimpleUIAnimator;
        [SerializeField] private GameObject targetUIObject;
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private Sprite[] selectedSprites;
        [SerializeField] private float frameRate = 12f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool preserveAspect = true;
        [SerializeField] private string animationName = "NewUIAnimation";
        
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/UI Animation Setup")]
        public static void ShowWindow()
        {
            GetWindow<UIAnimationSetupTool>("UI Animation Setup");
        }
        
        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.LabelField("UI Animation Setup Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 模式选择
            setupMode = (UISetupMode)EditorGUILayout.EnumPopup("Setup Mode", setupMode);
            EditorGUILayout.Space();
            
            // 目标设置
            EditorGUILayout.LabelField("Target Settings", EditorStyles.boldLabel);
            targetUIObject = EditorGUILayout.ObjectField("Target UI GameObject", targetUIObject, typeof(GameObject), true) as GameObject;
            targetCanvas = EditorGUILayout.ObjectField("Target Canvas (for new objects)", targetCanvas, typeof(Canvas), true) as Canvas;
            
            EditorGUILayout.Space();
            
            // Sprite选择
            EditorGUILayout.LabelField("UI Animation Sprites", EditorStyles.boldLabel);
            
            // 显示选中的Sprites
            if (Selection.objects != null && Selection.objects.Length > 0)
            {
                var sprites = Selection.objects.OfType<Sprite>().ToArray();
                if (sprites.Length > 0)
                {
                    EditorGUILayout.HelpBox($"当前选中 {sprites.Length} 个UI Sprite", MessageType.Info);
                    
                    if (GUILayout.Button("使用选中的Sprites"))
                    {
                        selectedSprites = sprites.OrderBy(s => s.name).ToArray();
                    }
                }
            }
            
            // 显示当前设置的Sprites
            if (selectedSprites != null && selectedSprites.Length > 0)
            {
                EditorGUILayout.LabelField($"设置的UI Sprites ({selectedSprites.Length}个):");
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
            
            // UI动画设置
            EditorGUILayout.LabelField("UI Animation Settings", EditorStyles.boldLabel);
            animationName = EditorGUILayout.TextField("Animation Name", animationName);
            frameRate = EditorGUILayout.FloatField("Frame Rate", frameRate);
            loop = EditorGUILayout.Toggle("Loop", loop);
            playOnStart = EditorGUILayout.Toggle("Play On Start", playOnStart);
            preserveAspect = EditorGUILayout.Toggle("Preserve Aspect", preserveAspect);
            
            EditorGUILayout.Space();
            
            // 设置按钮
            EditorGUI.BeginDisabledGroup(selectedSprites == null || selectedSprites.Length == 0);
            
            if (GUILayout.Button("Setup UI Animation", GUILayout.Height(30)))
            {
                SetupUIAnimation();
            }
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space();
            
            // 快速操作
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Create New UI Animation Object"))
            {
                CreateNewUIAnimationObject();
            }
            
            if (GUILayout.Button("Batch Setup Selected UI Objects"))
            {
                BatchSetupSelectedUIObjects();
            }
            
            if (GUILayout.Button("Find All UI Animators in Scene"))
            {
                FindAllUIAnimatorsInScene();
            }
            
            EditorGUILayout.Space();
            
            // 批量控制
            EditorGUILayout.LabelField("Batch Control (Runtime Only)", EditorStyles.boldLabel);
            
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Play All"))
                {
                    BatchControlAllUIAnimators("play");
                }
                
                if (GUILayout.Button("Stop All"))
                {
                    BatchControlAllUIAnimators("stop");
                }
                
                if (GUILayout.Button("Pause All"))
                {
                    BatchControlAllUIAnimators("pause");
                }
                
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.LabelField("批量控制仅在运行时可用");
                EditorGUI.EndDisabledGroup();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void SetupUIAnimation()
        {
            if (targetUIObject == null || selectedSprites == null || selectedSprites.Length == 0)
                return;
            
            // 确保有Image组件
            Image imageComponent = targetUIObject.GetComponent<Image>();
            if (imageComponent == null)
            {
                imageComponent = targetUIObject.AddComponent<Image>();
            }
            
            switch (setupMode)
            {
                case UISetupMode.SimpleUIAnimator:
                    SetupSimpleUIAnimator();
                    break;
                    
                case UISetupMode.AdvancedUIAnimator:
                    SetupAdvancedUIAnimator();
                    break;
                    
                case UISetupMode.UIAnimationController:
                    SetupUIAnimationController();
                    break;
            }
            
            // 设置初始Sprite和UI属性
            imageComponent.sprite = selectedSprites[0];
            imageComponent.preserveAspect = preserveAspect;
            
            EditorUtility.SetDirty(targetUIObject);
            Debug.Log($"[UIAnimationSetupTool] 已为 {targetUIObject.name} 设置UI动画组件");
        }
        
        private void SetupSimpleUIAnimator()
        {
            SimpleUIAnimator animator = targetUIObject.GetComponent<SimpleUIAnimator>();
            if (animator == null)
            {
                animator = targetUIObject.AddComponent<SimpleUIAnimator>();
            }
            
            animator.frames = selectedSprites;
            animator.frameRate = frameRate;
            animator.loop = loop;
            animator.playOnStart = playOnStart;
            animator.preserveAspect = preserveAspect;
        }
        
        private void SetupAdvancedUIAnimator()
        {
            UIAnimator animator = targetUIObject.GetComponent<UIAnimator>();
            if (animator == null)
            {
                animator = targetUIObject.AddComponent<UIAnimator>();
            }
            
            Debug.LogWarning("高级UI动画器需要通过Inspector手动设置动画数组");
        }
        
        private void SetupUIAnimationController()
        {
            UIAnimationController controller = targetUIObject.GetComponent<UIAnimationController>();
            if (controller == null)
            {
                controller = targetUIObject.AddComponent<UIAnimationController>();
            }
            
            UIAnimator animator = targetUIObject.GetComponent<UIAnimator>();
            if (animator == null)
            {
                animator = targetUIObject.AddComponent<UIAnimator>();
            }
            
            Debug.LogWarning("UI动画控制器需要通过Inspector手动配置状态映射");
        }
        
        private void CreateNewUIAnimationObject()
        {
            if (selectedSprites == null || selectedSprites.Length == 0)
            {
                EditorUtility.DisplayDialog("错误", "请先选择UI Sprite序列", "确定");
                return;
            }
            
            Canvas canvas = targetCanvas;
            if (canvas == null)
            {
                // 查找场景中的Canvas
                canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    EditorUtility.DisplayDialog("错误", "场景中没有找到Canvas", "确定");
                    return;
                }
            }
            
            GameObject newUIObj = UIAnimationUtils.CreateUIAnimationObject(
                string.IsNullOrEmpty(animationName) ? "New UI Animation" : animationName,
                canvas,
                selectedSprites
            );
            
            targetUIObject = newUIObj;
            SetupUIAnimation();
            
            Selection.activeGameObject = newUIObj;
        }
        
        private void BatchSetupSelectedUIObjects()
        {
            if (selectedSprites == null || selectedSprites.Length == 0)
            {
                EditorUtility.DisplayDialog("错误", "请先选择UI Sprite序列", "确定");
                return;
            }
            
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先选择要设置的UI对象", "确定");
                return;
            }
            
            foreach (var obj in selectedObjects)
            {
                targetUIObject = obj;
                SetupUIAnimation();
            }
            
            Debug.Log($"[UIAnimationSetupTool] 已为 {selectedObjects.Length} 个UI对象设置动画");
        }
        
        private void FindAllUIAnimatorsInScene()
        {
            var simpleAnimators = UIAnimationUtils.FindAllSimpleUIAnimators();
            var advancedAnimators = UIAnimationUtils.FindAllUIAnimators();
            
            Debug.Log($"[UIAnimationSetupTool] 场景中找到 {simpleAnimators.Count} 个SimpleUIAnimator，{advancedAnimators.Count} 个UIAnimator");
            
            // 选中所有找到的动画器
            List<GameObject> allAnimatorObjects = new List<GameObject>();
            allAnimatorObjects.AddRange(simpleAnimators.Select(a => a.gameObject));
            allAnimatorObjects.AddRange(advancedAnimators.Select(a => a.gameObject));
            
            if (allAnimatorObjects.Count > 0)
            {
                Selection.objects = allAnimatorObjects.ToArray();
            }
        }
        
        private void BatchControlAllUIAnimators(string action)
        {
            if (!Application.isPlaying) return;
            
            var simpleAnimators = UIAnimationUtils.FindAllSimpleUIAnimators();
            UIAnimationUtils.BatchControlUIAnimations(simpleAnimators, action);
            
            Debug.Log($"[UIAnimationSetupTool] 已对 {simpleAnimators.Count} 个UI动画器执行 {action} 操作");
        }
    }
#endif
}

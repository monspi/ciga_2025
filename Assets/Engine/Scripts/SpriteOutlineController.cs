using UnityEngine;

namespace FartGame
{
    /// <summary>
    /// 2D Sprite描边控制器
    /// 用于动态控制Sprite的描边效果
    /// </summary>
    public class SpriteOutlineController : MonoBehaviour
    {
        [Header("描边设置")]
        [SerializeField] private Color outlineColor = Color.black;
        [SerializeField] private float outlineSize = 2f;
        [SerializeField] private bool onlyOutline = false;
        
        [Header("动画设置")]
        [SerializeField] private bool animateOutline = false;
        [SerializeField] private float animationSpeed = 2f;
        [SerializeField] private float animationAmplitude = 0.5f;
        
        [Header("高级效果")]
        [SerializeField] private bool enableGlow = false;
        [SerializeField] private Color glowColor = new Color(1, 1, 1, 0.5f);
        [SerializeField] private float glowSize = 3f;
        [SerializeField] private float glowIntensity = 1f;
        
        [Header("Shader选择")]
        [SerializeField] private OutlineShaderType shaderType = OutlineShaderType.Basic;
        
        private SpriteRenderer spriteRenderer;
        private Material outlineMaterial;
        private Material originalMaterial;
        
        public enum OutlineShaderType
        {
            Basic,
            Advanced
        }
        
        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("SpriteOutlineController需要SpriteRenderer组件！");
                return;
            }
            
            originalMaterial = spriteRenderer.material;
            CreateOutlineMaterial();
        }
        
        void Start()
        {
            ApplyOutlineSettings();
        }
        
        private void CreateOutlineMaterial()
        {
            string shaderName = shaderType == OutlineShaderType.Basic 
                ? "Sprites/OutlineShader" 
                : "Sprites/AdvancedOutlineShader";
                
            Shader outlineShader = Shader.Find(shaderName);
            if (outlineShader != null)
            {
                outlineMaterial = new Material(outlineShader);
                spriteRenderer.material = outlineMaterial;
            }
            else
            {
                Debug.LogError($"找不到Shader: {shaderName}");
            }
        }
        
        private void ApplyOutlineSettings()
        {
            if (outlineMaterial == null) return;
            
            // 基础设置
            outlineMaterial.SetColor("_OutlineColor", outlineColor);
            outlineMaterial.SetFloat("_OutlineSize", outlineSize);
            outlineMaterial.SetFloat("_OnlyOutline", onlyOutline ? 1f : 0f);
            
            // 动画设置
            outlineMaterial.SetFloat("_AnimateOutline", animateOutline ? 1f : 0f);
            if (animateOutline)
            {
                outlineMaterial.SetFloat("_AnimSpeed", animationSpeed);
                outlineMaterial.SetFloat("_AnimAmplitude", animationAmplitude);
            }
            
            // 高级效果（仅Advanced Shader支持）
            if (shaderType == OutlineShaderType.Advanced)
            {
                outlineMaterial.SetFloat("_EnableGlow", enableGlow ? 1f : 0f);
                if (enableGlow)
                {
                    outlineMaterial.SetColor("_GlowColor", glowColor);
                    outlineMaterial.SetFloat("_GlowSize", glowSize);
                    outlineMaterial.SetFloat("_GlowIntensity", glowIntensity);
                }
            }
        }
        
        #region 公共接口
        
        /// <summary>
        /// 启用/禁用描边
        /// </summary>
        public void SetOutlineEnabled(bool enabled)
        {
            spriteRenderer.material = enabled ? outlineMaterial : originalMaterial;
        }
        
        /// <summary>
        /// 设置描边颜色
        /// </summary>
        public void SetOutlineColor(Color color)
        {
            outlineColor = color;
            if (outlineMaterial != null)
                outlineMaterial.SetColor("_OutlineColor", color);
        }
        
        /// <summary>
        /// 设置描边大小
        /// </summary>
        public void SetOutlineSize(float size)
        {
            outlineSize = size;
            if (outlineMaterial != null)
                outlineMaterial.SetFloat("_OutlineSize", size);
        }
        
        /// <summary>
        /// 切换到只显示描边模式
        /// </summary>
        public void SetOnlyOutline(bool onlyOutline)
        {
            this.onlyOutline = onlyOutline;
            if (outlineMaterial != null)
                outlineMaterial.SetFloat("_OnlyOutline", onlyOutline ? 1f : 0f);
        }
        
        /// <summary>
        /// 启用/禁用动画描边
        /// </summary>
        public void SetAnimateOutline(bool animate)
        {
            animateOutline = animate;
            if (outlineMaterial != null)
                outlineMaterial.SetFloat("_AnimateOutline", animate ? 1f : 0f);
        }
        
        /// <summary>
        /// 设置动画类型（仅Advanced Shader支持）
        /// </summary>
        public void SetAnimationType(AnimationType type)
        {
            if (outlineMaterial != null && shaderType == OutlineShaderType.Advanced)
            {
                outlineMaterial.SetFloat("_AnimType", (float)type);
            }
        }
        
        /// <summary>
        /// 启用发光效果（仅Advanced Shader支持）
        /// </summary>
        public void SetGlowEnabled(bool enabled)
        {
            enableGlow = enabled;
            if (outlineMaterial != null && shaderType == OutlineShaderType.Advanced)
            {
                outlineMaterial.SetFloat("_EnableGlow", enabled ? 1f : 0f);
            }
        }
        
        /// <summary>
        /// 闪烁效果
        /// </summary>
        public void FlashOutline(Color flashColor, float duration = 0.5f)
        {
            StartCoroutine(FlashCoroutine(flashColor, duration));
        }
        
        private System.Collections.IEnumerator FlashCoroutine(Color flashColor, float duration)
        {
            Color originalColor = outlineColor;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                
                // 使用Sin波形创建闪烁效果
                float intensity = Mathf.Sin(progress * Mathf.PI * 4f);
                Color currentColor = Color.Lerp(originalColor, flashColor, intensity);
                SetOutlineColor(currentColor);
                
                yield return null;
            }
            
            SetOutlineColor(originalColor);
        }
        
        #endregion
        
        public enum AnimationType
        {
            Pulse = 0,
            Wave = 1,
            Rainbow = 2
        }
        
        void OnValidate()
        {
            if (Application.isPlaying && outlineMaterial != null)
            {
                ApplyOutlineSettings();
            }
        }
        
        void OnDestroy()
        {
            if (outlineMaterial != null)
            {
                DestroyImmediate(outlineMaterial);
            }
        }
    }
}

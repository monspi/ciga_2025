using UnityEngine;
using QFramework;

namespace FartGame
{
    /// <summary>
    /// 游戏中描边效果的使用示例
    /// 展示如何在不同游戏场景中使用描边效果
    /// </summary>
    public class GameOutlineEffects : MonoBehaviour, IController
    {
        [Header("玩家描边设置")]
        [SerializeField] private SpriteOutlineController playerOutline;
        [SerializeField] private Color normalOutlineColor = Color.black;
        [SerializeField] private Color fumeOutlineColor = Color.green;
        [SerializeField] private Color lowFartOutlineColor = Color.red;
        
        [Header("敌人描边设置")]
        [SerializeField] private Color enemyThreatColor = Color.red;
        [SerializeField] private Color enemyNormalColor = Color.gray;
        
        [Header("道具描边设置")]
        [SerializeField] private Color itemHighlightColor = Color.yellow;
        
        private PlayerModel mPlayerModel;
        private GameModel mGameModel;
        
        void Start()
        {
            mPlayerModel = this.GetModel<PlayerModel>();
            mGameModel = this.GetModel<GameModel>();
            
            InitializePlayerOutline();
            RegisterEvents();
        }
        
        private void InitializePlayerOutline()
        {
            if (playerOutline == null)
            {
                // 如果没有指定，尝试从玩家对象获取
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerOutline = player.GetComponent<SpriteOutlineController>();
                    if (playerOutline == null)
                    {
                        playerOutline = player.AddComponent<SpriteOutlineController>();
                    }
                }
            }
        }
        
        private void RegisterEvents()
        {
            // 监听熏模式变化
            this.RegisterEvent<FumeModeChangedEvent>(OnFumeModeChanged);
            
            // 监听屁值变化
            if (mPlayerModel != null)
            {
                mPlayerModel.FartValue.Register(OnFartValueChanged);
            }
        }
        
        private void OnFumeModeChanged(FumeModeChangedEvent e)
        {
            if (playerOutline == null) return;
            
            if (e.IsActive)
            {
                // 进入熏模式 - 绿色发光描边
                playerOutline.SetOutlineColor(fumeOutlineColor);
                playerOutline.SetGlowEnabled(true);
                playerOutline.SetAnimateOutline(true);
                playerOutline.SetAnimationType(SpriteOutlineController.AnimationType.Pulse);
            }
            else
            {
                // 退出熏模式 - 恢复正常描边
                playerOutline.SetOutlineColor(normalOutlineColor);
                playerOutline.SetGlowEnabled(false);
                playerOutline.SetAnimateOutline(false);
            }
        }
        
        private void OnFartValueChanged(float fartValue)
        {
            if (playerOutline == null) return;
            
            // 根据屁值调整描边效果
            float maxFart = this.GetModel<GameConfigModel>().MaxFartValue;
            float fartPercentage = fartValue / maxFart;
            
            if (fartPercentage <= 0.2f) // 屁值低于20%
            {
                // 红色警告描边
                playerOutline.SetOutlineColor(lowFartOutlineColor);
                playerOutline.SetAnimateOutline(true);
                playerOutline.SetAnimationType(SpriteOutlineController.AnimationType.Wave);
            }
            else if (!mPlayerModel.IsFumeMode.Value)
            {
                // 正常状态描边
                playerOutline.SetOutlineColor(normalOutlineColor);
                playerOutline.SetAnimateOutline(false);
            }
        }
        
        /// <summary>
        /// 为敌人添加威胁指示描边
        /// </summary>
        public void HighlightEnemy(GameObject enemy, bool isThreat = true)
        {
            SpriteOutlineController outline = enemy.GetComponent<SpriteOutlineController>();
            if (outline == null)
            {
                outline = enemy.AddComponent<SpriteOutlineController>();
            }
            
            Color outlineColor = isThreat ? enemyThreatColor : enemyNormalColor;
            outline.SetOutlineColor(outlineColor);
            outline.SetOutlineSize(2f);
            
            if (isThreat)
            {
                outline.SetAnimateOutline(true);
                outline.SetAnimationType(SpriteOutlineController.AnimationType.Pulse);
            }
            
            outline.SetOutlineEnabled(true);
        }
        
        /// <summary>
        /// 移除敌人描边
        /// </summary>
        public void RemoveEnemyHighlight(GameObject enemy)
        {
            SpriteOutlineController outline = enemy.GetComponent<SpriteOutlineController>();
            if (outline != null)
            {
                outline.SetOutlineEnabled(false);
            }
        }
        
        /// <summary>
        /// 高亮显示可收集道具
        /// </summary>
        public void HighlightItem(GameObject item)
        {
            SpriteOutlineController outline = item.GetComponent<SpriteOutlineController>();
            if (outline == null)
            {
                outline = item.AddComponent<SpriteOutlineController>();
            }
            
            outline.SetOutlineColor(itemHighlightColor);
            outline.SetOutlineSize(3f);
            outline.SetAnimateOutline(true);
            outline.SetAnimationType(SpriteOutlineController.AnimationType.Rainbow);
            outline.SetOutlineEnabled(true);
        }
        
        /// <summary>
        /// 创建选中物体的描边效果
        /// </summary>
        public void ShowSelectionOutline(GameObject target)
        {
            SpriteOutlineController outline = target.GetComponent<SpriteOutlineController>();
            if (outline == null)
            {
                outline = target.AddComponent<SpriteOutlineController>();
            }
            
            outline.SetOnlyOutline(true);
            outline.SetOutlineColor(Color.white);
            outline.SetOutlineSize(4f);
            outline.SetAnimateOutline(true);
            outline.SetAnimationType(SpriteOutlineController.AnimationType.Pulse);
            outline.SetOutlineEnabled(true);
        }
        
        /// <summary>
        /// 创建伤害闪烁效果
        /// </summary>
        public void ShowDamageFlash(GameObject target)
        {
            SpriteOutlineController outline = target.GetComponent<SpriteOutlineController>();
            if (outline == null)
            {
                outline = target.AddComponent<SpriteOutlineController>();
            }
            
            outline.FlashOutline(Color.red, 0.3f);
        }
        
        /// <summary>
        /// 创建治疗闪烁效果
        /// </summary>
        public void ShowHealFlash(GameObject target)
        {
            SpriteOutlineController outline = target.GetComponent<SpriteOutlineController>();
            if (outline == null)
            {
                outline = target.AddComponent<SpriteOutlineController>();
            }
            
            outline.FlashOutline(Color.green, 0.5f);
        }
        
        /// <summary>
        /// 批量设置所有敌人的描边
        /// </summary>
        public void SetAllEnemiesOutline(bool enabled)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                if (enabled)
                {
                    HighlightEnemy(enemy, false);
                }
                else
                {
                    RemoveEnemyHighlight(enemy);
                }
            }
        }
        
        /// <summary>
        /// 游戏暂停时的描边效果
        /// </summary>
        public void OnGamePaused(bool isPaused)
        {
            if (playerOutline != null)
            {
                // 暂停时降低描边的动画速度
                if (isPaused)
                {
                    playerOutline.SetAnimateOutline(false);
                }
                else
                {
                    // 恢复动画（如果之前有动画）
                    if (mPlayerModel.IsFumeMode.Value)
                    {
                        playerOutline.SetAnimateOutline(true);
                    }
                }
            }
        }
        
        void OnDestroy()
        {
            if (mPlayerModel != null)
            {
                mPlayerModel.FartValue.UnRegister(OnFartValueChanged);
            }
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameApp.Interface;
        }
    }
}

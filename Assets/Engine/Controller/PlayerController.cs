using QFramework;
using UnityEngine;

namespace FartGame
{
    public class PlayerController : MonoBehaviour, IController
    {
        private PlayerModel mModel;
        private GameModel mGameModel;
        private Rigidbody2D mRigidbody;
        private Collider2D mCollider;
        private CollisionController mCollisionController;
        
        [Header("Visual References")]
        public GameObject visualObject; // 玩家的视觉表现对象
        
        void Start()
        {
            mModel = this.GetModel<PlayerModel>();
            mGameModel = this.GetModel<GameModel>();
            mRigidbody = GetComponent<Rigidbody2D>();
            mCollider = GetComponent<Collider2D>();
            mCollisionController = GetComponent<CollisionController>();
            
            // 如果没有CollisionController，自动添加并配置
            if (mCollisionController == null)
            {
                mCollisionController = gameObject.AddComponent<CollisionController>();
                mCollisionController.isPlayer = true;
                mCollisionController.spriteRenderer = GetComponent<SpriteRenderer>();
                if (mCollisionController.spriteRenderer == null && visualObject != null)
                {
                    mCollisionController.spriteRenderer = visualObject.GetComponent<SpriteRenderer>();
                }
            }
            
            // 如果没有指定visualObject，默认使用自身
            if (visualObject == null)
            {
                visualObject = gameObject;
            }
            
            // 绑定位置同步
            mModel.Position.RegisterWithInitValue(pos =>
            {
                transform.position = pos;
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
            
            // 绑定体型变化
            mModel.BodySize.RegisterWithInitValue(size =>
            {
                visualObject.transform.localScale = Vector3.one * size;
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
            
            // 绑定熏模式碰撞设置
            mModel.IsFumeMode.RegisterWithInitValue(isFume =>
            {
                if (mCollider != null)
                {
                    mCollider.isTrigger = isFume;
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        
        void Update()
        {
            // 只在游戏进行中且未暂停时处理输入
            if (mGameModel.CurrentGameState.Value != GameState.Gameplay || mGameModel.IsPaused.Value)
                return;
                
            HandleInput();
            HandleMovement();
        }
        
        private void HandleInput()
        {
            // 空格键切换熏模式
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Trigger Mode");
                this.SendCommand<ToggleFumeModeCommand>();
            }
            
            // ESC键暂停游戏
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                this.SendCommand<PauseGameCommand>();
            }
        }
        
        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            Vector3 movement = new Vector3(horizontal, vertical, 0);
            movement = movement.normalized * mModel.MoveSpeed.Value * Time.deltaTime;
            
            Vector3 targetPosition = transform.position + movement;
            
            // 使用CollisionManager检查和修正位置
            if (CollisionManager.Instance != null)
            {
                targetPosition = CollisionManager.Instance.GetCorrectedPlayerPosition(targetPosition);
            }
            
            if (mRigidbody != null)
            {
                mRigidbody.MovePosition((Vector2)targetPosition);
            }
            else
            {
                transform.position = targetPosition;
            }
            
            // 更新位置到Model
            mModel.Position.Value = transform.position;
        }
        
        public IArchitecture GetArchitecture()
        {
            return FartGameArchitecture.Interface;
        }
    }
}

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
            
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = currentPosition + movement;
            
            // 使用CollisionManager检查和修正位置
            if (CollisionManager.Instance != null)
            {
                // 先尝试完整移动
                if (CollisionManager.Instance.CanPlayerMoveTo(targetPosition))
                {
                    // 可以移动，直接使用目标位置
                    ApplyMovement(targetPosition);
                }
                else
                {
                    // 不能直接移动，尝试部分移动以获得更平滑的体验
                    Vector3 correctedPosition = CollisionManager.Instance.GetCorrectedPlayerPosition(targetPosition);
                    
                    // 如果修正后的位置与当前位置相同，尝试分轴移动
                    if (Vector3.Distance(correctedPosition, currentPosition) < 0.001f)
                    {
                        // 尝试分别处理水平和垂直移动
                        Vector3 horizontalTarget = currentPosition + new Vector3(movement.x, 0, 0);
                        Vector3 verticalTarget = currentPosition + new Vector3(0, movement.y, 0);
                        
                        Vector3 finalPosition = currentPosition;
                        
                        // 尝试水平移动
                        if (CollisionManager.Instance.CanPlayerMoveTo(horizontalTarget))
                        {
                            finalPosition.x = horizontalTarget.x;
                        }
                        
                        // 尝试垂直移动
                        Vector3 testVerticalPosition = new Vector3(finalPosition.x, verticalTarget.y, finalPosition.z);
                        if (CollisionManager.Instance.CanPlayerMoveTo(testVerticalPosition))
                        {
                            finalPosition.y = testVerticalPosition.y;
                        }
                        
                        ApplyMovement(finalPosition);
                    }
                    else
                    {
                        // 使用修正后的位置
                        ApplyMovement(correctedPosition);
                    }
                }
            }
            else
            {
                // 没有CollisionManager，直接移动
                ApplyMovement(targetPosition);
            }
        }
        
        /// <summary>
        /// 应用移动到指定位置
        /// </summary>
        private void ApplyMovement(Vector3 targetPosition)
        {
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

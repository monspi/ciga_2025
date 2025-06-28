using QFramework;
using UnityEngine;

namespace FartGame
{
    public class PlayerController : MonoBehaviour, IController
    {
        private PlayerModel mModel;
        private GameModel mGameModel;
        private Rigidbody mRigidbody;
        private Collider mCollider;
        
        [Header("Visual References")]
        public GameObject visualObject; // 玩家的视觉表现对象
        
        void Start()
        {
            mModel = this.GetModel<PlayerModel>();
            mGameModel = this.GetModel<GameModel>();
            mRigidbody = GetComponent<Rigidbody>();
            mCollider = GetComponent<Collider>();
            
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
            
            // 垂直速度是水平速度的一半
            vertical *= 0.4f;
            
            Vector3 movement = new Vector3(horizontal, 0, vertical);
            
            // Y轴顺时针旋转45度 (围绕Y轴顺时针旋转45度)
            float angle = 45f * Mathf.Deg2Rad; // 顺时针为负角度
            float cosAngle = Mathf.Cos(angle);
            float sinAngle = Mathf.Sin(angle);
            
            // 应用旋转变换矩阵
            Vector3 rotatedMovement = new Vector3(
                movement.x * cosAngle - movement.z * sinAngle,
                movement.y,
                movement.x * sinAngle + movement.z * cosAngle
            );
            
            rotatedMovement = rotatedMovement.normalized * mModel.MoveSpeed.Value * Time.deltaTime;
            
            if (mRigidbody != null)
            {
                mRigidbody.MovePosition(transform.position + rotatedMovement);
            }
            else
            {
                transform.position += rotatedMovement;
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

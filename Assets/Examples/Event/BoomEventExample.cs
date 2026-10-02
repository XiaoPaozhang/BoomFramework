using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// BoomEvent静态门户使用示例
    /// </summary>
    public class BoomEventExample : MonoBehaviour
    {
        IEventManager _eventManager;
        void Start()
        {
            _eventManager = ServiceContainer.Instance.GetService<IEventManager>();
            // 使用静态门户添加事件监听 - 非常简洁！
            _eventManager.AddListener<HappyEvent>(OnHappy);

            // 触发事件
            _eventManager.TriggerEvent(new HappyEvent { Message = "我是事件信息！" });
        }

        void OnDestroy()
        {
            // 移除事件监听
            _eventManager.RemoveListener<HappyEvent>(OnHappy);
        }

        private void OnHappy(HappyEvent happyEvent)
        {
            Debug.Log($"收到开心事件: {happyEvent.Message}");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 运行时触发事件
                _eventManager.TriggerEvent(new HappyEvent { Message = "按下空格键触发的事件！提示：按c键可查看当前监听时间数量" });
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                // 显示当前监听的事件数量
                Debug.Log($"当前监听的事件数量: {_eventManager.ListenerEventCount}");
            }
        }
    }

    public struct HappyEvent : IEventArg
    {
        public string Message;
        public HappyEvent(string message)
        {
            this.Message = message;
        }
    }
}
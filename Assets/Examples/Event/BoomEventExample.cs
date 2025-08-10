using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// BoomEvent静态门户使用示例
    /// </summary>
    public class BoomEventExample : MonoBehaviour
    {
        void Start()
        {
            // 使用静态门户添加事件监听 - 非常简洁！
            BoomEvent.AddListener<HappyEvent>(OnHappy);

            // 触发事件
            BoomEvent.TriggerEvent(new HappyEvent { Message = "使用BoomEvent静态门户触发事件！" });
        }

        void OnDestroy()
        {
            // 移除事件监听
            BoomEvent.RemoveListener<HappyEvent>(OnHappy);
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
                BoomEvent.TriggerEvent(new HappyEvent { Message = "按下空格键触发的事件！" });
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                // 显示当前监听的事件数量
                Debug.Log($"当前监听的事件数量: {BoomEvent.ListenerEventCount}");
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
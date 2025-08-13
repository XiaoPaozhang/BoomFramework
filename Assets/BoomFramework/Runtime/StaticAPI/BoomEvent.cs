using System;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 事件系统静态门户类
    /// 提供简洁的事件操作API，内部委托给ServiceLocator中的EventManager
    /// </summary>
    public static class BoomEvent
    {
        private static IEventManager _eventManager;
        /// <summary>
        /// 获取事件管理器实例
        /// </summary>
        public static void Init(IEventManager eventManager)
        {
            _eventManager = eventManager;
        }

        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="action">事件回调</param>
        public static void AddListener<T>(Action<T> action) where T : IEventArg
        {
            if (_eventManager == null)
            {
                Debug.LogError("EventManager未初始化,无法添加监听");
                return;
            }

            _eventManager.AddListener(action);
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="action">事件回调</param>
        public static void RemoveListener<T>(Action<T> action) where T : IEventArg
        {
            if (_eventManager == null)
            {
                Debug.LogError("EventManager未初始化,无法移除监听");
                return;
            }
            _eventManager.RemoveListener(action);
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="eventArg">事件参数</param>
        public static void TriggerEvent<T>(T eventArg) where T : IEventArg
        {
            if (_eventManager == null)
            {
                Debug.LogError("EventManager未初始化,无法触发事件");
                return;
            }
            _eventManager.TriggerEvent(eventArg);
        }

        /// <summary>
        /// 清空所有事件监听
        /// </summary>
        public static void Clear()
        {
            if (_eventManager == null)
            {
                Debug.LogError("EventManager未初始化,无法清空事件");
                return;
            }
            _eventManager.Clear();
        }

        /// <summary>
        /// 获取当前监听的事件数量
        /// </summary>
        public static int ListenerEventCount => _eventManager?.ListenerEventCount ?? 0;
    }
}
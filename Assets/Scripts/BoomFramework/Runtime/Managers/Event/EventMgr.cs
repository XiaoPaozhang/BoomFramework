using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    public class EventMgr : IEventMgr
    {
        /// <summary>
        /// 监听事件字典
        /// </summary>
        private Dictionary<Type, List<object>> _eventListenerDict = new();
        public int ListenerEventCount => _eventListenerDict.Count;
        public bool IsInit { get; private set; } = false;

        public void Init()
        {
            IsInit = true;
        }

        public void AddListener<T>(Action<T> action) where T : IEventArg
        {
            if (!IsInit)
                Debug.LogWarning("EventManager 未初始化, 无法添加监听");

            if (!_eventListenerDict.TryGetValue(typeof(T), out var actions))
            {
                actions = new List<object>();
                _eventListenerDict.Add(typeof(T), actions);
            }

            actions.Add(action);
        }

        public void RemoveListener<T>(Action<T> action) where T : IEventArg
        {
            if (!IsInit)
                Debug.LogWarning("EventManager 未初始化, 无法添加监听");

            if (!_eventListenerDict.TryGetValue(typeof(T), out var actions))
            {
                Debug.LogWarning("EventManager 未监听该事件, 无法移除监听");
            }
            else
            {
                actions.Remove(action);
            }
        }

        public void TriggerEvent<T>(T eventArg) where T : IEventArg
        {
            if (!IsInit)
                Debug.LogWarning("EventManager 未初始化, 无法添加监听");

            if (!_eventListenerDict.TryGetValue(typeof(T), out var actions))
            {
                Debug.LogWarning("EventManager 未监听该事件, 无法触发");
            }
            else
            {
                foreach (var action in actions)
                {
                    if (action is Action<T> typedAction)
                    {
                        typedAction.Invoke(eventArg);
                    }
                }
            }
        }

        public void Clear()
        {
            if (!IsInit)
                Debug.LogWarning("EventManager 未初始化, 无法清空");

            if (ListenerEventCount > 0)
                _eventListenerDict.Clear();
        }

        public void UnInit()
        {
            Clear();
            IsInit = false;
        }
    }
}

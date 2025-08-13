using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BoomFramework
{
    public class EventManager : IEventManager
    {
        /// <summary>
        /// 监听事件字典
        /// </summary>
        private Dictionary<Type, HashSet<Delegate>> _eventListenerDict = new();
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
                actions = new();
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
                // 若该事件类型已无任何监听者，从字典中移除键
                if (actions.Count == 0)
                {
                    _eventListenerDict.Remove(typeof(T));
                }
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
                // 使用快照，避免在回调中增删监听导致的枚举异常
                var snapshot = actions.ToArray();
                foreach (var action in snapshot)
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

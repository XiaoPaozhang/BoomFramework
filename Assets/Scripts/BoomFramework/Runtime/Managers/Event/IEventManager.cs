using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 事件管理器接口
    /// </summary>
    public interface IEventManager
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInit { get; }

        /// <summary>
        /// 初始化事件管理器
        /// </summary>
        void Init();

        /// <summary>
        /// 监听的事件数量
        /// </summary>
        int ListenerEventCount { get; }
        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件回调</param>
        void AddListener<T>(Action<T> action) where T : IEventArg;
        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件回调</param>
        void RemoveListener<T>(Action<T> action) where T : IEventArg;
        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="args">事件参数</param>
        void TriggerEvent<T>(T eventArg) where T : IEventArg;
        /// <summary>
        /// 清空所有事件
        /// </summary>
        void Clear();

        /// <summary>
        /// 反初始化/清理
        /// </summary>
        void UnInit();
    }
}

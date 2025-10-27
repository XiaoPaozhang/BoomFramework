
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 管理器所对应的mono节点 - 用于为管理器提供Mono相关功能
    /// </summary>
    public abstract class ManagerMonoBase : MonoBehaviour
    {
        public bool IsInited { get; private set; }
        public void Init()
        {
            OnInit();
            IsInited = true;
        }
        public void UnInit()
        {
            OnUnInit();
            IsInited = false;
        }
        protected virtual void OnInit()
        {
            Debug.Log($"[管理器初始化]: {this.GetType().Name}");
        }
        protected virtual void OnUnInit()
        {
            Debug.Log($"[管理器销毁]: {this.GetType().Name}");
        }
    }
}

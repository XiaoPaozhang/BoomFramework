
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
            IsInited = OnInit();
        }
        public void UnInit()
        {
            OnUnInit();
            IsInited = false;
        }
        protected abstract bool OnInit();
        protected abstract void OnUnInit();
    }
}

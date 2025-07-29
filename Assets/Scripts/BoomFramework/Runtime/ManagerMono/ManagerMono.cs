
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 管理器所对应的mono节点 - 用于为管理器提供Mono相关功能
    /// </summary>
    public abstract class MgrMonoBase : MonoBehaviour
    {
        [SerializeField]
        [LabelText("是否启用")]
        private bool _isEnable = true;
        public bool IsInit { get; private set; }

        public void Init()
        {
            if (!_isEnable) return;
            OnInit();
            IsInit = true;
        }
        public void UnInit()
        {
            OnUnInit();
            IsInit = false;
        }
        protected virtual void OnInit()
        {
            Debug.Log($"{this.GetType().Name} 初始化");
        }
        protected virtual void OnUnInit()
        {
            Debug.Log($"{this.GetType().Name} 销毁");
        }
    }
}

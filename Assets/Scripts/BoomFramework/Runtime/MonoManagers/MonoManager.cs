
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 需要继承mono的管理器基类
    /// </summary>
    public class MonoManager : MonoBehaviour, IManager
    {
        [SerializeField]
        [LabelText("是否启用")]
        private bool _isEnable;
        public bool IsInit { get; private set; }

        void IManager.Init()
        {
            if (!_isEnable) return;
            OnInit();
            IsInit = true;
        }

        void IManager.UnInit()
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

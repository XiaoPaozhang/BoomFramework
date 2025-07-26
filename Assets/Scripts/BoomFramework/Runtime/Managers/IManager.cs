using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 管理器生命周期接口
    /// </summary>
    public interface IManager
    {
        /// <summary>
        /// 是否初始化完成
        /// </summary>
        bool IsInit { get; }
        /// <summary>
        /// 初始化
        /// </summary>
        void Init();
        /// <summary>
        /// 销毁
        /// </summary>
        void UnInit();
    }
}

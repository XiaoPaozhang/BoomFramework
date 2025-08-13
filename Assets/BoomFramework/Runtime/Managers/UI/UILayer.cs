using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// UI层级
    /// </summary>
    public enum UILayer
    {
        /// <summary>
        /// 背景层
        /// </summary>
        Background,
        /// <summary>
        /// 页面层
        /// </summary>
        Page,
        /// <summary>
        /// 弹窗层
        /// </summary>
        Popup,
        /// <summary>
        /// 提示层
        /// </summary>
        Toast,
        /// <summary>
        /// 遮罩层
        /// </summary>
        Blocker
    }
}

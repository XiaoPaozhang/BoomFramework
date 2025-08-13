using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 项目启动器 - 通过反射获取所有实现该接口的类，并在框架预制体上显示供选择
    /// </summary>
    public interface ILauncher
    {
        /// <summary>启动项目</summary>
        void Launch();
    }
}

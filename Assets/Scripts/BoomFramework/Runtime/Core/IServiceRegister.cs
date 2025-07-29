using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 服务注册器接口
    /// </summary>
    public interface IServiceRegister
    {
        /// <summary>注册服务</summary>
        /// <param name="serviceContainer">服务容器</param>
        void RegisterServices(ServiceContainer serviceContainer);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 服务注册器基类
    /// </summary>
    public interface IServiceRegister
    {
        void RegisterServices(ServiceLocator serviceLocator);
    }

}

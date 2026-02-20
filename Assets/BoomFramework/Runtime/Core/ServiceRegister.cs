using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 服务注册器
    /// </summary>
    public class ServiceRegister : IServiceRegister
    {
        public void RegisterServices(ServiceContainer serviceContainer)
        {
            var eventManager = serviceContainer.RegisterService<IEventManager>(new EventManager());
            eventManager.Init();
        }
    }
}

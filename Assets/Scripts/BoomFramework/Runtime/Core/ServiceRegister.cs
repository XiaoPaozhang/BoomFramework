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
            serviceContainer.RegisterService<IEventMgr>(new EventMgr());

            Debug.Log($"{this.GetType().Name} 注册服务完毕");
        }
    }
}

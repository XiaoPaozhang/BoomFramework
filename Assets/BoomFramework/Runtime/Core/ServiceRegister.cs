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

            // 资源服务默认不在此注册，由 AssetProviderMono 在场景中按配置注册 IAssetService
            Debug.Log($"{this.GetType().Name} 注册服务完毕");
        }
    }
}

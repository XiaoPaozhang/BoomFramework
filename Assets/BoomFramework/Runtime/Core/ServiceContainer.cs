using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 服务定位器
    /// </summary>
    public class ServiceContainer : Singleton<ServiceContainer>
    {
        /// <summary> 服务容器 </summary>
        private Dictionary<Type, object> _servicesDict = new();

        /// <summary> 注册服务 </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="serviceInstance">服务实例</param>
        public T RegisterService<T>(T serviceInstance) where T : class
        {
            if (HasService<T>())
            {
                Debug.Log($"[服务注册]: {typeof(T).Name} 已注册,替换服务,新服务: {serviceInstance},旧服务: {_servicesDict[typeof(T)]}");
            }

            _servicesDict[typeof(T)] = serviceInstance;
            Debug.Log($"[服务注册]: {typeof(T).Name} 实例: {serviceInstance}");
            return serviceInstance;
        }

        /// <summary>
        /// 注销服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>是否注销成功</returns>
        public bool UnRegisterService<T>() where T : class
        {
            return _servicesDict.Remove(typeof(T));
        }

        /// <summary> 获取服务 </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>服务实例</returns>
        public T GetService<T>() where T : class
        {
            if (!_servicesDict.TryGetValue(typeof(T), out var service))
            {
                Debug.LogWarning($"服务 {typeof(T).Name} 获取失败,失败原因: 未注册");
            }
            return service as T;
        }

        /// <summary>
        /// 判断服务是否已注册
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>是否已注册</returns>
        public bool HasService<T>() where T : class => _servicesDict.ContainsKey(typeof(T));

    }
}

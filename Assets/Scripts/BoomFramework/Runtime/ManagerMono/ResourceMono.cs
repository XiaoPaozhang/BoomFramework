using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    public enum ResourceType
    {
        Resources,
        AssetBundle,
    }
    public class ResourceMono : MgrMonoBase
    {
        [SerializeField]
        private ResourceType _resourceType;
        private IResourceMgr _resourceMgr;
        protected override void OnInit()
        {
            base.OnInit();

            // 根据Inspector配置创建对应的资源管理器实现
            _resourceMgr = CreateResourceMgr(_resourceType);
            _resourceMgr.Init();

            // 注册到服务容器
            ServiceContainer.Instance.RegisterService<IResourceMgr>(_resourceMgr);
            Debug.Log($"已注册资源管理器: {_resourceMgr.GetType().Name}");
        }

        /// <summary>
        /// 根据资源类型创建对应的资源管理器实例
        /// </summary>
        /// <param name="resourceType">资源加载类型</param>
        /// <returns>资源管理器实例</returns>
        private IResourceMgr CreateResourceMgr(ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.Resources:
                    Debug.Log("创建 ResourceMgr 实例");
                    return new ResourceMgr();

                case ResourceType.AssetBundle:
                    Debug.Log("创建 ABMgr 实例（AssetBundle功能待完善）");
                    return new ABMgr();

                default:
                    Debug.LogWarning($"未知的资源类型: {resourceType}，使用默认的ResourceMgr");
                    return new ResourceMgr();
            }
        }

        protected override void OnUnInit()
        {
            base.OnUnInit();
        }

        /// <summary>
        /// 获取当前配置的资源类型（供外部查询）
        /// </summary>
        public ResourceType GetResourceType() => _resourceType;
    }
}

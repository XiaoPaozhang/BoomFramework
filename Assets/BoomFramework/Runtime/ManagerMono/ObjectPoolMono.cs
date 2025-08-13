using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    public class ObjectPoolMono : ManagerMonoBase
    {
        private IObjectPoolManager _objectPoolManager;
        private IAssetLoadManager _assetManager;
        protected override void OnInit()
        {
            base.OnInit();

            _objectPoolManager = new ObjectPoolManager();

            _assetManager = ServiceContainer.Instance.GetService<IAssetLoadManager>();
            if (_assetManager == null) Debug.Log("_assetManager空");

            _objectPoolManager.Init(_assetManager);

            // 注册到服务容器
            ServiceContainer.Instance.RegisterService<IObjectPoolManager>(_objectPoolManager);
        }

        protected override void OnUnInit()
        {
            base.OnUnInit();
        }
    }
}

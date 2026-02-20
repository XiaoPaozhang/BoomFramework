using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    public class ObjectPoolMono : ManagerMonoBase
    {
        private IObjectPoolManager _objectPoolManager;
        private IAssetLoadManager _assetManager;
        protected override bool OnInit()
        {
            if (!base.OnInit()) return false;

            _assetManager = ServiceContainer.Instance.GetService<IAssetLoadManager>();
            if (_assetManager == null)
            {
                Debug.LogError($"[{GetType().Name}]初始化失败：未找到 IAssetLoadManager，请检查 AssetLoadMono 初始化顺序");
                return false;
            }

            _objectPoolManager = new ObjectPoolManager();
            _objectPoolManager.Init(_assetManager);

            ServiceContainer.Instance.RegisterService<IObjectPoolManager>(_objectPoolManager);
            return true;
        }

        protected override void OnUnInit()
        {
            _objectPoolManager?.UnInit();
            ServiceContainer.Instance.UnRegisterService<IObjectPoolManager>();
            base.OnUnInit();
        }
    }
}

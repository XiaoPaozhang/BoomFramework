using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 对象池管理器接口
    /// </summary>
    public interface IObjectPoolManager
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInit { get; }
        /// <summary>
        /// 初始化
        /// </summary>
        void Init(IAssetLoadManager assetManager);
        /// <summary>
        /// 创建对象池(默认通过资源加载管理器的方式加载资源)
        /// </summary>
        void CreatePool(string poolName, Transform parent, int poolSize);
        /// <summary>
        /// 创建对象池
        /// </summary>
        void CreatePool(string poolName, GameObject prefab, Transform parent, int poolSize);
        /// <summary>
        /// 租借对象
        /// </summary>
        GameObject RentObject(string poolName);
        /// <summary>
        /// 租借对象（便捷重载，设置父级与位姿）
        /// </summary>
        GameObject RentObject(string poolName, Transform parent, Vector3? localPos = null, Quaternion? localRot = null);
        /// <summary>
        /// 归还对象
        /// </summary>
        void ReturnObject(GameObject obj);
        /// <summary>
        /// 归还对象
        /// </summary>
        void ReturnObject(string poolName, GameObject obj);
        /// <summary>
        /// 归还所有对象
        /// </summary>
        void ReturnAllObjects(string poolName);
        /// <summary>
        /// 删除对象池
        /// </summary>
        void RemovePool(string poolName);
        /// <summary>
        /// 是否存在对象池
        /// </summary>
        bool HasPool(string poolName);
        /// <summary>
        /// 清空所有对象池
        /// </summary>
        void ClearAllPool();
        /// <summary>
        /// 反初始化/销毁
        /// </summary>
        void UnInit();
    }
}

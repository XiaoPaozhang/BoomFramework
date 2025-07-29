using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BoomFramework
{
    /// <summary>
    /// 资源加载接口
    /// </summary>
    public interface IResourceMgr
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Init();

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源</returns>
        T LoadAsset<T>(string path) where T : Object;

        /// <summary>
        /// 异步加载资源 - 协程方式
        /// 使用者需要自己调用 StartCoroutine(resourceMgr.LoadAssetCoroutine(...))
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="callback">完成回调</param>
        /// <returns>协程枚举器</returns>
        IEnumerator LoadAssetCoroutine<T>(string path, Action<T> callback) where T : Object;

        /// <summary>
        /// 清理缓存
        /// </summary>
        void ClearCache();

        /// <summary>
        /// 卸载特定资源
        /// </summary>
        /// <param name="identifier">资源标识符（AB包名或Resources路径）</param>
        /// <param name="unloadAllLoadedObjects">是否卸载所有已加载的对象</param>
        void UnloadResource(string identifier, bool unloadAllLoadedObjects = false);

        /// <summary>
        /// 卸载所有资源
        /// </summary>
        /// <param name="unloadAllLoadedObjects">是否卸载所有已加载的对象</param>
        void UnloadAllResources(bool unloadAllLoadedObjects = false);

        /// <summary>
        /// 销毁清理
        /// </summary>
        void UnInit();
    }
}

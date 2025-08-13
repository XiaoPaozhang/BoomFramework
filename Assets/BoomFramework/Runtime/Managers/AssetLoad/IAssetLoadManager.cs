using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BoomFramework
{
    /// <summary>
    /// 资源管理器接口
    /// </summary>
    public interface IAssetLoadManager
    {
        /// <summary>初始化</summary>
        void Init();

        /// <summary>同步加载资产</summary>
        T LoadAsset<T>(string path) where T : Object;

        /// <summary>异步加载资产（协程）</summary>
        IEnumerator LoadAssetCoroutine<T>(string path, Action<T> callback) where T : Object;

        /// <summary>清理缓存</summary>
        void ClearCache();

        /// <summary>卸载特定资产/资源集（由实现定义 identifier 含义）</summary>
        void UnloadResource(string identifier, bool unloadAllLoadedObjects = false);

        /// <summary>卸载所有资产</summary>
        void UnloadAllResources(bool unloadAllLoadedObjects = false);

        /// <summary>反初始化/清理</summary>
        void UnInit();
    }
}




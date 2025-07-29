using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BoomFramework
{
    /// <summary>
    /// Resources文件夹资源加载实现类
    /// </summary>
    public class ResourceMgr : IResourceMgr
    {
        private readonly Dictionary<string, Object> _assetCache = new Dictionary<string, Object>();

        public void Init()
        {
            Debug.Log("ResourceMgr 初始化完成");
        }

        public T LoadAsset<T>(string path) where T : Object
        {
            // 先检查缓存
            if (_assetCache.TryGetValue(path, out Object cachedAsset))
            {
                Debug.Log($"从缓存加载资源: {path}");
                return cachedAsset as T;
            }

            // 从Resources加载
            var asset = Resources.Load<T>(path);
            if (asset == null)
            {
                Debug.LogWarning($"Resources加载失败: {path}");
            }
            else
            {
                _assetCache[path] = asset;
                Debug.Log($"Resources加载成功: {path}");
            }
            return asset;
        }

        public IEnumerator LoadAssetCoroutine<T>(string path, Action<T> callback) where T : Object
        {
            // 先检查缓存
            if (_assetCache.TryGetValue(path, out Object cachedAsset))
            {
                Debug.Log($"从缓存加载资源: {path}");
                callback?.Invoke(cachedAsset as T);
                yield break;
            }

            ResourceRequest request = Resources.LoadAsync<T>(path);
            yield return request;

            var asset = request.asset as T;
            if (asset == null)
            {
                Debug.LogWarning($"Resources协程加载失败: {path}");
            }
            else
            {
                _assetCache[path] = asset;
                Debug.Log($"Resources协程加载成功: {path}");
            }
            callback?.Invoke(asset);
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        public void ClearCache()
        {
            _assetCache.Clear();
            Debug.Log("Resources资源缓存已清理");
        }

        /// <summary>
        /// 卸载特定资源
        /// </summary>
        /// <param name="identifier">资源路径</param>
        /// <param name="unloadAllLoadedObjects">是否卸载所有已加载的对象（Resources忽略此参数）</param>
        public void UnloadResource(string identifier, bool unloadAllLoadedObjects = false)
        {
            if (_assetCache.TryGetValue(identifier, out Object cachedAsset))
            {
                Resources.UnloadAsset(cachedAsset);
                _assetCache.Remove(identifier);
                Debug.Log($"Resources资源已卸载: {identifier}");
            }
            else
            {
                Debug.LogWarning($"要卸载的资源不在缓存中: {identifier}");
            }
        }

        /// <summary>
        /// 卸载所有资源
        /// </summary>
        /// <param name="unloadAllLoadedObjects">是否卸载所有已加载的对象（Resources忽略此参数）</param>
        public void UnloadAllResources(bool unloadAllLoadedObjects = false)
        {
            // 清理缓存
            _assetCache.Clear();

            // 卸载所有未使用的Resources资源
            Resources.UnloadUnusedAssets();

            Debug.Log("所有Resources资源已卸载");
        }

        /// <summary>
        /// 销毁清理
        /// </summary>
        public void UnInit()
        {
            // 卸载所有资源
            UnloadAllResources(false);

            Debug.Log("ResourceMgr 已清理完成");
        }
    }
}
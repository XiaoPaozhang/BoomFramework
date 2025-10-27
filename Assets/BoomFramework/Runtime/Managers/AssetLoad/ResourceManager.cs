using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BoomFramework
{
    /// <summary>
    /// 使用 Unity Resources 的资产加载实现类
    /// </summary>
    public class ResourceManager : IAssetLoadManager
    {
        private readonly Dictionary<string, Object> _assetCache = new Dictionary<string, Object>();

        public void Init()
        {
            Debug.Log("ResourceManager 初始化完成");
        }

        public T LoadAsset<T>(string path) where T : Object
        {
            // 先检查缓存
            if (_assetCache.TryGetValue(path, out Object cachedAsset))
            {
                Debug.Log($"从缓存加载资源: {path}");
                return cachedAsset as T;
            }

            // 从Resources加载（支持自动添加扩展名）
            T asset = LoadFromResources<T>(path);
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
        /// 从 Resources 加载资源，支持自动添加扩展名
        /// </summary>
        private T LoadFromResources<T>(string path) where T : Object
        {
            // 如果已经有扩展名，直接加载
            if (HasExtension(path))
            {
                return Resources.Load<T>(path);
            }

            // 没有扩展名，尝试根据类型添加候选扩展名
            var candidates = GetCandidateExtensionsForType<T>();
            if (candidates.Length == 0)
            {
                // 没有候选扩展名，直接尝试加载
                return Resources.Load<T>(path);
            }

            // 尝试每个候选扩展名
            foreach (var ext in candidates)
            {
                var candidatePath = path + ext;
                var asset = Resources.Load<T>(candidatePath);
                if (asset != null)
                {
                    Debug.Log($"[ResourceManager] 使用扩展名 {ext} 加载成功: {candidatePath}");
                    return asset;
                }
            }

            // 所有候选扩展名都失败，最后尝试不带扩展名
            return Resources.Load<T>(path);
        }

        /// <summary>
        /// 检查路径是否包含扩展名
        /// </summary>
        private static bool HasExtension(string path)
        {
            var ext = Path.GetExtension(path);
            return !string.IsNullOrEmpty(ext);
        }

        /// <summary>
        /// 根据资源类型获取候选扩展名
        /// </summary>
        private static string[] GetCandidateExtensionsForType<T>() where T : Object
        {
            var t = typeof(T);
            if (t == typeof(GameObject)) return new[] { ".prefab" };
            if (t == typeof(Texture2D) || t == typeof(Sprite)) return new[] { ".png", ".jpg", ".jpeg", ".tga", ".psd" };
            if (t == typeof(Material)) return new[] { ".mat" };
            if (t == typeof(AudioClip)) return new[] { ".wav", ".mp3", ".ogg" };
            if (t == typeof(TextAsset)) return new[] { ".txt", ".json", ".bytes", ".xml" };
            if (t == typeof(Shader)) return new[] { ".shader" };
            // 兜底：不做猜测
            return Array.Empty<string>();
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

            Debug.Log("ResourceManager 已清理完成");
        }
    }
}
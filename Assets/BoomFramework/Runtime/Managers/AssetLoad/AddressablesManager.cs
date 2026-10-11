using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace BoomFramework
{
    /// <summary>
    /// Addressables 资源加载器。路径参数使用 Addressables 中配置的 Address，支持同步、协程加载及句柄缓存。
    /// </summary>
    public sealed class AddressablesManager : IAssetLoadManager
    {
        private interface ICachedAsset
        {
            string Address { get; }
            Object Asset { get; }
            void Release();
        }

        private sealed class CachedAsset<T> : ICachedAsset where T : Object
        {
            private readonly AsyncOperationHandle<T> _handle;

            public CachedAsset(string address, T asset, AsyncOperationHandle<T> handle)
            {
                Address = address;
                Asset = asset;
                _handle = handle;
            }

            public string Address { get; }
            public Object Asset { get; }

            public void Release()
            {
                if (_handle.IsValid())
                {
                    Addressables.Release(_handle);
                }
            }
        }

        private readonly Dictionary<string, ICachedAsset> _cache = new(StringComparer.Ordinal);

        public void Init()
        {
            Debug.Log("AddressablesManager 初始化完成");
        }

        /// <summary>
        /// 同步加载 Addressables 资源。当前框架 UI 注册和对象池初始化使用同步接口，
        /// 因此此处等待 Addressables 操作完成；大量或远程资源应优先使用协程接口。
        /// </summary>
        public T LoadAsset<T>(string path) where T : Object
        {
            var addresses = GetCandidateAddresses<T>(path);
            if (addresses.Count == 0)
            {
                Debug.LogError("[AddressablesManager] 加载失败：资源地址为空");
                return null;
            }

            Exception lastException = null;
            for (int i = 0; i < addresses.Count; i++)
            {
                string address = addresses[i];
                string cacheKey = GetCacheKey<T>(address);
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    return cached.Asset as T;
                }

                AsyncOperationHandle<T> handle = default;
                try
                {
                    handle = Addressables.LoadAssetAsync<T>(address);
                    T asset = handle.WaitForCompletion();
                    if (handle.Status == AsyncOperationStatus.Succeeded && asset != null)
                    {
                        _cache[cacheKey] = new CachedAsset<T>(address, asset, handle);
                        return asset;
                    }

                    lastException = handle.OperationException;
                }
                catch (Exception exception)
                {
                    lastException = exception;
                }

                ReleaseHandle(handle);
            }

            Debug.LogError($"[AddressablesManager] 加载失败: {NormalizeAddress(path)}" +
                           (lastException == null ? string.Empty : $"\n{lastException}"));
            return null;
        }

        /// <summary>异步加载 Addressables 资源，并在完成后调用回调。</summary>
        public IEnumerator LoadAssetCoroutine<T>(string path, Action<T> callback) where T : Object
        {
            var addresses = GetCandidateAddresses<T>(path);
            if (addresses.Count == 0)
            {
                Debug.LogError("[AddressablesManager] 加载失败：资源地址为空");
                callback?.Invoke(null);
                yield break;
            }

            Exception lastException = null;
            for (int i = 0; i < addresses.Count; i++)
            {
                string address = addresses[i];
                string cacheKey = GetCacheKey<T>(address);
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    callback?.Invoke(cached.Asset as T);
                    yield break;
                }

                AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    // 其他协程可能已用同一地址完成加载；缓存只保留一个句柄。
                    if (_cache.TryGetValue(cacheKey, out cached))
                    {
                        ReleaseHandle(handle);
                        callback?.Invoke(cached.Asset as T);
                    }
                    else
                    {
                        _cache[cacheKey] = new CachedAsset<T>(address, handle.Result, handle);
                        callback?.Invoke(handle.Result);
                    }
                    yield break;
                }

                lastException = handle.OperationException;
                ReleaseHandle(handle);
            }

            Debug.LogError($"[AddressablesManager] 异步加载失败: {NormalizeAddress(path)}" +
                           (lastException == null ? string.Empty : $"\n{lastException}"));
            callback?.Invoke(null);
        }

        /// <summary>释放此加载器持有的所有 Addressables 句柄。</summary>
        public void ClearCache()
        {
            foreach (var cached in _cache.Values)
            {
                cached.Release();
            }
            _cache.Clear();
        }

        /// <summary>按完整地址卸载资源；省略扩展名时会匹配该路径下带扩展名的缓存项。</summary>
        public void UnloadResource(string identifier, bool unloadAllLoadedObjects = false)
        {
            string address = NormalizeAddress(identifier);
            if (string.IsNullOrEmpty(address))
            {
                return;
            }

            var keysToRemove = new List<string>();
            foreach (var pair in _cache)
            {
                bool matches = string.Equals(pair.Value.Address, address, StringComparison.Ordinal) ||
                               (!HasExtension(address) && pair.Value.Address.StartsWith(address + ".", StringComparison.Ordinal));
                if (matches)
                {
                    pair.Value.Release();
                    keysToRemove.Add(pair.Key);
                }
            }

            for (int i = 0; i < keysToRemove.Count; i++)
            {
                _cache.Remove(keysToRemove[i]);
            }
        }

        public void UnloadAllResources(bool unloadAllLoadedObjects = false)
        {
            ClearCache();
        }

        public void UnInit()
        {
            ClearCache();
        }

        private static List<string> GetCandidateAddresses<T>(string path) where T : Object
        {
            var address = NormalizeAddress(path);
            if (string.IsNullOrEmpty(address))
            {
                return new List<string>();
            }

            if (HasExtension(address))
            {
                return new List<string> { address };
            }

            var candidates = new List<string>();
            string[] extensions = GetCandidateExtensions<T>();
            for (int i = 0; i < extensions.Length; i++)
            {
                candidates.Add(address + extensions[i]);
            }

            // 也允许不带扩展名的自定义 Address。
            candidates.Add(address);
            return candidates;
        }

        private static string[] GetCandidateExtensions<T>() where T : Object
        {
            Type type = typeof(T);
            if (type == typeof(GameObject)) return new[] { ".prefab" };
            if (type == typeof(Texture2D) || type == typeof(Sprite)) return new[] { ".png", ".jpg", ".jpeg", ".tga", ".psd" };
            if (type == typeof(Material)) return new[] { ".mat" };
            if (type == typeof(AudioClip)) return new[] { ".wav", ".mp3", ".ogg" };
            if (type == typeof(TextAsset)) return new[] { ".txt", ".json", ".bytes", ".xml" };
            if (type == typeof(Shader)) return new[] { ".shader" };
            return Array.Empty<string>();
        }

        private static string NormalizeAddress(string address)
        {
            return string.IsNullOrWhiteSpace(address) ? string.Empty : address.Trim().Replace('\\', '/');
        }

        private static bool HasExtension(string address)
        {
            return !string.IsNullOrEmpty(Path.GetExtension(address));
        }

        private static string GetCacheKey<T>(string address) where T : Object
        {
            return typeof(T).FullName + "|" + address;
        }

        private static void ReleaseHandle<T>(AsyncOperationHandle<T> handle) where T : Object
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
    }
}

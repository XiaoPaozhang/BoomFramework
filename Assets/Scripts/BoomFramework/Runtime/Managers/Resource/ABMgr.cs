using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BoomFramework
{
    /// <summary>
    /// AssetBundle资源加载实现类
    /// </summary>
    public class ABMgr : IResourceMgr
    {
        private readonly Dictionary<string, AssetBundle> _loadedAssetBundles = new Dictionary<string, AssetBundle>();
        private readonly Dictionary<string, Object> _assetCache = new Dictionary<string, Object>();
        private AssetBundleManifest _manifest;
        private string _assetBundleRootPath;

        public void Init()
        {
            // 设置AssetBundle根路径
            _assetBundleRootPath = Application.streamingAssetsPath;

            // 加载AssetBundle清单文件
            LoadManifest();

            Debug.Log($"ABMgr 初始化完成，AssetBundle路径: {_assetBundleRootPath}");
        }

        /// <summary>
        /// 加载AssetBundle清单文件
        /// </summary>
        private void LoadManifest()
        {
            try
            {
                string manifestPath = Path.Combine(_assetBundleRootPath, "AssetBundles");
                if (File.Exists(manifestPath))
                {
                    AssetBundle manifestBundle = AssetBundle.LoadFromFile(manifestPath);
                    if (manifestBundle != null)
                    {
                        _manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                        manifestBundle.Unload(false);
                        Debug.Log("AssetBundle清单加载成功");
                    }
                    else
                    {
                        Debug.LogError("AssetBundle清单文件加载失败");
                    }
                }
                else
                {
                    Debug.LogWarning($"AssetBundle清单文件不存在: {manifestPath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"加载AssetBundle清单时发生错误: {e.Message}");
            }
        }

        public T LoadAsset<T>(string path) where T : Object
        {
            try
            {
                // 先检查缓存
                if (_assetCache.TryGetValue(path, out Object cachedAsset))
                {
                    Debug.Log($"从缓存加载资源: {path}");
                    return cachedAsset as T;
                }

                // 解析路径获取AssetBundle名称和资源名称
                if (!ParseAssetPath(path, out string bundleName, out string assetName))
                {
                    Debug.LogError($"无法解析资源路径: {path}");
                    return null;
                }

                // 加载AssetBundle
                AssetBundle bundle = LoadAssetBundle(bundleName);
                if (bundle == null)
                {
                    Debug.LogError($"加载AssetBundle失败: {bundleName}");
                    return null;
                }

                // 从AssetBundle加载资源
                T asset = bundle.LoadAsset<T>(assetName);
                if (asset != null)
                {
                    _assetCache[path] = asset;
                    Debug.Log($"AssetBundle同步加载成功: {path}");
                }
                else
                {
                    Debug.LogError($"从AssetBundle加载资源失败: {assetName}");
                }

                return asset;
            }
            catch (Exception e)
            {
                Debug.LogError($"AssetBundle同步加载异常: {path}, 错误: {e.Message}");
                return null;
            }
        }

        public IEnumerator LoadAssetCoroutine<T>(string path, Action<T> callback) where T : Object
        {
            // 参数验证
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("LoadAssetCoroutine: 资源路径不能为空");
                callback?.Invoke(null);
                yield break;
            }

            if (callback == null)
            {
                Debug.LogWarning("LoadAssetCoroutine: 回调为空，加载完成后无法通知结果");
            }

            // 先检查缓存
            if (_assetCache.TryGetValue(path, out Object cachedAsset))
            {
                Debug.Log($"从缓存加载资源: {path}");
                callback?.Invoke(cachedAsset as T);
                yield break;
            }

            // 解析路径获取AssetBundle名称和资源名称
            if (!ParseAssetPath(path, out string bundleName, out string assetName))
            {
                Debug.LogError($"无法解析资源路径: {path}，正确格式: 'bundleName/assetName' 或 'bundleName|assetName'");
                callback?.Invoke(null);
                yield break;
            }

            // 协程加载AssetBundle（包含依赖）
            AssetBundle bundle = null;
            yield return LoadAssetBundleCoroutine(bundleName, (loadedBundle) =>
            {
                bundle = loadedBundle;
            });

            if (bundle == null)
            {
                Debug.LogError($"协程加载AssetBundle失败: {bundleName}");
                callback?.Invoke(null);
                yield break;
            }

            // 协程从AssetBundle加载资源
            AssetBundleRequest request = bundle.LoadAssetAsync<T>(assetName);
            if (request == null)
            {
                Debug.LogError($"创建AssetBundleRequest失败: {assetName}");
                callback?.Invoke(null);
                yield break;
            }

            yield return request;

            T asset = request.asset as T;
            if (asset != null)
            {
                _assetCache[path] = asset;
                Debug.Log($"AssetBundle协程加载成功: {path}");
            }
            else
            {
                Debug.LogError($"从AssetBundle协程加载资源失败: {assetName}，可能原因：1.资源不存在 2.类型不匹配 3.资源已损坏");
            }

            callback?.Invoke(asset);
        }

        /// <summary>
        /// 协程加载AssetBundle（包含依赖处理）
        /// </summary>
        private IEnumerator LoadAssetBundleCoroutine(string bundleName, Action<AssetBundle> callback)
        {
            // 检查是否已加载
            if (_loadedAssetBundles.TryGetValue(bundleName, out AssetBundle bundle))
            {
                callback?.Invoke(bundle);
                yield break;
            }

            // 先协程加载依赖
            yield return LoadDependenciesCoroutine(bundleName);

            // 再协程加载目标AssetBundle
            string bundlePath = Path.Combine(_assetBundleRootPath, bundleName);
            if (File.Exists(bundlePath))
            {
                AssetBundleCreateRequest createRequest = AssetBundle.LoadFromFileAsync(bundlePath);
                yield return createRequest;

                bundle = createRequest.assetBundle;
                if (bundle != null)
                {
                    _loadedAssetBundles[bundleName] = bundle;
                    Debug.Log($"AssetBundle协程加载成功: {bundleName}");
                }
                else
                {
                    Debug.LogError($"AssetBundle协程加载失败: {bundlePath}");
                }
            }
            else
            {
                Debug.LogError($"AssetBundle文件不存在: {bundlePath}");
            }

            callback?.Invoke(bundle);
        }

        /// <summary>
        /// 协程加载AssetBundle依赖
        /// </summary>
        private IEnumerator LoadDependenciesCoroutine(string bundleName)
        {
            if (_manifest == null) yield break;

            string[] dependencies = _manifest.GetAllDependencies(bundleName);
            foreach (string dependency in dependencies)
            {
                if (!_loadedAssetBundles.ContainsKey(dependency))
                {
                    yield return LoadAssetBundleCoroutine(dependency, null);
                }
            }
        }

        /// <summary>
        /// 加载AssetBundle
        /// </summary>
        private AssetBundle LoadAssetBundle(string bundleName)
        {
            // 检查是否已加载
            if (_loadedAssetBundles.TryGetValue(bundleName, out AssetBundle bundle))
            {
                return bundle;
            }

            // 加载依赖
            LoadDependencies(bundleName);

            // 加载AssetBundle文件
            string bundlePath = Path.Combine(_assetBundleRootPath, bundleName);
            if (File.Exists(bundlePath))
            {
                bundle = AssetBundle.LoadFromFile(bundlePath);
                if (bundle != null)
                {
                    _loadedAssetBundles[bundleName] = bundle;
                    Debug.Log($"AssetBundle加载成功: {bundleName}");
                }
                else
                {
                    Debug.LogError($"AssetBundle文件加载失败: {bundlePath}");
                }
            }
            else
            {
                Debug.LogError($"AssetBundle文件不存在: {bundlePath}");
            }

            return bundle;
        }

        /// <summary>
        /// 加载AssetBundle依赖
        /// </summary>
        private void LoadDependencies(string bundleName)
        {
            if (_manifest == null) return;

            string[] dependencies = _manifest.GetAllDependencies(bundleName);
            foreach (string dependency in dependencies)
            {
                if (!_loadedAssetBundles.ContainsKey(dependency))
                {
                    LoadAssetBundle(dependency);
                }
            }
        }

        /// <summary>
        /// 解析资源路径，分离出AssetBundle名称和资源名称
        /// 格式: "bundleName/assetName" 或者 "bundleName|assetName"
        /// </summary>
        private bool ParseAssetPath(string path, out string bundleName, out string assetName)
        {
            bundleName = "";
            assetName = "";

            if (string.IsNullOrEmpty(path))
                return false;

            // 支持两种分隔符: / 和 |
            char[] separators = { '/', '|' };
            string[] parts = path.Split(separators, 2);

            if (parts.Length == 2)
            {
                bundleName = parts[0];
                assetName = parts[1];
                return true;
            }
            else if (parts.Length == 1)
            {
                // 如果只有一个部分，假设bundle名称和资源名称相同
                bundleName = parts[0];
                assetName = parts[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// 卸载特定资源（AssetBundle）
        /// </summary>
        /// <param name="identifier">AssetBundle名称</param>
        /// <param name="unloadAllLoadedObjects">是否卸载所有已加载的对象</param>
        public void UnloadResource(string identifier, bool unloadAllLoadedObjects = false)
        {
            if (_loadedAssetBundles.TryGetValue(identifier, out AssetBundle bundle))
            {
                bundle.Unload(unloadAllLoadedObjects);
                _loadedAssetBundles.Remove(identifier);
                Debug.Log($"AssetBundle已卸载: {identifier}");
            }
        }

        /// <summary>
        /// 卸载AssetBundle（保留原方法名以兼容）
        /// </summary>
        public void UnloadAssetBundle(string bundleName, bool unloadAllLoadedObjects = false)
        {
            UnloadResource(bundleName, unloadAllLoadedObjects);
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        public void ClearCache()
        {
            _assetCache.Clear();
            Debug.Log("AssetBundle资源缓存已清理");
        }

        /// <summary>
        /// 卸载所有资源
        /// </summary>
        /// <param name="unloadAllLoadedObjects">是否卸载所有已加载的对象</param>
        public void UnloadAllResources(bool unloadAllLoadedObjects = false)
        {
            foreach (var kvp in _loadedAssetBundles)
            {
                kvp.Value.Unload(unloadAllLoadedObjects);
            }
            _loadedAssetBundles.Clear();
            _assetCache.Clear();
            Debug.Log("所有AssetBundle已卸载");
        }

        /// <summary>
        /// 卸载所有AssetBundle（保留原方法名以兼容）
        /// </summary>
        public void UnloadAllAssetBundles(bool unloadAllLoadedObjects = false)
        {
            UnloadAllResources(unloadAllLoadedObjects);
        }

        /// <summary>
        /// 销毁时清理所有资源
        /// </summary>
        public void UnInit()
        {
            // 卸载所有AssetBundle
            UnloadAllResources(false);

            Debug.Log("ABMgr 已清理完成");
        }
    }
}
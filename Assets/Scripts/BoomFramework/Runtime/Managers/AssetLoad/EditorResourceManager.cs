// 仅限编辑器的资源加载器，使用 AssetDatabase 实现。
// 用 UNITY_EDITOR 包裹，确保在发布版本中不会包含此实现。

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BoomFramework
{
    /// <summary>
    /// 编辑器模式资产管理器
    /// - 基于 AssetDatabase 从项目任意目录加载资源
    /// - 路径为相对配置的根目录（如：Assets/GameAssets）
    /// - 同步/异步（协程）接口与 IAssetManager 对齐
    /// </summary>
    public class EditorResourceManager : IAssetLoadManager
    {
        private readonly string _rootPath;          // 形如 "Assets/GameAssets"
        private readonly Dictionary<string, Object> _assetCache = new();

        public EditorResourceManager(string rootPath)
        {
            var p = string.IsNullOrWhiteSpace(rootPath) ? "Assets" : rootPath.Replace('\\', '/').Trim();
            if (!p.StartsWith("Assets/", StringComparison.Ordinal) && !p.Equals("Assets", StringComparison.Ordinal))
            {
                p = "Assets/" + p.TrimStart('/');
            }
            _rootPath = p;
        }

        public void Init()
        {
            Debug.Log($"EditorResourceManager 初始化完成，根目录: {_rootPath}");
        }

        public T LoadAsset<T>(string path) where T : Object
        {
            // 允许无扩展名。若不含扩展名，则按类型候选扩展名依次尝试。
            var basePath = CombinePath(path);

            if (HasExtension(basePath))
            {
                if (_assetCache.TryGetValue(basePath, out var cached))
                    return cached as T;
                var hit = AssetDatabase.LoadAssetAtPath<T>(basePath);
                if (hit != null)
                {
                    _assetCache[basePath] = hit;
                    return hit;
                }
            }
            else
            {
                var candidates = GetCandidateExtensionsForType<T>();
                for (int i = 0; i < candidates.Length; i++)
                {
                    var candidatePath = basePath + candidates[i];
                    if (_assetCache.TryGetValue(candidatePath, out var cached))
                        return cached as T;
                    var hit = AssetDatabase.LoadAssetAtPath<T>(candidatePath);
                    if (hit != null)
                    {
                        _assetCache[candidatePath] = hit;
                        return hit;
                    }
                }
            }

            Debug.LogWarning($"[EditorResourceManager] 加载失败: {basePath}");
            return null;
        }

        public IEnumerator LoadAssetCoroutine<T>(string path, Action<T> callback) where T : Object
        {
            // AssetDatabase 为同步 API，这里用一帧延迟模拟协程异步
            yield return null;
            var asset = LoadAsset<T>(path);
            callback?.Invoke(asset);
        }

        public void ClearCache()
        {
            _assetCache.Clear();
        }

        public void UnloadResource(string identifier, bool unloadAllLoadedObjects = false)
        {
            var basePath = CombinePath(identifier);
            if (_assetCache.Remove(basePath)) return;

            if (!HasExtension(basePath))
            {
                var candidates = GetCandidateExtensionsForType<Object>();
                for (int i = 0; i < candidates.Length; i++)
                {
                    if (_assetCache.Remove(basePath + candidates[i])) return;
                }
            }
        }

        public void UnloadAllResources(bool unloadAllLoadedObjects = false)
        {
            _assetCache.Clear();
            // 编辑器下可触发一次清理
            if (unloadAllLoadedObjects)
            {
                Resources.UnloadUnusedAssets();
            }
        }

        public void UnInit()
        {
            UnloadAllResources(false);
        }

        private string CombinePath(string relative)
        {
            if (string.IsNullOrWhiteSpace(relative)) return _rootPath;
            var norm = relative.Replace('\\', '/');
            if (norm.StartsWith("Assets/", StringComparison.Ordinal) || norm.Equals("Assets", StringComparison.Ordinal))
            {
                return norm;
            }
            return _rootPath.TrimEnd('/', '\\') + "/" + norm.TrimStart('/', '\\');
        }

        private static bool HasExtension(string p)
        {
            var ext = Path.GetExtension(p);
            return !string.IsNullOrEmpty(ext);
        }

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
    }
}
#endif



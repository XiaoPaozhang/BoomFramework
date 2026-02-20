using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BoomFramework
{
    public class AssetLoadMono : ManagerMonoBase
    {
        [Header("资源加载配置")]
        [Tooltip("资源加载模式")]
        [SerializeField]
        private AssetLoadModeType _providerType;

        [Header("统一路径配置")]
        [Tooltip("默认资源根路径（必须以 Assets/ 开头）\n" +
                 "Editor 模式：直接使用此路径\n" +
                 "AssetBundle 模式：去掉此路径前缀后使用")]
        [SerializeField]
        private string _defaultRootPath = "Assets/game_assets_bundle";

        [Header("UI 子路径配置")]
        [Tooltip("UI 预制体子路径（相对于根路径）")]
        [SerializeField]
        private string _uiSubPath = "ui";

        private IAssetLoadManager _assetLoadManager;

        /// <summary>
        /// 获取默认资源根路径（Editor 模式使用）
        /// </summary>
        public string DefaultRootPath => _defaultRootPath;

        /// <summary>
        /// 获取 UI 资源的路径
        /// Editor 模式：Assets/game_assets_bundle/ui
        /// AssetBundle 模式：ui
        /// </summary>
        public string UIPath
        {
            get
            {
                if (_providerType == AssetLoadModeType.Editor)
                {
                    // Editor 模式：返回完整路径
                    string resultUIPath = Path.Combine(_defaultRootPath, _uiSubPath).Replace('\\', '/');
                    // Debug.Log($"Editor_UIPath: {resultUIPath}");
                    return resultUIPath;

                }
                else if (_providerType == AssetLoadModeType.AssetBundle)
                {
                    // AssetBundle 模式：只返回子路径（不需要前缀）
                    // Debug.Log($"AB_UIPath: {_uiSubPath}");
                    return _uiSubPath;
                }
                else
                {
                    // Resources 模式（暂不支持）
                    Debug.LogWarning("Resources 模式暂不支持，请使用 Editor 或 AssetBundle 模式");
                    return _uiSubPath;
                }
            }
        }

        protected override bool OnInit()
        {
            if (!base.OnInit()) return false;

            // 根据Inspector配置创建对应的资源管理器实现
            _assetLoadManager = CreateAssetLoadManager(_providerType);
            if (_assetLoadManager == null)
            {
                Debug.LogError($"[{GetType().Name}]初始化失败：CreateAssetLoadManager 返回空");
                return false;
            }
            _assetLoadManager.Init();

            // 注册到服务容器
            ServiceContainer.Instance.RegisterService<IAssetLoadManager>(_assetLoadManager);
            return true;
        }

        /// <summary>
        /// 根据资源类型创建对应的资源管理器实例
        /// </summary>
        /// <param name="providerType">资源加载类型</param>
        /// <returns>资源管理器实例</returns>
        private IAssetLoadManager CreateAssetLoadManager(AssetLoadModeType providerType)
        {
            switch (providerType)
            {
                case AssetLoadModeType.Resources_路径不与其他模式同步固暂时不推荐用:
                    Debug.LogWarning("Resources 模式暂不支持，请使用 Editor 或 AssetBundle 模式");
                    return new ResourceManager();

                case AssetLoadModeType.AssetBundle:
                    Debug.Log($"[资源加载模式]: AssetBundle");
                    return new ABManager();

                case AssetLoadModeType.Editor:
                    {
#if UNITY_EDITOR
                        Debug.Log($"[资源加载模式]: Editor,根路径: {_defaultRootPath}");
                        return new EditorResourceManager(_defaultRootPath);
#else
                    Debug.LogWarning("当前为非编辑器环境，Editor 资源加载不可用，回退到 AssetBundle 模式");
                    return new ABManager();
#endif
                    }

                default:
                    Debug.LogWarning($"未知的资产提供类型: {providerType}，使用默认的 ResourceManager");
                    return new ResourceManager();
            }
        }

        protected override void OnUnInit()
        {
            _assetLoadManager?.UnInit();
            base.OnUnInit();
        }
    }

    public enum AssetLoadModeType
    {
        /// <summary>
        /// 编辑模式（使用 AssetDatabase）
        /// </summary>
        Editor,
        /// <summary>
        /// AB包模式（使用 AssetBundle）
        /// </summary>
        AssetBundle,
        /// <summary>
        /// 资源模式（使用 Resources）
        /// </summary>
        Resources_路径不与其他模式同步固暂时不推荐用,
    }
}

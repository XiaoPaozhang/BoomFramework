using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BoomFramework
{
    public class AssetLoadMono : ManagerMonoBase
    {
        #region Inspector 调试
        [SerializeField]
        [LabelText("资源加载模式")]
        private AssetLoadModeType _providerType;

        [SerializeField]
        [LabelText("编辑器资源根目录")]
        [FolderPath(RequireExistingPath = true)]
        [ShowIf(nameof(IsEditorType))]
        [InfoBox("示例：Assets/Examples/AssetMgr/Art", InfoMessageType.Info)]
        [OnValueChanged(nameof(NormalizeToAssetsPath))]
        private string _editorRootRelativePath = "";
        #endregion

        private IAssetLoadManager _assetService;

        private bool IsEditorType() => _providerType == AssetLoadModeType.Editor;
        protected override void OnInit()
        {
            base.OnInit();

            // 根据Inspector配置创建对应的资源管理器实现
            _assetService = CreateAssetService(_providerType);
            _assetService.Init();

            // 注册到服务容器
            ServiceContainer.Instance.RegisterService<IAssetLoadManager>(_assetService);
        }

        /// <summary>
        /// 根据资源类型创建对应的资源管理器实例
        /// </summary>
        /// <param name="resourceType">资源加载类型</param>
        /// <returns>资源管理器实例</returns>
        private IAssetLoadManager CreateAssetService(AssetLoadModeType providerType)
        {
            switch (providerType)
            {
                case AssetLoadModeType.Resources_路径不与其他模式同步固暂时不推荐用:
                    Debug.Log("创建 ResourceManager 实例");
                    return new ResourceManager();

                case AssetLoadModeType.AssetBundle:
                    Debug.Log("创建 ABManager 实例（AssetBundle功能待完善）");
                    return new ABManager();

                case AssetLoadModeType.Editor:
                    {
#if UNITY_EDITOR
                        Debug.Log($"创建 EditorResourceManager 实例, 根目录: {_editorRootRelativePath}");
                        return new EditorResourceManager(_editorRootRelativePath);
#else
                    Debug.LogWarning("当前为非编辑器环境，Editor 资源加载不可用，回退到 ResourceManager");
                    return new ResourceManager();
#endif
                    }

                default:
                    Debug.LogWarning($"未知的资产提供类型: {providerType}，使用默认的 ResourceManager");
                    return new ResourceManager();
            }
        }

        protected override void OnUnInit()
        {
            _assetService?.UnInit();
            base.OnUnInit();
        }

        // 规范化：保证 Inspector 中与运行时使用的路径都是以 "Assets/" 开头
        private void NormalizeToAssetsPath()
        {
            if (string.IsNullOrWhiteSpace(_editorRootRelativePath)) return;
            var p = _editorRootRelativePath.Replace('\\', '/').Trim();
            if (!p.StartsWith("Assets/", StringComparison.Ordinal) && !p.Equals("Assets", StringComparison.Ordinal))
            {
                p = "Assets/" + p.TrimStart('/');
            }
            _editorRootRelativePath = p;
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

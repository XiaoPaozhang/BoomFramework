using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    public class UIMono : ManagerMonoBase
    {
        private IAssetLoadManager _assetLoadManager;
        private IObjectPoolManager _objectPoolManager;
        private IUIManager _uiManager;
        private RectTransform _canvasRectTransform;
        [SerializeField]
        [Tooltip("UI预制体加载路径 (优先级低于AssetsLoadMono的UI路径配置)")]
         private string _uiLoadPath = "Assets/GameAssetsBundle/ui";

        protected override bool OnInit()
        {
            if (!base.OnInit()) return false;

            _assetLoadManager = ServiceContainer.Instance.GetService<IAssetLoadManager>();
            _objectPoolManager = ServiceContainer.Instance.GetService<IObjectPoolManager>();
            if (_assetLoadManager == null || _objectPoolManager == null)
            {
                Debug.LogError($"[{GetType().Name}]初始化失败：依赖缺失。IAssetLoadManager={_assetLoadManager != null}, IObjectPoolManager={_objectPoolManager != null}");
                return false;
            }
            _uiManager = new UIManager();

            _canvasRectTransform = transform.Find("Canvas") as RectTransform;
            if (_canvasRectTransform == null)
            {
                Debug.LogError($"[{GetType().Name}]初始化失败：未找到 Canvas 子节点");
                return false;
            }
            RectTransform poolParent = _canvasRectTransform.Find("UIPools") as RectTransform;

            // 声明UI层级节点
            Dictionary<UILayer, RectTransform> uILayersRectTransformDict = new()
            {
                { UILayer.Background, _canvasRectTransform.Find("Background") as RectTransform },
                { UILayer.Page, _canvasRectTransform.Find("Page") as RectTransform },
                { UILayer.Popup, _canvasRectTransform.Find("Popup") as RectTransform },
                { UILayer.Toast, _canvasRectTransform.Find("Toast") as RectTransform },
                { UILayer.Blocker, _canvasRectTransform.Find("Blocker") as RectTransform }
            };
            foreach (var kv in uILayersRectTransformDict)
            {
                if (kv.Value == null)
                {
                    Debug.LogError($"[{GetType().Name}]初始化失败：缺少 UI 层节点 {kv.Key}");
                    return false;
                }
            }


            UIRootContext uIRootContext = new()
            {
                UIRoot = transform as RectTransform,
                CanvasRectTransform = _canvasRectTransform,
                UICamera = GetComponent<Camera>(),
                UILayersRectTransformDict = uILayersRectTransformDict,
                PoolParent = poolParent
            };

            // 从 AssetLoadMono 获取 UI 路径
            var assetLoadMono = BoomFrameworkCore.Instance.GetManagerMono<AssetLoadMono>();
            string uiLoadPath = assetLoadMono != null ? assetLoadMono.UIPath : _uiLoadPath;

            _uiManager.Init(uiLoadPath, uIRootContext, _assetLoadManager, _objectPoolManager);

            ServiceContainer.Instance.RegisterService<IUIManager>(_uiManager);
            return true;
        }

        protected override void OnUnInit()
        {
            _uiManager?.UnInit();
            ServiceContainer.Instance.UnRegisterService<IUIManager>();
            base.OnUnInit();
        }
    }

    public struct UIRootContext
    {
        /// <summary>
        /// UI根节点
        /// </summary>
        public RectTransform UIRoot;
        /// <summary>
        /// 画布根节点
        /// </summary>
        public RectTransform CanvasRectTransform;
        /// <summary>
        /// UI相机
        /// </summary>
        public Camera UICamera;
        /// <summary>
        /// UI层级节点字典
        /// </summary>
        public Dictionary<UILayer, RectTransform> UILayersRectTransformDict;
        /// <summary>
        /// 对象池空闲对象的停放父节点
        /// </summary>
        public RectTransform PoolParent;
    }
}

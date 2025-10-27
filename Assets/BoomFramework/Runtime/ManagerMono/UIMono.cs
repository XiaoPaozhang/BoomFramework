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

        protected override void OnInit()
        {
            base.OnInit();

            _assetLoadManager = ServiceContainer.Instance.GetService<IAssetLoadManager>();
            _objectPoolManager = ServiceContainer.Instance.GetService<IObjectPoolManager>();
            _uiManager = new UIManager();

            _canvasRectTransform = transform.Find("Canvas") as RectTransform;

            // 声明UI层级节点
            Dictionary<UILayer, RectTransform> uILayersRectTransformDict = new()
            {
                { UILayer.Background, _canvasRectTransform.Find("Background") as RectTransform },
                { UILayer.Page, _canvasRectTransform.Find("Page") as RectTransform },
                { UILayer.Popup, _canvasRectTransform.Find("Popup") as RectTransform },
                { UILayer.Toast, _canvasRectTransform.Find("Toast") as RectTransform },
                { UILayer.Blocker, _canvasRectTransform.Find("Blocker") as RectTransform }
            };

            // UI池对象
            RectTransform poolParent = _canvasRectTransform.Find("UIPools") as RectTransform;

            UIRootContext uIRootContext = new(
                transform as RectTransform,
                _canvasRectTransform,
                GetComponent<Camera>(),
                uILayersRectTransformDict,
                poolParent
            );

            // 从 AssetLoadMono 获取 UI 路径
            var assetLoadMono = BoomFrameworkCore.Instance.GetManagerMono<AssetLoadMono>();
            string uiLoadPath = assetLoadMono != null ? assetLoadMono.UIPath : _uiLoadPath;

            _uiManager.Init(uiLoadPath, uIRootContext, _assetLoadManager, _objectPoolManager);

            ServiceContainer.Instance.RegisterService<IUIManager>(_uiManager);
        }

        protected override void OnUnInit()
        {
            base.OnUnInit();
        }
    }

    public class UIRootContext
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

        public Dictionary<UILayer, RectTransform> UILayersRectTransformDict = new();
        /// <summary>
        /// 对象池空闲对象的停放父节点
        /// </summary>
        public RectTransform PoolParent;

        public UIRootContext(
            RectTransform uiRoot,
            RectTransform canvasRectTransform,
            Camera uICamera,
            Dictionary<UILayer, RectTransform> uILayersRectTransformDict,
            RectTransform poolParent
        )
        {
            UIRoot = uiRoot;
            CanvasRectTransform = canvasRectTransform;
            UICamera = uICamera;
            UILayersRectTransformDict = uILayersRectTransformDict;
            PoolParent = poolParent;
        }
    }
}

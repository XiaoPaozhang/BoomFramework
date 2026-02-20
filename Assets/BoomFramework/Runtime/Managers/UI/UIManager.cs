using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace BoomFramework
{
    /// <summary>
    /// UI管理器
    /// </summary>
    public class UIManager : IUIManager
    {
        private string _uiLoadPath;
        private UIRootContext _uIRootContext;
        private IAssetLoadManager _assetLoadManager;
        private IObjectPoolManager _objectPoolManager;
        // 注册表：uiKey -> 元数据
        private readonly Dictionary<string, UiMetadata> _uiRegistry = new();
        // 类型索引：uiType -> uiKey（用于支持自定义 uiName）
        private readonly Dictionary<Type, string> _uiTypeToKey = new();
        // 活跃实例：uiKey -> 单实例（不允许同类多开）
        private readonly Dictionary<string, UIBase> _activeInstanceByKey = new();

        public bool IsInit { get; private set; }

        #region 上下文属性
        /// <summary>
        /// uiMono节点
        /// </summary>
        public RectTransform UIRoot;
        /// <summary>
        /// ui画布
        /// </summary>
        public RectTransform CanvasRectTransform;
        /// <summary>
        /// 画布
        /// </summary>
        public Canvas Canvas;
        /// <summary>
        /// 画布缩放器
        /// </summary>
        public CanvasScaler CanvasScaler;
        /// <summary>
        /// 画布组
        /// </summary>
        public CanvasGroup CanvasGroup;
        /// <summary>
        /// 射线检测器
        /// </summary>
        public GraphicRaycaster GraphicRaycaster;
        /// <summary>
        /// ui摄像机
        /// </summary>
        public Camera UICamera;
        /// <summary>
        /// ui层级节点字典
        /// </summary>
        public Dictionary<UILayer, RectTransform> UILayersRectTransformDict;
        #endregion

        public void Init(string uiLoadPath, UIRootContext uIRootContext, IAssetLoadManager assetLoadManager, IObjectPoolManager objectPoolManager)
        {
            IsInit = true;
            _uiLoadPath = uiLoadPath;
            _uIRootContext = uIRootContext;
            _assetLoadManager = assetLoadManager;
            _objectPoolManager = objectPoolManager;

            UIRoot = uIRootContext.UIRoot;
            CanvasRectTransform = uIRootContext.CanvasRectTransform;
            UILayersRectTransformDict = uIRootContext.UILayersRectTransformDict;

            Canvas = CanvasRectTransform.GetComponent<Canvas>();
            CanvasScaler = CanvasRectTransform.GetComponent<CanvasScaler>();
            GraphicRaycaster = CanvasRectTransform.GetComponent<GraphicRaycaster>();
            CanvasGroup = CanvasRectTransform.GetComponent<CanvasGroup>();
            UICamera = _uIRootContext.UICamera;
        }

        private sealed class UiMetadata
        {
            public string UiName;
            public Type UiType;
            public UILayer DefaultLayer;
            public string LoadPath;
        }

        private bool TryGetUiKey<T>(out string uiKey) where T : UIBase
        {
            if (_uiTypeToKey.TryGetValue(typeof(T), out uiKey))
            {
                return true;
            }
            uiKey = typeof(T).Name;
            return _uiRegistry.ContainsKey(uiKey);
        }

        public void RegisterUI<T>(string uiName = null, UILayer uILayer = UILayer.Page) where T : UIBase
        {
            uiName ??= typeof(T).Name;
            if (_uiTypeToKey.TryGetValue(typeof(T), out var existedKey))
            {
                Debug.LogError($"[{GetType().Name}]UI 类型 {typeof(T).Name} 已注册，key={existedKey}");
                return;
            }
            // ui已注册
            if (_uiRegistry.ContainsKey(uiName))
            {
                Debug.LogError($"[{GetType().Name}]UI {uiName} 已注册");
                return;
            }

            // 加载ui资源（统一使用正斜杠）
            string uiPath = Path.Combine(_uiLoadPath, uiName).Replace('\\', '/');
            GameObject uiPrefabs = _assetLoadManager.LoadAsset<GameObject>(uiPath);
            if (uiPrefabs == null)
                return;

            // ui没有挂载UIBase派生类脚本
            if (uiPrefabs.GetComponent<T>() == null)
            {
                Debug.LogError($"[{GetType().Name}]UI {uiName} 没有挂载{typeof(T).Name}脚本");
                return;
            }

            // 创建对象池：空闲对象挂在 PoolParent，避免层里堆隐藏对象
            var poolParent = _uIRootContext.PoolParent != null ? _uIRootContext.PoolParent : CanvasRectTransform;
            _objectPoolManager.CreatePool(uiName, uiPrefabs, poolParent, 1);

            // 记录元数据
            _uiRegistry.Add(uiName, new UiMetadata
            {
                UiName = uiName,
                UiType = typeof(T),
                DefaultLayer = uILayer,
                LoadPath = Path.Combine(_uiLoadPath, uiName)
            });
            _uiTypeToKey[typeof(T)] = uiName;

            _activeInstanceByKey.Remove(uiName);
            Debug.Log($"[{GetType().Name}]注册UI {uiName}，层：{uILayer}");
        }

        public T GetUI<T>() where T : UIBase
        {
            if (!TryGetUiKey<T>(out var uiName))
            {
                Debug.LogError($"[{GetType().Name}]UI {typeof(T).Name} 未注册");
                return null;
            }
            if (_activeInstanceByKey.TryGetValue(uiName, out var inst))
            {
                return inst as T;
            }
            return null;
        }

        public T OpenUI<T>(object arg = null) where T : UIBase
        {
            if (!TryGetUiKey<T>(out var uiName) || !_uiRegistry.TryGetValue(uiName, out var meta))
            {
                Debug.LogError($"[{GetType().Name}]UI {typeof(T).Name} 未注册");
                return null;
            }

            // 若已存在活跃实例，直接返回
            if (_activeInstanceByKey.TryGetValue(uiName, out var existing))
            {
                return existing as T;
            }

            // 使用实例上的脚本
            var parent = UILayersRectTransformDict[meta.DefaultLayer];
            GameObject go = _objectPoolManager.GetObject(uiName, parent);

            // 设置到顶层
            go.transform.SetAsLastSibling();

            // 调用ui脚本的业务打开
            var instance = go.GetComponent<T>();
            instance.OnOpen(arg);

            // 记录到 活跃实例 中
            _activeInstanceByKey.Add(uiName, instance);

            return instance;
        }

        public void CloseUI<T>() where T : UIBase
        {
            if (!TryGetUiKey<T>(out var uiName))
            {
                Debug.LogError($"[{GetType().Name}]关闭UI<{typeof(T).Name}>失败,该UI未注册");
                return;
            }
            if (!_activeInstanceByKey.TryGetValue(uiName, out var instance))
            {
                Debug.LogError($"[{GetType().Name}]关闭UI<{typeof(T).Name}>失败,该UI未打开");
                return;
            }

            // 先执行业务关闭
            instance.OnClose();
            // 再归还对象
            _objectPoolManager.RecycleObject(instance.gameObject);
            _activeInstanceByKey.Remove(uiName);
        }

        public void CloseAll()
        {
            // 关闭所有活跃实例（不区分层）
            if (_activeInstanceByKey.Count == 0) return;
            // 创建拷贝以避免枚举时修改字典
            var list = new List<KeyValuePair<string, UIBase>>(_activeInstanceByKey);
            foreach (var kv in list)
            {
                var uiKey = kv.Key;
                var inst = kv.Value;
                try
                {
                    inst.OnClose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[{GetType().Name}]CloseAll Close异常: {e}");
                }
                finally
                {
                    _objectPoolManager.RecycleObject(inst.gameObject);
                    _activeInstanceByKey.Remove(uiKey);
                }
            }
        }

        public void UnInit()
        {
            CloseAll();
            _uiTypeToKey.Clear();
            _uiRegistry.Clear();
            IsInit = false;
        }
    }
}

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
        // 活跃实例：uiKey -> 单实例（不允许同类多开）
        private readonly Dictionary<string, UIBase> _activeInstanceByKey = new();
        // Page 路由栈
        private readonly Stack<UIBase> _pageStack = new();

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
            public UILayer DefaultLayer;
            public string LoadPath;
        }

        public void RegisterUI<T>(string uiName = null, UILayer uILayer = UILayer.Page) where T : UIBase
        {
            uiName ??= typeof(T).Name;
            // ui已注册
            if (_uiRegistry.ContainsKey(uiName))
            {
                Debug.LogError($"[{GetType().Name}]UI {uiName} 已注册");
                return;
            }

            // ui资源加载失败
            GameObject uiPrefabs = _assetLoadManager.LoadAsset<GameObject>(Path.Combine(_uiLoadPath, uiName));
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
                DefaultLayer = uILayer,
                LoadPath = Path.Combine(_uiLoadPath, uiName)
            });

            _activeInstanceByKey.Remove(uiName);
            Debug.Log($"[{GetType().Name}]注册UI {uiName}，层：{uILayer}");
        }

        public T GetUI<T>() where T : UIBase
        {
            var uiName = typeof(T).Name;
            if (!_uiRegistry.ContainsKey(uiName))
            {
                Debug.LogError($"[{GetType().Name}]UI {uiName} 未注册");
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
            var uiName = typeof(T).Name;
            if (!_uiRegistry.TryGetValue(uiName, out var meta))
            {
                Debug.LogError($"[{GetType().Name}]UI {uiName} 未注册");
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
            var uiRectTransform = go.transform as RectTransform;

            #region 根据层级做归一化：Page 全屏拉伸；其余层仅规范变换，不改布局锚点
            if (meta.DefaultLayer == UILayer.Page)
            {
                // 全屏拉伸
                if (uiRectTransform.anchorMin != Vector2.zero || uiRectTransform.anchorMax != Vector2.one)
                {
                    uiRectTransform.anchorMin = Vector2.zero;
                    uiRectTransform.anchorMax = Vector2.one;
                    Debug.LogWarning($"[{GetType().Name}]UI {uiName} 的anchorMin或anchorMax不是0或1，{GetType().Name}已为您修改为0,1,请注意检查");
                }
                // 贴边
                if (uiRectTransform.offsetMin != Vector2.zero)
                {
                    uiRectTransform.offsetMin = Vector2.zero;
                    Debug.LogWarning($"[{GetType().Name}]UI {uiName} 的offsetMin不是0，{GetType().Name}已为您修改为0,请注意检查");
                }
                if (uiRectTransform.offsetMax != Vector2.zero)
                {
                    uiRectTransform.offsetMax = Vector2.zero;
                    Debug.LogWarning($"[{GetType().Name}]UI {uiName} 的offsetMax不是0，{GetType().Name}已为您修改为0,请注意检查");
                }
                // 居中轴心（常见页模板）
                if (uiRectTransform.pivot != new Vector2(0.5f, 0.5f))
                {
                    uiRectTransform.pivot = new Vector2(0.5f, 0.5f);
                    Debug.LogWarning($"[{GetType().Name}]UI {uiName} 的pivot不是0.5,0.5，{GetType().Name}已为您修改为0.5,0.5,请注意检查");
                }
                // 对齐父节点
                if (uiRectTransform.anchoredPosition != Vector2.zero)
                {
                    uiRectTransform.anchoredPosition = Vector2.zero;
                    Debug.LogWarning($"[{GetType().Name}]UI {uiName} 的anchoredPosition不是0，{GetType().Name}已为您修改为0,请注意检查");
                }
            }

            // 统一规范变换（避免 prefab root 被随意改动）
            if (uiRectTransform.localScale != Vector3.one)
            {
                uiRectTransform.localScale = Vector3.one;
                Debug.LogWarning($"[{GetType().Name}]UI {uiName} 的localScale不是1，{GetType().Name}已为您修改为1,请注意检查");
            }
            if (uiRectTransform.localRotation != Quaternion.identity)
            {
                uiRectTransform.localRotation = Quaternion.identity;
                Debug.LogWarning($"[{GetType().Name}]UI {uiName} 的localRotation不是0，{GetType().Name}已为您修改为0,请注意检查");
            }
            if (uiRectTransform.localPosition.z != 0f)
            {
                var lp = uiRectTransform.localPosition;
                uiRectTransform.localPosition = new Vector3(lp.x, lp.y, 0f);
                Debug.LogWarning($"[{GetType().Name}]UI {uiName} 的localPosition.z不是0，{GetType().Name}已为您修改为0,请注意检查");
            }
            #endregion

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
            var uiName = typeof(T).Name;
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

        /// <summary>
        /// 关闭 Page 栈顶页面
        /// - 若栈为空或仅有一个页面（根页），则不执行关闭并返回 false
        /// - 否则关闭栈顶页面并弹栈，返回 true
        /// </summary>
        [Obsolete("暂时废弃请使用CloseUI<T>代替")]
        public bool Back()
        {
            // 若没有页面或仅剩根页，则不允许返回
            if (_pageStack.Count <= 0)
            {
                Debug.LogError($"[{GetType().Name}]Back失败,栈中无可返回页面");
                return false;
            }

            var topUI = _pageStack.Peek();
            var uiName = topUI.GetType().Name;

            // 业务关闭
            topUI.OnClose();

            // 回收对象并维护活跃表
            _objectPoolManager.RecycleObject(topUI.gameObject);
            _activeInstanceByKey.Remove(uiName);

            // 弹栈
            if (_pageStack.Count > 0 && ReferenceEquals(_pageStack.Peek(), topUI))
            {
                _pageStack.Pop();
            }

            return _pageStack.Count > 0;
        }

        public void CloseAll()
        {
            // 关闭所有活跃实例（不区分层）
            if (_activeInstanceByKey.Count == 0) return;
            // 创建拷贝以避免枚举时修改字典
            var list = new List<UIBase>(_activeInstanceByKey.Values);
            foreach (var inst in list)
            {
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
                    _activeInstanceByKey.Remove(inst.GetType().Name);
                }
            }

            // 清空 Page 栈
            _pageStack.Clear();
        }

        public void UnInit()
        {
            IsInit = false;
        }
    }
}

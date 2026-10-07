using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// Boom框架Mono主类，用于管理所有管理器
    /// </summary>
    // 设置默认执行顺序为-1000，确保在其他组件之前执行
    [DefaultExecutionOrder(-1000)]
    // 禁止重复添加组件
    [DisallowMultipleComponent]
    // 点击组件右上角的问号就可以跳转github仓库
    [HelpURL("https://github.com/XiaoPaozhang/BoomFramework")]
    public partial class BoomFrameworkCore : MonoSingleton<BoomFrameworkCore>
    {
        #region Inspector 属性,供 Editor 使用，参见 BoomFrameworkCoreEditor.cs

        [Tooltip("游戏启动器组件")]
        [SerializeField]
        private LauncherBase _launcher;

        #endregion


        private const string FrameWorkName = "BoomFramework";
        private GameObject _frameWorkRoot;
        private Transform _frameWorkRootTransform;
        private ManagerMonoBase[] _managerMonos = Array.Empty<ManagerMonoBase>();
        private Dictionary<string, ManagerMonoBase> _managerMonosByTypeName = new();

        public Transform FrameWorkRootTransform => _frameWorkRootTransform;

        public override void Awake()
        {
            base.Awake();
            if(Instance != this) return;
            
            _frameWorkRoot = this.gameObject;
            _frameWorkRootTransform = _frameWorkRoot.transform;

            _frameWorkRoot.name = FrameWorkName;
            InitMgrMono();
            LaunchGame();
        }

        // 通过获取子节点组件拿到所有管理器mono
        private void InitMgrMono()
        {
            _managerMonos = _frameWorkRootTransform.GetComponentsInChildren<ManagerMonoBase>();
            foreach (var managerMono in _managerMonos)
            {
                managerMono.Init();
                _managerMonosByTypeName.Add(managerMono.GetType().Name, managerMono);
            }

        }

        private void UnInitMgrMono()
        {
            // 按初始化的逆序清理，依赖的服务在使用它们的模块之后销毁。
            for (int i = _managerMonos.Length - 1; i >= 0; i--)
            {
                var managerMono = _managerMonos[i];
                if (managerMono != null && managerMono.IsInited)
                {
                    managerMono.UnInit();
                }
                else
                {
                    Debug.LogWarning($"管理器跳过注销：未初始化或实例为空，索引 {i}");
                }
            }
        }

        // 启动游戏项目
        private void LaunchGame()
        {
            if (_launcher == null)
            {
                Debug.LogError($"[{GetType().Name}]未选择启动器，点我查看框架预制体Launcher", this);
                return;
            }

            try
            {
                Debug.Log($"[启动游戏]: 启动器名称: {_launcher.GetType().Name}", _launcher);
                StartCoroutine(_launcher.Launch());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}]启动器执行失败: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public T GetManagerMono<T>() where T : ManagerMonoBase
        {
            return _managerMonosByTypeName.TryGetValue(typeof(T).Name, out var manager) ? manager as T : null;
        }

        void OnDestroy()
        {
            UnInitMgrMono();
        }
    }
}

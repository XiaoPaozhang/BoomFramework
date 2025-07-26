using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
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
    // 添加组件菜单
    [AddComponentMenu("BoomFramework/BoomFramework")]
    // 点击组件右上角的问号就可以跳转github仓库
    [HelpURL("https://github.com/XiaoPaozhang/BoomFramework")]
    public partial class BoomFrameworkMono : MonoSingleton<BoomFrameworkMono>
    {
        private const string FrameWorkName = "BoomFramework";
        private GameObject _frameWorkRoot;
        private Transform _frameWorkRootTransform;
        private ServiceLocator _serviceLocator;
        private IManager[] _monoManagers;

        public override void Awake()
        {
            base.Awake();
            _frameWorkRoot = this.gameObject;
            _frameWorkRootTransform = _frameWorkRoot.transform;
            _serviceLocator = ServiceLocator.Instance;

            _frameWorkRoot.name = FrameWorkName;

            Debug.Log("==========初始化静态门户API==========");
            InitStaticAPI();

            Debug.Log("==========初始化管理器==========");
            InitMonoManager();

            Debug.Log("==========注册业务服务==========");
            RegisterService();

            Debug.Log("==========启动入口脚本==========");
            StartGameProject();
        }

        private void InitStaticAPI()
        {
            Debug.Log("事件管理器, 初始化");
            BoomEvent.Init(new EventManager());
        }

        private void InitMonoManager()
        {
            _monoManagers = _frameWorkRoot.GetComponentsInChildren<IManager>();
            foreach (var manager in _monoManagers)
            {
                manager.Init();
            }
        }

        private void UnInitMonoManager()
        {
            foreach (var manager in _monoManagers)
            {
                manager.UnInit();
            }
        }

        private void RegisterService()
        {

        }

        void OnDestroy()
        {
            UnInitMonoManager();
        }
    }
}

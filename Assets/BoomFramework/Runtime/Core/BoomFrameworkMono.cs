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
        private ServiceContainer _serviceLocator;
        private ManagerMonoBase[] _monoManagers;

        public override void Awake()
        {
            base.Awake();
            _frameWorkRoot = this.gameObject;
            _frameWorkRootTransform = _frameWorkRoot.transform;
            _serviceLocator = ServiceContainer.Instance;

            _frameWorkRoot.name = FrameWorkName;

            Debug.Log($"[{GetType().Name}]初始化管理器Mono");
            InitMgrMono();

            Debug.Log($"[{GetType().Name}]注册业务服务");
            RegisterService();

            Debug.Log($"[{GetType().Name}]初始化静态门户API");
            InitStaticAPI();

            Debug.Log($"[{GetType().Name}]启动入口脚本");
            StartGameProject();
        }

        private void InitStaticAPI()
        {
            BoomEvent.Init(_serviceLocator.GetService<IEventManager>());
            Debug.Log($"{nameof(IEventManager)}, 初始化成功");
        }

        private void InitMgrMono()
        {
            _monoManagers = _frameWorkRoot.GetComponentsInChildren<ManagerMonoBase>();
            foreach (var manager in _monoManagers)
            {
                manager.Init();
            }
        }

        private void UnInitMgrMono()
        {
            foreach (var manager in _monoManagers)
            {
                manager.UnInit();
            }
        }

        private void RegisterService()
        {
            var serviceRegisters = ReflectionUtility.GetAllTypes<IServiceRegister>();
            foreach (var serviceRegister in serviceRegisters)
            {
                var serviceRegisterInstance = Activator.CreateInstance(serviceRegister) as IServiceRegister;
                serviceRegisterInstance.RegisterServices(_serviceLocator);
            }
        }

        void OnDestroy()
        {
            UnInitMgrMono();
        }
    }
}

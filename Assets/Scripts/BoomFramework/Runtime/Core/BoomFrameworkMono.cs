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
        private MgrMonoBase[] _monoManagers;

        public override void Awake()
        {
            base.Awake();
            _frameWorkRoot = this.gameObject;
            _frameWorkRootTransform = _frameWorkRoot.transform;
            _serviceLocator = ServiceContainer.Instance;

            _frameWorkRoot.name = FrameWorkName;

            Debug.Log("==========初始化管理器Mono==========");
            InitMgrMono();

            Debug.Log("==========注册业务服务==========");
            RegisterService();

            Debug.Log("==========初始化静态门户API==========");
            InitStaticAPI();

            Debug.Log("==========启动入口脚本==========");
            StartGameProject();
        }

        private void InitStaticAPI()
        {
            Debug.Log("事件管理器, 初始化");
            BoomEvent.Init(_serviceLocator.GetService<IEventMgr>());
        }

        private void InitMgrMono()
        {
            _monoManagers = _frameWorkRoot.GetComponentsInChildren<MgrMonoBase>();
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
            // 首先注册框架自身到服务容器，供其他服务使用（如ABMgr需要启动协程）
            _serviceLocator.RegisterService<BoomFrameworkMono>(this);

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

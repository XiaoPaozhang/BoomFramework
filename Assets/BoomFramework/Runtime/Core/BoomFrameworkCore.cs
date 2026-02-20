using System;
using System.Collections.Generic;
using System.Linq;
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
        private const string FrameWorkName = "BoomFramework";
        private GameObject _frameWorkRoot;
        private Transform _frameWorkRootTransform;
        private ServiceContainer _serviceLocator;
        private Dictionary<string, ManagerMonoBase> _managerMonosByTypeName = new();

        public Transform FrameWorkRootTransform => _frameWorkRootTransform;

        public override void Awake()
        {
            base.Awake();
            _frameWorkRoot = this.gameObject;
            _frameWorkRootTransform = _frameWorkRoot.transform;
            _serviceLocator = ServiceContainer.Instance;

            _frameWorkRoot.name = FrameWorkName;
            InitMgrMono();
            RegisterService();
            InitStaticAPI();
            LaunchGame();
        }

        // 通过获取子节点组件拿到所有管理器mono
        private void InitMgrMono()
        {
            ManagerMonoBase[] _managerMonos = _frameWorkRootTransform.GetComponentsInChildren<ManagerMonoBase>();
            foreach (var managerMono in _managerMonos)
            {
                managerMono.Init();
                _managerMonosByTypeName.Add(managerMono.GetType().Name, managerMono);
            }
            
        }
        
        // 初始化静态门户API
        private void InitStaticAPI()
        {
            BoomEvent.Init(_serviceLocator.GetService<IEventManager>());
        }


        private void UnInitMgrMono()
        {
            foreach (var managerMono in _managerMonosByTypeName)
            {
                if (managerMono.Value != null && managerMono.Value.IsInited)
                {
                    managerMono.Value.UnInit();
                }
                else
                {
                    Debug.LogWarning($"{managerMono.Key}跳过注销：未初始化或实例为空");
                }
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

        // 启动游戏项目
        private void LaunchGame()
        {
            if (string.IsNullOrEmpty(_selectedLauncherTypeName))
            {
                Debug.LogWarning($"[{GetType().Name}]未选择启动器，清关闭游戏检查框架预制体");
                return;
            }

            var launcherType = Type.GetType(_selectedLauncherTypeName);
            if (launcherType == null)
            {
                Debug.LogError($"[{GetType().Name}]无法找到启动器类型: {_selectedLauncherTypeName}");
                return;
            }

            if (!typeof(ILauncher).IsAssignableFrom(launcherType))
            {
                Debug.LogError($"[{GetType().Name}]类型 {launcherType.Name} 未实现 ILauncher 接口");
                return;
            }

            try
            {
                var launcher = Activator.CreateInstance(launcherType) as ILauncher;
                Debug.Log($"[启动游戏]: 启动器名称: {launcherType.Name}");
                launcher.Launch();
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

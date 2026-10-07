using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 负责事件管理器的创建、服务注册和清理。
    /// </summary>
    [DisallowMultipleComponent]
    public class EventMono : ManagerMonoBase
    {
        private IEventManager _eventManager;

        protected override bool OnInit()
        {
            _eventManager = new EventManager();
            _eventManager.Init();
            ServiceContainer.Instance.RegisterService<IEventManager>(_eventManager);
            return true;
        }

        protected override void OnUnInit()
        {
            _eventManager?.UnInit();
            ServiceContainer.Instance.UnRegisterService<IEventManager>();
            _eventManager = null;
        }
    }
}

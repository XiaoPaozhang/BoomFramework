using System.Collections;
using System.Collections.Generic;
using BoomFramework;
using UnityEngine;

namespace BoomFramework{
    public class FsmMono : ManagerMonoBase
    {
        private IFsmManager _fsmManager;
        protected override bool OnInit()
        {
            _fsmManager = new FsmManager();
            _fsmManager.Init();
            ServiceContainer.Instance.RegisterService<IFsmManager>(_fsmManager);
            return true;
        }

        void Update()
        {
            if (_fsmManager == null) return;
            _fsmManager.OnUpdate(Time.deltaTime);
        }

        protected override void OnUnInit()
        {
            _fsmManager?.UnInit();
            ServiceContainer.Instance.UnRegisterService<IFsmManager>();
            _fsmManager = null;
        }
    }
}

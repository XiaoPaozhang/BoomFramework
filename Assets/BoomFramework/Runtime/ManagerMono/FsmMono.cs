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
            if (!base.OnInit()) return false;
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

        void OnDestroy()
        {
            _fsmManager?.OnDesdroy();
        }
    }
}

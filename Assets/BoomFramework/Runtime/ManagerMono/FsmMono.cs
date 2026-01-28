using System.Collections;
using System.Collections.Generic;
using BoomFramework;
using UnityEngine;

namespace BoomFramework{
    public class FsmMono : ManagerMonoBase
    {
        private IFsmManager _fsmManager;
        protected override void OnInit()
        {
            base.OnInit();
            _fsmManager = new FsmManager();
            _fsmManager.Init();
            ServiceContainer.Instance.RegisterService<IFsmManager>(_fsmManager);
        }

        void Update()
        {
            _fsmManager.OnUpdate(Time.deltaTime);
        }

        void OnDestroy()
        {
            _fsmManager.OnDesdroy();
        }
    }
}

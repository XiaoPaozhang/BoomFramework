using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    public interface IState
    {
        void OnCreate(IFsm fsm);
        void OnEnter();
        void OnUpdate(float deltaTime);
        void OnExit();
        void OnDestroyed();
    }
}
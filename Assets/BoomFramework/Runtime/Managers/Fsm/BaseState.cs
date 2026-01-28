using UnityEngine;

namespace BoomFramework
{
    public abstract class BaseState : IState
    {
        protected IFsm fsm;

        protected virtual void OnCreate() { }

        protected virtual void OnEnter() { }

        protected virtual void OnExit() { }

        protected virtual void OnDestroy() { }

        protected virtual void OnUpdate(float deltaTime) { }

        void IState.OnCreate(IFsm fsm)
        {
            Debug.Log($"<color=#FFA500>{GetType().Name} created</color>");
            this.fsm = fsm;
            OnCreate();
        }

        void IState.OnEnter()
        {
            Debug.Log($"<color=#00FF00>{GetType().Name} entered</color>");
            OnEnter();
        }

        void IState.OnUpdate(float deltaTime)
        {
            OnUpdate(deltaTime);
        }

        void IState.OnExit()
        {
            Debug.Log($"<color=#FF0000>{GetType().Name} exited</color>");
            OnExit();
        }

        void IState.OnDestroyed()
        {
            Debug.Log($"<color=#FF0000>{GetType().Name} exited</color>");
            OnDestroy();
        }
    }
}
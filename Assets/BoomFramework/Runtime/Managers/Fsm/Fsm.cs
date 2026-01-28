using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 有限状态机(单个)
    /// </summary>
    public class Fsm : IFsm
    {
        private Dictionary<string, IState> _states = new Dictionary<string, IState>();
        public string FsmName { get; private set; }
        public string CurrentStateKey { get; private set; } = string.Empty;
        public IState CurrentState { get; private set; }

        public void Init(string fsmName)
        {
            FsmName = fsmName;
        }

        public IFsm AddState(string stateName, IState state)
        {
            if (!_states.ContainsKey(stateName))
            {
                _states.Add(stateName, state);
                state.OnCreate(this);
            }
            else
            {
                Debug.LogError($"状态 {stateName} 已经存在于Fsm {FsmName} 中");
            }

            return this;
        }

        public IFsm SwitchState(string newStateName)
        {
            if (CurrentState == null)
            {
                Debug.LogError($"Fsm {FsmName} 尚未启动或已被关闭,无法切换状态");
                return this;
            }

            if (CurrentStateKey != string.Empty && CurrentStateKey.Equals(newStateName))
            {
                Debug.LogError($"状态 {newStateName} 与当前状态 {CurrentStateKey} 相同，无需切换");
                return this;
            }

            if (!_states.TryGetValue(newStateName, out var state))
            {
                Debug.LogError($"状态 {newStateName} 不存在于Fsm {FsmName} 中");
                return this;
            }

            CurrentState?.OnExit();
            CurrentStateKey = newStateName;
            CurrentState = state;
            CurrentState.OnEnter();
            return this;
        }

        public IFsm Start(string initialStateName)
        {
            if (CurrentState != null)
            {
                Debug.LogError($"Fsm {FsmName} 已经启动，不能重复启动");
                return this;
            }

            if (!_states.TryGetValue(initialStateName, out var state))
            {
                Debug.LogError($"状态 {initialStateName} 不存在于Fsm {FsmName} 中");
                return this;
            }

            CurrentStateKey = initialStateName;
            CurrentState = state;
            CurrentState.OnEnter();
            return this;
        }

        public IFsm Stop()
        {
            if (CurrentState == null) return this;
            CurrentState?.OnExit();
            CurrentStateKey = string.Empty;
            CurrentState = null;
            return this;
        }

        public void Update(float deltaTime)
        {
            if (CurrentState == null) return;
            CurrentState.OnUpdate(deltaTime);
        }

        public bool HasState(string stateName)
        {
            return _states.ContainsKey(stateName);
        }
    }
}
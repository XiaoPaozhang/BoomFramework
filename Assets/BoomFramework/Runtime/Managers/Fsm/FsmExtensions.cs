using System;

namespace BoomFramework
{
    public static class FsmExtensions
    {
        public static IFsm AddState(this IFsm fsm, Enum stateName, IState state)
        {
            return fsm.AddState(stateName.ToString(), state);
        }

        public static IFsm SwitchState(this IFsm fsm, Enum newStateName)
        {
            return fsm.SwitchState(newStateName.ToString());
        }

        public static IFsm Start(this IFsm fsm, Enum initialStateName)
        {
            return fsm.Start(initialStateName.ToString());
        }

        public static IFsm AddState(this IFsm fsm, IState state)
        {
            return fsm.AddState(state.GetType().Name, state);
        }

        public static IFsm SwitchState<T>(this IFsm fsm) where T : IState
        {
            return fsm.SwitchState(typeof(T).Name);
        }

        public static IFsm Start<T>(this IFsm fsm) where T : IState
        {
            return fsm.Start(typeof(T).Name);
        }
    }
}
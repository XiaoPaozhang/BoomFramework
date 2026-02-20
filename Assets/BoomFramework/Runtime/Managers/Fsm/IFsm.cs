using System;

namespace BoomFramework
{
  /// <summary>
  /// 定义有限状态机的接口。
  /// </summary>
  public interface IFsm
  {
    /// <summary>
    /// 获取有限状态机的名称。
    /// </summary>
    string FsmName { get; }

    /// <summary>
    /// 获取当前状态对应的枚举键值，用于标识当前状态。
    /// </summary>
    string CurrentStateKey { get; }

    /// <summary>
    /// 获取当前状态。
    /// </summary>
    IState CurrentState { get; }

    /// <summary>
    /// 初始化状态机。
    /// </summary>
    /// <param name="fsmName">状态机的名称。</param>
    void Init(string fsmName);

    /// <summary>
    /// 添加状态到状态机中。
    /// </summary>
    /// <param name="stateName">状态对应的枚举值。</param>
    /// <param name="state">状态对象。</param>
    /// <returns>返回当前状态机实例。</returns>
    IFsm AddState(string stateName, IState state); // 改为字符串类型

    /// <summary>
    /// 切换到指定状态。
    /// </summary>
    /// <param name="newStateName">目标状态对应的枚举值。</param>
    /// <returns>返回当前状态机实例。</returns>
    IFsm SwitchState(string newStateName); // 改为字符串类型

    /// <summary>
    /// 启动状态机并进入指定的初始状态。
    /// </summary>
    /// <param name="initialStateName">初始状态对应的枚举值。</param>
    /// <returns>返回当前状态机实例。</returns>
    IFsm Start(string initialStateName); // 改为字符串类型

    /// <summary>
    /// 停止状态机运行。
    /// </summary>
    /// <returns>返回当前状态机实例。</returns>
    IFsm Stop();

    /// <summary>
    /// 销毁状态机，调用所有状态的销毁回调并清理内部数据。
    /// </summary>
    void Destroy();

    /// <summary>
    /// 更新当前状态。
    /// </summary>
    /// <param name="deltaTime">时间增量。</param>
    void Update(float deltaTime);

    /// <summary>
    /// 判断状态机中是否包含指定的状态。
    /// </summary>
    /// <param name="stateName">状态对应的枚举值。</param>
    /// <returns>若包含返回 true，否则返回 false。</returns>
    bool HasState(string stateName); // 改为字符串类型
  }
}
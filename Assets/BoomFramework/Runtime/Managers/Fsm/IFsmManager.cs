using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
  /// <summary>
  /// FSM 管理器接口，负责管理所有有限状态机实例
  /// </summary>
  public interface IFsmManager
  {
    /// <summary>
    /// 是否被初始化
    /// </summary>
    bool IsInit { get; }
    /// <summary>
    /// 获取当前注册的 FSM 实例数量
    /// </summary>
    int FsmCount { get; }
    /// <summary>
    /// 初始化
    /// </summary>
    void Init();
    /// <summary>
    /// 创建一个 FSM 实例
    /// </summary>
    /// <param name="fsmName">要注册的 FSM 名字</param>
    T CreateFsm<T>(string fsmName) where T : IFsm, new();

    /// <summary>
    /// 删除一个 FSM 实例
    /// </summary>
    /// <param name="fsm">要删除的 FSM 名字</param>
    void RemoveFsm(string fsmName);

    /// <summary>
    /// 更新所有 FSM 实例的状态
    /// </summary>
    /// <param name="deltaTime">上一帧到当前帧的时间间隔</param>
    void OnUpdate(float deltaTime);

    /// <summary>
    /// 根据名称获取 FSM 实例
    /// </summary>
    /// <param name="name">FSM 实例的名称</param>
    /// <returns>对应名称的 FSM 实例，如果不存在则返回 null</returns>
    IFsm GetFsm(string name);

    /// <summary>
    /// 检查是否存在指定名称的 FSM 实例
    /// </summary>
    /// <param name="name">要检查的 FSM 实例名称</param>
    /// <returns>如果存在则返回 true，否则返回 false</returns>
    bool HasFsm(string name);

    /// <summary>
    /// 关闭指定名称的状态机实例
    /// </summary>
    /// <param name="fsmName">要关闭的状态机名称</param>
    void ShutdownFsm(string fsmName);

    /// <summary>
    /// 销毁 FSM 管理器
    /// </summary>
    void OnDesdroy();
  }
}
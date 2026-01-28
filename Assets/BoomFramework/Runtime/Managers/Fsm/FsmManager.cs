using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{

  /// <summary>
  /// 状态机管理器
  /// </summary>
  public class FsmManager : IFsmManager
  {
    private Dictionary<string, IFsm> fsms = new Dictionary<string, IFsm>();
    public int FsmCount => fsms.Count;
    public bool IsInit { get; private set; }
    public void Init()
    {
      IsInit = true;
    }
    public T CreateFsm<T>(string fsmName) where T : IFsm, new()
    {
      if (!IsInit)
      {
        Debug.LogError("状态机管理器未被初始化");
        return default(T);
      }

      if (!fsms.TryGetValue(fsmName, out var fsm))
      {
        fsm = new T();
        fsm.Init(fsmName);
        fsms.Add(fsmName, fsm);
      }
      return (T)fsms[fsmName];
    }

    public void RemoveFsm(string fsmName)
    {
      if (fsms.ContainsKey(fsmName))
      {
        fsms.Remove(fsmName);
      }
    }

    public void ShutdownFsm(string fsmName)
    {
      if (fsms.TryGetValue(fsmName, out var fsm))
      {
        fsm.Stop();
      }
    }

    public void OnUpdate(float deltaTime)
    {
      if (!IsInit) return;
      foreach (var fsm in fsms.Values)
      {
        fsm.Update(deltaTime);
      }
    }

    public IFsm GetFsm(string fsmName)
    {
      if (fsms.TryGetValue(fsmName, out var fsm))
      {
        return fsm;
      }
      return null;
    }

    public bool HasFsm(string fsmName)
    {
      return fsms.ContainsKey(fsmName);
    }

    public void OnDesdroy()
    {
      foreach (var fsm in fsms.Values)
      {
        fsm.Stop();
      }
      fsms.Clear();
    }
  }
}
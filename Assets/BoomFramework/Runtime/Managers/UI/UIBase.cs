using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
  /// <summary>
  /// UI基类
  /// </summary>
  public abstract class UIBase : MonoBehaviour
  {

    private void Awake()
    {
      _ = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    public virtual void OnOpen(object arg)
    {
      Debug.Log($"{GetType().Name} UI打开,参数：{arg}");
    }

    public virtual void OnClose()
    {
      Debug.Log($"{GetType().Name} UI关闭");
    }

  }
}

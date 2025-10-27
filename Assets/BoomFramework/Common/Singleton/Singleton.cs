using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// C# 单例基类
    /// </summary>
    public class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;
        public static T Instance => _instance ??= new T();
    }
}

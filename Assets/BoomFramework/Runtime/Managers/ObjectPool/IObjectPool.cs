using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    public interface IObjectPool
    {
        int IdleCount { get; }
        int ActiveCount { get; }
        int TotalCount { get; }
        int PoolSize { get; }

        GameObject GetObject();
        void RecycleAllObjects();
        void RecycleObject(GameObject obj);
        void Clear();
    }
}

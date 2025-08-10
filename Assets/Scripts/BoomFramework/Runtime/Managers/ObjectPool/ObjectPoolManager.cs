using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    public class ObjectPoolManager : IObjectPoolManager
    {
        public bool IsInit { get; private set; }

        /// <summary>
        /// 资源管理由外部提供
        /// </summary>
        private IAssetLoadManager _assetManager;
        private Dictionary<string, ObjectPool> _poolsDict = new();
        // 实例归属登记：字典<实例ID, 对象池名称>
        private Dictionary<int, string> _instanceToPool = new();

        public void Init(IAssetLoadManager assetManager)
        {
            IsInit = true;
            _assetManager = assetManager;
        }

        public void CreatePool(string poolName, Transform parent, int poolSize)
        {
            if (_assetManager == null)
            {
                Debug.LogError("资产管理器未初始化");
                return;
            }
            GameObject prefab = _assetManager.LoadAsset<GameObject>(poolName);
            if (prefab == null)
            {
                Debug.LogError($"资源 {poolName} 不存在");
                return;
            }
            CreatePool(poolName, prefab, parent, poolSize);
        }

        public void CreatePool(string poolName, GameObject prefab, Transform parent, int poolSize)
        {
            if (_poolsDict.ContainsKey(poolName))
            {
                Debug.LogError($"对象池 {poolName} 已经存在");
                return;
            }
            _poolsDict.Add(poolName, new ObjectPool(poolName, prefab, poolSize, parent));
        }

        public GameObject RentObject(string poolName)
        {
            if (!_poolsDict.TryGetValue(poolName, out var objectPool))
            {
                Debug.LogError($"对象池 {poolName} 不存在");
                return null;
            }
            var obj = objectPool.RentObject();
            if (obj != null)
            {
                _instanceToPool[obj.GetInstanceID()] = poolName;
            }
            return obj;
        }

        public GameObject RentObject(string poolName, Transform parent, Vector3? localPos = null, Quaternion? localRot = null)
        {
            var obj = RentObject(poolName);
            if (obj == null) return null;

            if (parent != null)
            {
                obj.transform.SetParent(parent, false);
            }
            if (localPos.HasValue)
            {
                obj.transform.localPosition = localPos.Value;
            }
            if (localRot.HasValue)
            {
                obj.transform.localRotation = localRot.Value;
            }
            return obj;
        }

        public void ReturnObject(string poolName, GameObject obj)
        {
            if (!_poolsDict.TryGetValue(poolName, out var objectPool))
            {
                Debug.LogError($"对象池 {poolName} 不存在");
                return;
            }
            // 若登记的归属与传入不一致，发出警告，但以登记为准
            if (obj != null && _instanceToPool.TryGetValue(obj.GetInstanceID(), out var recordedPool) && recordedPool != poolName)
            {
                Debug.LogWarning($"对象池归还不一致：传入 {poolName}，登记为 {recordedPool}，已按登记归还");
                if (_poolsDict.TryGetValue(recordedPool, out var recordedObjectPool))
                {
                    recordedObjectPool.ReturnObject(obj);
                    _instanceToPool.Remove(obj.GetInstanceID());
                    return;
                }
            }

            objectPool.ReturnObject(obj);
            if (obj != null)
            {
                _instanceToPool.Remove(obj.GetInstanceID());
            }
        }

        // 便捷归还：无需指定池名
        public void ReturnObject(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("ReturnObject 传入空对象");
                return;
            }
            var id = obj.GetInstanceID();
            if (!_instanceToPool.TryGetValue(id, out var poolName))
            {
                Debug.LogWarning($"未找到对象 {obj.name} 的归属池，可能未从池租借或已被外部销毁/重复归还");
                return;
            }
            ReturnObject(poolName, obj);
        }
        public void ReturnAllObjects(string poolName)
        {
            if (!_poolsDict.TryGetValue(poolName, out var objectPool))
            {
                Debug.LogError($"对象池 {poolName} 不存在");
                return;
            }

            // 先清理登记
            var ids = new List<int>();
            foreach (var kv in _instanceToPool)
            {
                if (kv.Value == poolName)
                {
                    ids.Add(kv.Key);
                }
            }
            for (int i = 0; i < ids.Count; i++)
            {
                _instanceToPool.Remove(ids[i]);
            }

            // 再调用池内召回
            objectPool.ReturnAllObjects();
        }
        public void RemovePool(string poolName)
        {
            if (!_poolsDict.TryGetValue(poolName, out var objectPool))
            {
                Debug.LogError($"对象池 {poolName} 不存在");
                return;
            }
            // 尝试先召回活跃对象，避免遗留登记
            objectPool.ReturnAllObjects();
            // 清理登记中属于该池的实例
            var snapshot = new List<int>();
            foreach (var kv in _instanceToPool)
            {
                if (kv.Value == poolName) snapshot.Add(kv.Key);
            }
            for (int i = 0; i < snapshot.Count; i++) _instanceToPool.Remove(snapshot[i]);

            objectPool.Clear();
            _poolsDict.Remove(poolName);
        }

        public bool HasPool(string poolName)
        {
            return _poolsDict.ContainsKey(poolName);
        }

        public void ClearAllPool()
        {
            foreach (var pool in _poolsDict)
            {
                pool.Value.ReturnAllObjects();
                pool.Value.Clear();
            }
            _poolsDict.Clear();
            _instanceToPool.Clear();
        }

        public void UnInit()
        {
            IsInit = false;
            _assetManager = null;
            ClearAllPool();
        }
    }
}

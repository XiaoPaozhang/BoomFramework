using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 对象池子
    /// </summary>
    public class ObjectPool : IObjectPool
    {
        /// <summary>
        /// 池子名称
        /// </summary>
        private string _poolName;
        /// <summary>
        /// 预制体（仅作模板，不入池）
        /// </summary>
        private GameObject _prefab;
        /// <summary>
        /// 空闲对象栈
        /// </summary>
        private Stack<GameObject> _idleObjects = new();
        /// <summary>
        /// 活跃对象集合
        /// </summary>
        private HashSet<GameObject> _activeObjects = new();
        /// <summary>
        /// 空闲对象的父节点
        /// </summary>
        private Transform _parent;

        /// <summary>
        /// 初始数量
        /// </summary>
        private int _poolSize;
        /// <summary>
        /// 空闲对象数量
        /// </summary>
        public int IdleCount => _idleObjects.Count;
        /// <summary>
        /// 活跃对象数量
        /// </summary>
        public int ActiveCount => _activeObjects.Count;
        /// <summary>
        /// 总对象数量
        /// </summary>
        public int TotalCount => IdleCount + ActiveCount;
        /// <summary>
        /// 池子大小
        /// </summary>
        public int PoolSize => _poolSize;

        public ObjectPool(string poolName, GameObject prefab, int poolSize, Transform parent)
        {
            _poolName = poolName;
            _poolSize = Mathf.Max(0, poolSize);
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < _poolSize; i++)
            {
                var obj = GameObject.Instantiate(_prefab, _parent);
                obj.SetActive(false);
                _idleObjects.Push(obj);
            }
        }

        public GameObject GetObject()
        {
            GameObject obj = _idleObjects.Count > 0
                ? _idleObjects.Pop()
                : GameObject.Instantiate(_prefab, _parent);

            obj.SetActive(true);

            _activeObjects.Add(obj);
            return obj;
        }

        public void RecycleObject(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning($"对象池 {_poolName}: 试图归还空对象");
                return;
            }

            if (!_activeObjects.Contains(obj))
            {
                Debug.LogWarning($"对象池 {_poolName}: 归还未登记为活跃的对象 {obj.name}，可能重复归还或归还到错误的池");
                return;
            }

            // 还原到池的父节点，确保层级容器不堆积隐藏对象
            if (obj.transform.parent != _parent)
            {
                obj.transform.SetParent(_parent, false);
            }
            obj.SetActive(false);
            _activeObjects.Remove(obj);
            _idleObjects.Push(obj);
        }

        public void RecycleAllObjects()
        {
            if (_activeObjects.Count == 0) return;

            // 拷贝快照，避免遍历期间集合被修改
            var snapshot = new List<GameObject>(_activeObjects);
            for (int i = 0; i < snapshot.Count; i++)
            {
                RecycleObject(snapshot[i]);
            }
        }

        public void Clear()
        {
            while (_idleObjects.Count > 0)
            {
                var go = _idleObjects.Pop();
                if (go != null)
                {
                    GameObject.Destroy(go);
                }
            }

            if (_activeObjects.Count > 0)
            {
                Debug.LogWarning($"对象池 {_poolName}: 清理时仍有 {_activeObjects.Count} 个活跃对象未归还");
            }
        }

    }
}

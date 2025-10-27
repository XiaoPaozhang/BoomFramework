
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    public class ObjectPoolTest : MonoBehaviour
    {


        private IAssetLoadManager _assetLoadManager;
        private IObjectPoolManager _objectPoolManager;
        [SerializeField]
        [Range(0, 10)]
        private int _RentCount = 5;

        [SerializeField]
        [Range(0, 5)]
        private int _delayReturnTime = 1;

        void Start()
        {
            _assetLoadManager = ServiceContainer.Instance.GetService<IAssetLoadManager>();
            _objectPoolManager = ServiceContainer.Instance.GetService<IObjectPoolManager>();
            // 第一种写法
            // 先加载预制体资源
            // 再拿预制体资源去创建对象池
            // var cube = _assetLoadManager.LoadAsset<GameObject>("Cube");
            // _objectPoolManager.CreatePool("Cube", cube, transform, _RentCount);


            // 第二种写法(推荐)
            // 默认填入的对象池名称作为资源路径
            _objectPoolManager.CreatePool("Cube", transform, _RentCount);

            //判断是否存在对象池
            bool hasCubePool = _objectPoolManager.HasPool("Cube");

            if (hasCubePool)
            {
                // 获取对象
                for (int i = 0; i < _RentCount; i++)
                {
                    var cubeobj = _objectPoolManager.GetObject("Cube");
                }
                StartCoroutine(RecycleObjectAll());
            }
        }

        private IEnumerator RecycleObjectAll()
        {
            yield return new WaitForSeconds(_delayReturnTime);
            _objectPoolManager.RecycleAllObjects("Cube");
        }

        void Update()
        {

        }

    }
}


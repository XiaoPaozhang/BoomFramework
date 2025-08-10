
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Examples;
using UnityEngine;

namespace BoomFramework
{
    public class ObjectPoolTest : MonoBehaviour
    {


        private IAssetLoadManager _assetLoadManager;
        private IObjectPoolManager _objectPoolManager;
        [SerializeField]
        [LabelText("租借数量")]
        [Range(0, 10)]
        private int _RentCount = 5;

        [SerializeField]
        [LabelText("延迟归还时间")]
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
                // 租借对象
                for (int i = 0; i < _RentCount; i++)
                {
                    var cubeobj = _objectPoolManager.RentObject("Cube", Camera.main.transform);
                }
                StartCoroutine(ReturnObjectAll());
            }
        }

        private IEnumerator ReturnObjectAll()
        {
            yield return new WaitForSeconds(_delayReturnTime);
            _objectPoolManager.ReturnAllObjects("Cube");
        }

        void Update()
        {

        }

    }
}


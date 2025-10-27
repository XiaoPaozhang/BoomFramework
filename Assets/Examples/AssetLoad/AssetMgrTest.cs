using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 资源管理器测试示例
    /// 展示通过Inspector配置的资源加载方式
    /// </summary>
    public class AssetMgrTest : MonoBehaviour
    {
        private global::BoomFramework.IAssetLoadManager _assetLoadManager;
        private string _path = "Prefabs/Cube";

        void Start()
        {
            // 获取根据Inspector配置创建的资源管理器
            _assetLoadManager = ServiceContainer.Instance.GetService<global::BoomFramework.IAssetLoadManager>();

            if (_assetLoadManager != null)
            {
                Debug.Log($"获取到资产服务: {_assetLoadManager.GetType().Name}");

                // 测试加载资源（需要在Resources文件夹下有这个资源）
                var cube = _assetLoadManager.LoadAsset<GameObject>(_path);
                if (cube != null)
                {
                    Instantiate(cube, transform);
                }
            }
            else
            {
                Debug.LogError("未找到资产服务，请确保资产提供组件已正确配置并启用");
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (_assetLoadManager != null)
                {
                    Debug.Log($"当前使用的资产服务: {_assetLoadManager.GetType().Name}");
                }
                else
                {
                    Debug.LogWarning("资产服务为空");
                }
            }
        }
    }
}

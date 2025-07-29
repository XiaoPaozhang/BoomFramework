using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// 资源管理器测试示例
    /// 展示通过Inspector配置的资源加载方式
    /// </summary>
    public class ResourceMgrTest : MonoBehaviour
    {
        private IResourceMgr _resourceMgr;
        private string _path = "Cube";

        void Start()
        {
            // 获取根据Inspector配置创建的资源管理器
            _resourceMgr = ServiceContainer.Instance.GetService<IResourceMgr>();

            if (_resourceMgr != null)
            {
                Debug.Log($"获取到资源管理器: {_resourceMgr.GetType().Name}");

                // 测试加载资源（需要在Resources文件夹下有这个资源）
                var cube = _resourceMgr.LoadAsset<GameObject>(_path);
                if (cube != null)
                {
                    Instantiate(cube, transform);
                }
            }
            else
            {
                Debug.LogError("未找到资源管理器，请确保ResourceMono组件已正确配置并启用");
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (_resourceMgr != null)
                {
                    Debug.Log($"当前使用的资源管理器: {_resourceMgr.GetType().Name}");
                }
                else
                {
                    Debug.LogWarning("资源管理器为空");
                }
            }
        }
    }
}

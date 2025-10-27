using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// BoomFrameworkCore 的 Inspector 相关字段
    /// </summary>
    public partial class BoomFrameworkCore
    {
#pragma warning disable CS0414 // 字段已赋值但从未使用（仅在 Editor 中使用）
        [Header("启动器配置")]
        [Tooltip("是否显示启动器的完整类型名（包含命名空间）")]
        [SerializeField]
        private bool _isShowFullName = false;
#pragma warning restore CS0414

        [Tooltip("选中的启动器类型全名（AssemblyQualifiedName）")]
        [SerializeField]
        private string _selectedLauncherTypeName = string.Empty;
    }
}


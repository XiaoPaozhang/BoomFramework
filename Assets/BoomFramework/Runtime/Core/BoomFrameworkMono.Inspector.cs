using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// BoomFrameworkMono.Inspector
    /// </summary>
    public partial class BoomFrameworkMono
    {
        #region 启动器选择

        [SerializeField]
        [LabelText("是否显示启动器选项的全名")]
        private bool _isShowFullName;

        /// <summary>选中的启动器类型名称</summary>
        [SerializeField]
        [Sirenix.OdinInspector.ValueDropdown(nameof(GetAllLauncherTypeNames))]
        [LabelText("选择启动器")]
        [InfoBox("选择一个启动器作为项目启动入口", InfoMessageType.Info)]
        [EnableIf(nameof(IsNotPlaying))]
        private string _selectedLauncherTypeName = "";

        /// <summary>当前运行的启动器实例（内部字段）</summary>
        private ILauncher _currentLauncherInstance;

        /// <summary>
        /// 获取所有启动器类型名称（用于下拉菜单）
        /// </summary>
        private IEnumerable<ValueDropdownItem<string>> GetAllLauncherTypeNames()
        {
            return ReflectionUtility.GetAllTypeDropdownItems<ILauncher>(_isShowFullName);
        }

        /// <summary>
        /// 检查是否不在运行状态（用于启用编辑器功能）
        /// </summary>
        private bool IsNotPlaying()
        {
            return !Application.isPlaying;
        }

        private void StartGameProject()
        {
            // 检查是否选择了启动器
            if (string.IsNullOrEmpty(_selectedLauncherTypeName))
            {
                Debug.LogWarning("未选择启动器,请在框架预制体上选择启动器");
                return;
            }

            // 根据选择的启动器类型名称创建实例
            var type = ReflectionUtility.GetTypeByName(_selectedLauncherTypeName);
            if (type != null && typeof(ILauncher).IsAssignableFrom(type))
            {
                _currentLauncherInstance = Activator.CreateInstance(type) as ILauncher;
                if (_currentLauncherInstance != null)
                {
                    _currentLauncherInstance.Launch();
                    Debug.Log($"启动器 {type.Name} 已启动");
                }
                else
                {
                    Debug.LogError($"无法创建启动器实例: {type.Name}");
                }
            }
            else
            {
                Debug.LogError($"无法找到启动器类型: {_selectedLauncherTypeName}");
            }
        }
        #endregion


    }
}

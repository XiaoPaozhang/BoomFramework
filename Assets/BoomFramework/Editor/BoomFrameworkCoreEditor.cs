using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BoomFramework.EditorTools
{
    /// <summary>
    /// BoomFrameworkCore 的自定义 Inspector
    /// </summary>
    [CustomEditor(typeof(BoomFrameworkCore))]
    public class BoomFrameworkCoreEditor : Editor
    {
        private SerializedProperty _isShowFullNameProp;
        private SerializedProperty _selectedLauncherTypeNameProp;

        private List<Type> _launcherTypes;
        private string[] _launcherDisplayNames;
        private int _selectedIndex = 0;

        private void OnEnable()
        {
            // 获取序列化属性
            _isShowFullNameProp = serializedObject.FindProperty("_isShowFullName");
            _selectedLauncherTypeNameProp = serializedObject.FindProperty("_selectedLauncherTypeName");

            // 刷新启动器列表
            RefreshLauncherList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 绘制默认的 Inspector（除了我们自定义的字段）
            DrawPropertiesExcluding(serializedObject, "_isShowFullName", "_selectedLauncherTypeName");

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("启动器配置", EditorStyles.boldLabel);

            // 绘制"显示完整名称"开关
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(_isShowFullNameProp, new GUIContent("显示完整类型名"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    RefreshLauncherList(); // 重新生成显示名称
                }

                // 绘制刷新按钮
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("启动器列表");
                if (GUILayout.Button("刷新", GUILayout.Width(60)))
                {
                    RefreshLauncherList();
                }
            }
            EditorGUILayout.EndHorizontal();

            // 绘制启动器下拉框
            if (_launcherTypes == null || _launcherTypes.Count == 0)
            {
                EditorGUILayout.HelpBox("未找到任何实现 ILauncher 接口的类。\n请创建一个实现 ILauncher 接口的类。", MessageType.Warning);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                _selectedIndex = EditorGUILayout.Popup("选择启动器", _selectedIndex, _launcherDisplayNames);
                if (EditorGUI.EndChangeCheck())
                {
                    // 更新选中的启动器类型名
                    if (_selectedIndex >= 0 && _selectedIndex < _launcherTypes.Count)
                    {
                        _selectedLauncherTypeNameProp.stringValue = _launcherTypes[_selectedIndex].AssemblyQualifiedName;
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                // 显示当前选中的启动器信息
                if (_selectedIndex >= 0 && _selectedIndex < _launcherTypes.Count)
                {
                    var selectedType = _launcherTypes[_selectedIndex];
                    EditorGUILayout.HelpBox(
                        $"类型: {selectedType.FullName}\n" +
                        $"程序集: {selectedType.Assembly.GetName().Name}",
                        MessageType.Info
                    );
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 刷新启动器列表
        /// </summary>
        private void RefreshLauncherList()
        {
            // 使用反射获取所有实现 ILauncher 接口的类型
            _launcherTypes = ReflectionUtility.GetAllTypes<ILauncher>().ToList();

            // 生成显示名称
            bool showFullName = _isShowFullNameProp?.boolValue ?? false;
            _launcherDisplayNames = _launcherTypes
                .Select(t => showFullName ? t.FullName : t.Name)
                .ToArray();

            // 查找当前选中的索引
            string currentTypeName = _selectedLauncherTypeNameProp?.stringValue;
            if (!string.IsNullOrEmpty(currentTypeName))
            {
                _selectedIndex = _launcherTypes.FindIndex(t => t.AssemblyQualifiedName == currentTypeName);
                if (_selectedIndex < 0)
                {
                    _selectedIndex = 0;
                }
            }
            else
            {
                _selectedIndex = 0;
            }

            // 如果有启动器但没有选中任何一个，默认选中第一个
            if (_launcherTypes.Count > 0 && string.IsNullOrEmpty(currentTypeName))
            {
                _selectedLauncherTypeNameProp.stringValue = _launcherTypes[0].AssemblyQualifiedName;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}


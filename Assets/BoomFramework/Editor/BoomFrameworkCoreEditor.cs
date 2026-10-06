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
        private BoomFrameworkCore _target;
        private SerializedProperty _launcherProp;

        private List<Type> _launcherTypes;
        private string[] _launcherDisplayNames;

        private void OnEnable()
        {
            _target = (BoomFrameworkCore)target;
            // 获取序列化属性
            _launcherProp = serializedObject.FindProperty("_launcher");

            // 刷新启动器列表
            RefreshLauncherList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 绘制默认的 Inspector（除了我们自定义的字段）
            DrawPropertiesExcluding(serializedObject, "_launcher");

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("启动器配置", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("启动器列表");
            if (GUILayout.Button("刷新", GUILayout.Width(60)))
            {
                RefreshLauncherList();
            }
            EditorGUILayout.EndHorizontal();

            if (_launcherTypes.Count == 0)
            {
                EditorGUILayout.HelpBox("未找到可用的启动器。\n请创建一个继承 LauncherBase 的非抽象类。", MessageType.Warning);
            }

            // 下拉选项由实际组件引用推导，不再保存独立的类型名或选择索引。
            var launcher = _launcherProp.objectReferenceValue as LauncherBase;
            int selectedIndex = launcher != null
                ? _launcherTypes.IndexOf(launcher.GetType()) + 1
                : 0;
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("选择启动器", selectedIndex, _launcherDisplayNames);
            if (EditorGUI.EndChangeCheck())
            {
                SwitchLauncher(newIndex == 0 ? null : _launcherTypes[newIndex - 1]);
                launcher = _launcherProp.objectReferenceValue as LauncherBase;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_launcherProp, new GUIContent("启动器组件"));
            }

            if (launcher != null)
            {
                var launcherType = launcher.GetType();
                EditorGUILayout.HelpBox(
                    $"类型: {launcherType.FullName}\n" +
                    $"程序集: {launcherType.Assembly.GetName().Name}",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox("尚未配置启动器，请从下拉列表选择。", MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void SwitchLauncher(Type launcherType)
        {
            serializedObject.ApplyModifiedProperties();
            foreach (var launcher in _target.GetComponents<LauncherBase>())
            {
                DestroyImmediate(launcher);
            }

            var launcherInstance = launcherType != null
                ? _target.gameObject.AddComponent(launcherType) as LauncherBase
                : null;
            serializedObject.Update();
            _launcherProp.objectReferenceValue = launcherInstance;
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 刷新启动器列表
        /// </summary>
        private void RefreshLauncherList()
        {
            // 只列出可挂载的 LauncherBase 派生组件。
            _launcherTypes = ReflectionUtility.GetAllTypes<LauncherBase>()
                .Where(t => !t.ContainsGenericParameters)
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .ToList();

            // 生成显示名称
            _launcherDisplayNames = _launcherTypes
                .Select(t => t.Name)
                .Prepend("未选择")
                .ToArray();
        }
    }
}


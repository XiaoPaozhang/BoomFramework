using UnityEditor;
using UnityEngine;

namespace BoomFramework.EditorTools
{
    [CustomEditor(typeof(AssetLoadMono))]
    public class AssetLoadMonoEditor : Editor
    {
        private SerializedProperty _providerTypeProp;
        private SerializedProperty _defaultRootPathProp;
        private SerializedProperty _uiSubPathProp;

        private FolderSelector _defaultRootPathSelector;

        private void OnEnable()
        {
            _providerTypeProp = serializedObject.FindProperty("_providerType");
            _defaultRootPathProp = serializedObject.FindProperty("_defaultRootPath");
            _uiSubPathProp = serializedObject.FindProperty("_uiSubPath");

            // 默认根路径选择器
            _defaultRootPathSelector = new FolderSelector(
                prefsKey: "AssetLoadMono.DefaultRootPath",
                defaultPath: _defaultRootPathProp.stringValue,
                dragAreaLabel: "拖拽资源根目录到这里",
                dragAreaHeight: 50f,
                onPathChanged: (newPath) =>
                {
                    _defaultRootPathProp.stringValue = newPath;
                    serializedObject.ApplyModifiedProperties();
                }
            );
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 绘制脚本字段（只读）
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoBehaviour), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10);

            // 资源加载配置
            EditorGUILayout.LabelField("资源加载配置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_providerTypeProp, new GUIContent("资源加载模式"));

            EditorGUILayout.Space(10);

            // 统一路径配置
            DrawUnifiedPathConfig();

            EditorGUILayout.Space(10);

            // UI 子路径配置
            var providerType = (AssetLoadModeType)_providerTypeProp.enumValueIndex;
            DrawUISubPathConfig(providerType);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawUnifiedPathConfig()
        {
            EditorGUILayout.LabelField("统一路径配置", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "默认资源根路径（必须以 Assets/ 开头）\n" +
                "• Editor 模式：直接使用此路径\n" +
                "• AssetBundle 模式：去掉此路径前缀后使用",
                MessageType.Info);
            _defaultRootPathSelector.DrawGUI();
        }

        private void DrawUISubPathConfig(AssetLoadModeType providerType)
        {
            EditorGUILayout.LabelField("UI 预制体子路径", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_uiSubPathProp, new GUIContent("UI 子路径"));

            // 显示不同模式下的完整路径
            string editorUIPath = System.IO.Path.Combine(_defaultRootPathProp.stringValue, _uiSubPathProp.stringValue).Replace('\\', '/');
            string abUIPath = _uiSubPathProp.stringValue;

            EditorGUILayout.HelpBox(
                $"Editor 模式 UI 路径：{editorUIPath}\n" +
                $"AssetBundle 模式 UI 路径：{abUIPath}",
                MessageType.Info);
        }
    }
}


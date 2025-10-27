using System;
using UnityEditor;
using UnityEngine;

namespace BoomFramework.EditorTools
{
    /// <summary>
    /// 文件夹选择器 - 可复用的 Editor GUI 组件
    /// 支持拖拽、点击选择文件夹，并自动保存到 EditorPrefs
    /// </summary>
    public class FolderSelector
    {
        private string _currentPath;
        private readonly string _prefsKey;
        private readonly string _dragAreaLabel;
        private readonly float _dragAreaHeight;
        private readonly Action<string> _onPathChanged;

        /// <summary>
        /// 当前选中的文件夹路径（相对路径，如 "Assets/..."）
        /// </summary>
        public string CurrentPath
        {
            get => _currentPath;
            set
            {
                if (_currentPath != value)
                {
                    _currentPath = value;
                    SaveToPrefs();
                    _onPathChanged?.Invoke(_currentPath);
                }
            }
        }

        /// <summary>
        /// 创建文件夹选择器
        /// </summary>
        /// <param name="prefsKey">EditorPrefs 保存键名</param>
        /// <param name="defaultPath">默认路径</param>
        /// <param name="dragAreaLabel">拖拽区域显示文本</param>
        /// <param name="dragAreaHeight">拖拽区域高度</param>
        /// <param name="onPathChanged">路径改变时的回调</param>
        public FolderSelector(
            string prefsKey,
            string defaultPath = "Assets",
            string dragAreaLabel = "点击选择或拖拽文件夹到这里",
            float dragAreaHeight = 60f,
            Action<string> onPathChanged = null)
        {
            _prefsKey = prefsKey;
            _dragAreaLabel = dragAreaLabel;
            _dragAreaHeight = dragAreaHeight;
            _onPathChanged = onPathChanged;

            // 从 EditorPrefs 加载保存的路径
            string savedPath = EditorPrefs.GetString(_prefsKey, defaultPath);
            _currentPath = ConvertToRelativePath(savedPath);
        }

        /// <summary>
        /// 绘制文件夹选择器 GUI（包含拖拽区域和当前路径显示）
        /// </summary>
        public void DrawGUI()
        {
            DrawDragArea();
            DrawCurrentPathDisplay();
        }

        /// <summary>
        /// 仅绘制拖拽区域
        /// </summary>
        public void DrawDragArea()
        {
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, _dragAreaHeight, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, _dragAreaLabel, EditorStyles.helpBox);

            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (dropArea.Contains(evt.mousePosition))
                    {
                        HandleDirectorySelection();
                        evt.Use();
                    }
                    break;

                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (var draggedObject in DragAndDrop.objectReferences)
                        {
                            string path = AssetDatabase.GetAssetPath(draggedObject);
                            if (AssetDatabase.IsValidFolder(path))
                            {
                                UpdateSelectedPath(path);
                                break;
                            }
                        }
                    }
                    evt.Use();
                    break;
            }
        }

        /// <summary>
        /// 仅绘制当前路径显示
        /// </summary>
        public void DrawCurrentPathDisplay()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("当前目标目录:", _currentPath);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制为 PropertyField 样式（适合在 Custom Editor 中使用）
        /// </summary>
        /// <param name="label">显示标签</param>
        public void DrawAsPropertyField(string label = "目标文件夹")
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(label);

                // 显示当前路径（只读）
                EditorGUI.BeginDisabledGroup(true);
                {
                    EditorGUILayout.TextField(_currentPath);
                }
                EditorGUI.EndDisabledGroup();

                // 选择按钮
                if (GUILayout.Button("选择", GUILayout.Width(60)))
                {
                    HandleDirectorySelection();
                }
            }
            EditorGUILayout.EndHorizontal();

            // 拖拽区域
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 30f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "拖拽文件夹到这里", EditorStyles.helpBox);

            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                if (dropArea.Contains(evt.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (var draggedObject in DragAndDrop.objectReferences)
                        {
                            string path = AssetDatabase.GetAssetPath(draggedObject);
                            if (AssetDatabase.IsValidFolder(path))
                            {
                                UpdateSelectedPath(path);
                                break;
                            }
                        }
                    }
                    evt.Use();
                }
            }
        }

        /// <summary>
        /// 处理目录选择（点击时触发）
        /// </summary>
        private void HandleDirectorySelection()
        {
            string newPath = "";

            // 先检查当前选中的对象是否是文件夹
            if (Selection.activeObject != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    newPath = assetPath;
                }
            }

            // 如果没有选中文件夹，打开文件夹选择对话框
            if (string.IsNullOrEmpty(newPath))
            {
                newPath = EditorUtility.OpenFolderPanel(
                    "选择目标文件夹",
                    AbsolutePathFromRelative(_currentPath),
                    ""
                );
            }

            if (!string.IsNullOrEmpty(newPath))
            {
                UpdateSelectedPath(newPath);
            }
        }

        /// <summary>
        /// 更新选中的路径
        /// </summary>
        private void UpdateSelectedPath(string newPath)
        {
            if (newPath.StartsWith(Application.dataPath) || newPath.StartsWith("Assets"))
            {
                CurrentPath = ConvertToRelativePath(newPath);
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "请选择工程内的目录（Assets 下）", "确定");
            }
        }

        /// <summary>
        /// 保存路径到 EditorPrefs
        /// </summary>
        private void SaveToPrefs()
        {
            if (!string.IsNullOrEmpty(_prefsKey))
            {
                EditorPrefs.SetString(_prefsKey, _currentPath);
            }
        }

        /// <summary>
        /// 将绝对路径或相对路径转换为 Unity 相对路径（Assets/...）
        /// </summary>
        private static string ConvertToRelativePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "Assets";

            path = path.Replace("\\", "/");

            if (path.StartsWith("Assets"))
                return path;

            if (path.StartsWith(Application.dataPath))
            {
                return "Assets" + path.Substring(Application.dataPath.Length);
            }

            return "Assets";
        }

        /// <summary>
        /// 将相对路径转换为绝对路径
        /// </summary>
        private static string AbsolutePathFromRelative(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return Application.dataPath;

            if (relativePath.StartsWith("Assets"))
            {
                return Application.dataPath + relativePath.Substring("Assets".Length);
            }

            return relativePath;
        }

        /// <summary>
        /// 验证当前路径是否有效
        /// </summary>
        public bool IsValid()
        {
            return AssetDatabase.IsValidFolder(_currentPath);
        }

        /// <summary>
        /// 重置为默认路径
        /// </summary>
        public void Reset(string defaultPath = "Assets")
        {
            CurrentPath = defaultPath;
        }
    }
}


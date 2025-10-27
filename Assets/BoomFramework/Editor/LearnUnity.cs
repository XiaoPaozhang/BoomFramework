using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using BoomFramework.EditorTools;

namespace XpzUtility
{
    public class LearnUnityTemplateGenerate : EditorWindow
    {
        private const string PrefsKey = "XpzUtility.LastSelectedFolder";
        private static string winTitle = "学习unity的模板生成";
        private string templateName;
        private Vector2 scrollPosition;
        private FolderSelector _folderSelector;

        private string FormatPath => ToUnityPath(Path.Combine(_folderSelector.CurrentPath, templateName));

        [MenuItem("BoomFramework/学习unity的模板生成 %#_z")]
        private static void ShowWindow()
        {
            // 创建并打开窗口
            var win = GetWindow<LearnUnityTemplateGenerate>(winTitle);
            win.minSize = new Vector2(600, 300);
            win.Center();

        }

        private void OnEnable()
        {
            // 初始化文件夹选择器
            _folderSelector = new FolderSelector(
                prefsKey: PrefsKey,
                defaultPath: "Assets",
                dragAreaLabel: "点击选择或拖拽文件夹到这里",
                dragAreaHeight: 60f
            );
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 使用 FolderSelector 绘制文件夹选择器
            _folderSelector.DrawGUI();

            templateName = EditorGUILayout.TextField("模板名称:", templateName);

            if (GUILayout.Button("生成模板", GUILayout.Height(30)))
            {
                if (ValidateInput())
                {
                    GenerateTemplate();
                }
            }

            if (EditorGUILayout.LinkButton($"清除目录{_folderSelector.CurrentPath}"))
            {
                bool confirm = EditorUtility.DisplayDialog(
                    "警告",
                    $"确定要删除{_folderSelector.CurrentPath}吗？",
                    "确认",
                    "取消"
                );

                if (confirm) ClearDirectory();
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 使用AssetDatabase.DeleteAsset来删除目录，确保删除meta文件等
        /// </summary>
        private void ClearDirectory()
        {
            // 使用相对路径，直接操作AssetDatabase
            if (AssetDatabase.IsValidFolder(_folderSelector.CurrentPath))
            {
                bool isDeleted = AssetDatabase.DeleteAsset(_folderSelector.CurrentPath);
                if (isDeleted)
                {
                    AssetDatabase.Refresh();
                    Debug.Log($"已删除文件夹: {_folderSelector.CurrentPath}");
                }
                else
                {
                    Debug.LogError($"删除文件夹失败: {_folderSelector.CurrentPath}");
                }
            }
            else
            {
                Debug.LogError("目录不存在或路径错误");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(templateName))
            {
                EditorUtility.DisplayDialog("错误", "请输入模板名称", "确定");
                return false;
            }

            if (!AssetDatabase.IsValidFolder(_folderSelector.CurrentPath))
            {
                EditorUtility.DisplayDialog("错误", "目标目录不存在或无效，请重新选择", "确定");
                return false;
            }

            return true;
        }
        private void GenerateTemplate()
        {
            try
            {
                // 检查目标目录（FormatPath）是否存在，FormatPath为 "Assets/TemplateName"
                if (!AssetDatabase.IsValidFolder(FormatPath))
                {
                    // 使用AssetDatabase.CreateFolder创建文件夹
                    string parentFolder = _folderSelector.CurrentPath; // 比如 "Assets/Xpznl"
                    string newFolderName = templateName;        // 模板名称作为新文件夹名称
                    AssetDatabase.CreateFolder(parentFolder, newFolderName);

                    // 创建场景并保存到新建的文件夹中
                    Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
                    string scenePath = ToUnityPath(Path.Combine(FormatPath, $"{templateName}Scene.unity"));
                    // 创建Main对象并延迟挂载脚本
                    var main = new GameObject("Main");
                    EditorSceneManager.SaveScene(newScene, scenePath);

                    // 创建脚本文件（File IO操作）
                    CreateScriptFile(FormatPath);
                    AssetDatabase.Refresh();

                    // 将脚本挂载到Main对象上（若未编译完成则在编译后自动挂载）
                    string componentFullName = $"BoomFramework.{templateName}Test, Assembly-CSharp";
                    TryAttachOrSchedule(main, scenePath, componentFullName);
                }
                else
                {
                    Debug.LogWarning("模板目录已存在！");
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"生成失败: {e.Message}", "确定");
                Debug.LogError(e);
            }
        }

        private void CreateScriptFile(string rootPath)
        {
            string scriptContent = $@"
using UnityEngine;

namespace BoomFramework
{{
    public class {templateName}Test : MonoBehaviour
    {{
        void Start()
        {{
        
        }}

        void Update()
        {{
        
        }}
    }}
}}

";
            string scriptPath = ToUnityPath(Path.Combine(rootPath, $"{templateName}Test.cs"));
            File.WriteAllText(scriptPath, scriptContent);

            AssetDatabase.Refresh();
        }

        // 窗口居中方法
        private void Center()
        {
            var main = EditorGUIUtility.GetMainWindowPosition();
            position = new Rect(
                main.x + (main.width - position.width) * 0.5f,
                main.y + (main.height - position.height) * 0.5f,
                position.width,
                position.height
            );
        }

        private static string ToUnityPath(string path)
        {
            return string.IsNullOrEmpty(path) ? path : path.Replace("\\", "/");
        }

        private static void TryAttachOrSchedule(GameObject target, string scenePath, string componentFullName)
        {
            if (target == null) return;

            var scriptType = Type.GetType(componentFullName);
            if (scriptType != null)
            {
                target.AddComponent(scriptType);
                EditorSceneManager.MarkAllScenesDirty();
                EditorSceneManager.SaveOpenScenes();
                return;
            }

            // 若类型尚不可用（脚本仍在编译），记录待办信息，编译完成后自动挂载
            string pending = string.Join("|", new[] { scenePath, target.name, componentFullName });
            EditorPrefs.SetString(LearnUnityPostCompileAttacher.PendingKey, pending);
            Debug.Log("脚本正在编译，编译完成后会自动挂载到 Main。");
        }
    }

    [InitializeOnLoad]
    public static class LearnUnityPostCompileAttacher
    {
        public const string PendingKey = "XpzUtility.PendingAttach";

        static LearnUnityPostCompileAttacher()
        {
            EditorApplication.delayCall += TryAttachPending;
        }

        private static void TryAttachPending()
        {
            if (EditorApplication.isCompiling)
            {
                EditorApplication.delayCall += TryAttachPending;
                return;
            }
            if (!EditorPrefs.HasKey(PendingKey)) return;

            string data = EditorPrefs.GetString(PendingKey, string.Empty);
            if (string.IsNullOrEmpty(data)) return;

            string[] parts = data.Split('|');
            if (parts.Length < 3)
            {
                EditorPrefs.DeleteKey(PendingKey);
                return;
            }

            string scenePath = parts[0].Replace("\\", "/");
            string gameObjectName = parts[1];
            string componentFullTypeName = parts[2];

            try
            {
                // 确保目标场景已加载（若未加载则以叠加方式打开）
                var scene = EditorSceneManager.GetSceneByPath(scenePath);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                }

                // 在目标场景中查找对象
                GameObject go = null;
                var roots = scene.GetRootGameObjects();
                for (int i = 0; i < roots.Length; i++)
                {
                    if (roots[i].name == gameObjectName)
                    {
                        go = roots[i];
                        break;
                    }
                }

                var type = Type.GetType(componentFullTypeName);
                if (type == null)
                {
                    // 类型尚不可用，延迟再次尝试
                    EditorApplication.delayCall += TryAttachPending;
                    return;
                }

                if (go != null)
                {
                    go.AddComponent(type);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"已自动将脚本 {componentFullTypeName} 挂载到 {gameObjectName}");
                    EditorPrefs.DeleteKey(PendingKey);
                }
                else
                {
                    Debug.LogWarning($"自动挂载失败，找不到对象：{gameObjectName}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"自动挂载失败：{ex.Message}");
            }
        }
    }
}

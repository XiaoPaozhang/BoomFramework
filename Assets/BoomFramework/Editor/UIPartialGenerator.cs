using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BoomFramework.EditorTools
{
    /// <summary>
    /// 生成 UI 组件绑定的分部类：ClassName.Generate.cs
    /// 规则：子节点命名为 "前缀@别名"，根据前缀映射组件类型，别名作为字段名
    /// 示例："img@pic" -> Image pic; Awake 中绑定：pic = transform.FindComponent<Image>("img@pic");
    /// </summary>
    public static class UIPartialGenerator
    {
        // 前缀到组件类型名的映射（可按需扩展）
        private static readonly Dictionary<string, string> PrefixToTypeName = new()
        {
            { "img", nameof(Image) },
            { "raw", nameof(RawImage) },
            { "btn", nameof(Button) },
            { "tog", nameof(Toggle) },
            { "sld", nameof(Slider) },
            { "scr", nameof(ScrollRect) },
            { "ipt", nameof(TMP_InputField) },
            { "tmptxt", nameof(TMP_Text) },
            { "dd", nameof(TMP_Dropdown) },
            { "rect", nameof(RectTransform) }
        };

        // 命名前缀优先级（从高到低），用于自动/轮询命名
        private sealed class PrefixRule
        {
            public string Prefix;
            public Type ComponentType;
            public int Priority;
        }

        private static readonly List<PrefixRule> PrefixRules = new()
        {
            new() { Prefix = "btn",    ComponentType = typeof(Button),          Priority = 100 },
            new() { Prefix = "ipt",    ComponentType = typeof(TMP_InputField),  Priority = 95  },
            new() { Prefix = "tmptxt", ComponentType = typeof(TMP_Text),        Priority = 90  },
            new() { Prefix = "dd",     ComponentType = typeof(TMP_Dropdown),    Priority = 85  },
            new() { Prefix = "tog",    ComponentType = typeof(Toggle),          Priority = 80  },
            new() { Prefix = "sld",    ComponentType = typeof(Slider),          Priority = 70  },
            new() { Prefix = "scr",    ComponentType = typeof(ScrollRect),      Priority = 60  },
            new() { Prefix = "img",    ComponentType = typeof(Image),           Priority = 50  },
            new() { Prefix = "raw",    ComponentType = typeof(RawImage),        Priority = 45  },
            new() { Prefix = "rect",   ComponentType = typeof(RectTransform),   Priority = 10  },
        };

        [MenuItem("BoomFramework/UI/为选中节点循环后缀 %e")]
        public static void CyclePrefixForSelected()
        {
            var selection = Selection.gameObjects;
            if (selection == null || selection.Length == 0)
            {
                EditorUtility.DisplayDialog("UIPartialGenerator", "请先选择一个或多个需要循环前缀的节点", "OK");
                return;
            }

            int changed = CyclePrefixesOnTransforms(selection.Select(g => g.transform));
            if (changed > 0) MarkDirty(selection[0]);
        }

        [MenuItem("BoomFramework/UI/移除选中节点后缀 %q")]
        public static void RemovePrefixForSelected()
        {
            var selection = Selection.gameObjects;
            if (selection == null || selection.Length == 0)
            {
                EditorUtility.DisplayDialog("UIPartialGenerator", "请先选择一个或多个需要移除前缀的节点", "OK");
                return;
            }
            int changed = RemovePrefixesOnTransforms(selection.Select(g => g.transform));
            if (changed > 0) MarkDirty(selection[0]);
        }

        private static void MarkDirty(GameObject go)
        {
            if (go == null) return;
            EditorUtility.SetDirty(go);
            if (go.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(go.scene);
            }
        }

        // 对外复用：批量循环前缀
        public static int CyclePrefixesOnTransforms(IEnumerable<Transform> transforms)
        {
            int changed = 0;
            foreach (var t in transforms)
            {
                var candidates = GetCandidatePrefixCombos(t);
                if (candidates.Count == 0) continue;
                ParseName(t.name, out var curPrefix, out _);

                string next = candidates[0];
                if (!string.IsNullOrEmpty(curPrefix))
                {
                    int idx = candidates.IndexOf(curPrefix);
                    if (idx >= 0) next = candidates[(idx + 1) % candidates.Count];
                }
                if (ApplyPrefix(t, next)) changed++;
            }
            if (changed > 0)
            {
                Debug.Log($"[UIPartialGenerator] 循环前缀完成，重命名 {changed} 个节点。");
            }
            else
            {
                Debug.LogWarning("[UIPartialGenerator] 无可用组件用于前缀循环，未进行任何修改。");
            }
            return changed;
        }

        // 对外复用：批量移除前缀
        public static int RemovePrefixesOnTransforms(IEnumerable<Transform> transforms)
        {
            int changed = 0;
            foreach (var t in transforms)
            {
                ParseName(t.name, out var curPrefix, out var alias);
                if (string.IsNullOrEmpty(curPrefix)) continue; // 无前缀
                string newName = string.IsNullOrEmpty(alias) ? t.name : alias;
                if (newName == t.name) continue;
                Undo.RecordObject(t.gameObject, "Remove prefix");
                t.name = newName;
                EditorUtility.SetDirty(t.gameObject);
                changed++;
            }
            if (changed > 0)
            {
                Debug.Log($"[UIPartialGenerator] 已移除 {changed} 个节点的前缀。");
            }
            else
            {
                Debug.LogWarning("[UIPartialGenerator] 选中项均无前缀可移除。");
            }
            return changed;
        }

        private static bool TryGetBestPrefix(Transform t, out string prefix)
        {
            foreach (var rule in PrefixRules.OrderByDescending(r => r.Priority))
            {
                // RectTransform 是所有 UI 节点必有，作为低优先级备选
                if (rule.ComponentType == typeof(RectTransform))
                {
                    if (t is RectTransform) { prefix = rule.Prefix; return true; }
                    continue;
                }

                if (t.GetComponent(rule.ComponentType) != null)
                {
                    prefix = rule.Prefix;
                    return true;
                }
            }
            prefix = null;
            return false;
        }

        public static List<string> GetCandidatePrefixes(Transform t)
        {
            var result = new List<string>();
            foreach (var rule in PrefixRules.OrderByDescending(r => r.Priority))
            {
                if (rule.ComponentType == typeof(RectTransform))
                {
                    if (t is RectTransform) result.Add(rule.Prefix);
                }
                else if (t.GetComponent(rule.ComponentType) != null)
                {
                    result.Add(rule.Prefix);
                }
            }
            return result;
        }

        // 生成候选的前缀组合（单组件 + 多组件的所有非空组合），按优先级从高到低排序
        public static List<string> GetCandidatePrefixCombos(Transform t)
        {
            var single = GetCandidatePrefixes(t);
            if (single.Count == 0) return single;
            // 优先级降序
            single = single.OrderByDescending(GetPriority).ToList();

            var combos = new List<string>();
            int n = single.Count;
            for (int mask = 1; mask < (1 << n); mask++)
            {
                var parts = new List<string>();
                for (int i = 0; i < n; i++)
                {
                    if (((mask >> i) & 1) == 1)
                    {
                        parts.Add(single[i]);
                    }
                }
                combos.Add(string.Join("_", parts));
            }
            // 排序：先长度升序，再按优先级串降序
            combos = combos
                .Select(s => new { s, parts = s.Split('_') })
                .OrderBy(x => x.parts.Length)
                .ThenByDescending(x => string.Join(",", x.parts.Select(p => GetPriority(p).ToString("D4"))))
                .Select(x => x.s)
                .ToList();
            return combos;
        }

        public static bool ApplyPrefix(Transform t, string newPrefix)
        {
            if (string.IsNullOrEmpty(newPrefix)) return false;
            string name = t.name;
            ParseName(name, out var curSuffixCombo, out var alias);
            if (string.IsNullOrEmpty(alias))
            {
                alias = SanitizeIdentifier(name.Contains('@') ? name.Substring(0, name.IndexOf('@')) : name);
            }
            string newName = $"{alias}@{newPrefix}";
            if (newName == name) return false;
            Undo.RecordObject(t.gameObject, "Rename with prefix");
            t.name = newName;
            EditorUtility.SetDirty(t.gameObject);
            return true;
        }

        public static void ParseName(string name, out string suffixCombo, out string alias)
        {
            suffixCombo = null; alias = null;
            if (string.IsNullOrEmpty(name)) return;
            int at = name.IndexOf('@');
            if (at > 0 && at < name.Length - 1)
            {
                alias = name.Substring(0, at);
                suffixCombo = name.Substring(at + 1);
            }
        }

        [MenuItem("CONTEXT/UIBase/生成组件绑定脚本文件")]
        private static void ContextGenerateForUIBase(MenuCommand command)
        {
            var comp = command.context as MonoBehaviour;
            if (comp == null) return;
            GenerateForComponent(comp);
        }

        [MenuItem("CONTEXT/UIBase/自动绑定字段（将子节点组件赋值到字段）")]
        private static void ContextBindForUIBase(MenuCommand command)
        {
            var comp = command.context as MonoBehaviour;
            if (comp == null) return;
            BindFieldsToComponents(comp);
        }


        public static void GenerateForGameObject(GameObject go)
        {
            if (go == null) return;
            var targetMb = PickTargetScript(go);
            if (targetMb == null)
            {
                EditorUtility.DisplayDialog("UIPartialGenerator", "选中的对象上未找到可用于生成的脚本组件（优先 UIBase 或 Assets 下的脚本）。", "OK");
                return;
            }
            GenerateForComponent(targetMb);
        }

        private static void GenerateForComponent(MonoBehaviour targetMb)
        {
            var go = targetMb.gameObject;
            Type uiType = targetMb.GetType();
            string className = uiType.Name;
            string @namespace = string.IsNullOrWhiteSpace(uiType.Namespace) ? null : uiType.Namespace;

            // 通过脚本资产定位主脚本目录（避免写入 Packages）
            var monoScript = MonoScript.FromMonoBehaviour(targetMb);
            string scriptPath = AssetDatabase.GetAssetPath(monoScript);
            string scriptDir = Path.GetDirectoryName(scriptPath).Replace('\\', '/');
            if (!scriptDir.StartsWith("Assets/", StringComparison.Ordinal))
            {
                EditorUtility.DisplayDialog("UIPartialGenerator", $"目标脚本位于不可写目录：{scriptDir}\n请将脚本放在 Assets 下后再生成。", "OK");
                return;
            }
            string genPath = Path.Combine(scriptDir, className + ".Generate.cs").Replace('\\', '/');

            var entries = CollectEntries(go.transform);
            if (entries.Count == 0)
            {
                if (!EditorUtility.DisplayDialog("UIPartialGenerator", "未发现符合“前缀@别名”命名规则的子节点，仍然生成空的分部类吗？", "生成", "取消"))
                    return;
            }

            string code = BuildCode(@namespace, className, entries);
            File.WriteAllText(genPath, code, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            AssetDatabase.ImportAsset(genPath);
            Debug.Log($"[UIPartialGenerator] 生成成功 → {genPath}");

            if (go.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(go.scene);
            }
        }

        private static MonoBehaviour PickTargetScript(GameObject go)
        {
            // 1) 优先 UIBase 派生
            var uiBase = go.GetComponent<UIBase>();
            if (uiBase != null) return uiBase as MonoBehaviour;
            // 2) 其次选择脚本文件位于 Assets/ 下的 MonoBehaviour
            var mbs = go.GetComponents<MonoBehaviour>();
            foreach (var mb in mbs)
            {
                if (mb == null) continue;
                var ms = MonoScript.FromMonoBehaviour(mb);
                if (ms == null) continue;
                var path = AssetDatabase.GetAssetPath(ms);
                if (!string.IsNullOrEmpty(path) && path.Replace('\\', '/').StartsWith("Assets/", StringComparison.Ordinal))
                {
                    return mb;
                }
            }
            // 3) 兜底返回第一个非空脚本
            return mbs.FirstOrDefault(m => m != null);
        }

        // 根据命名规则为指定脚本上的同名字段赋值
        public static void BindFieldsToComponents(MonoBehaviour targetMb)
        {
            if (targetMb == null)
            {
                Debug.LogWarning("[UIPartialGenerator] 绑定失败：目标脚本为空。");
                return;
            }

            var go = targetMb.gameObject;
            var entries = CollectEntries(go.transform);
            if (entries.Count == 0)
            {
                Debug.LogWarning("[UIPartialGenerator] 未找到符合命名规则的子节点，跳过绑定。");
                return;
            }

            var so = new SerializedObject(targetMb);
            int bindCount = 0;
            foreach (var e in entries)
            {
                var comp = FindComponentByNameAndType(go.transform, e.FullNodeName, e.ComponentType);
                if (comp == null) continue;
                var sp = so.FindProperty(e.FieldName);
                if (sp == null)
                {
                    Debug.LogWarning($"[UIPartialGenerator] 字段不存在或未编译：{e.FieldName}，已跳过。");
                    continue;
                }
                sp.objectReferenceValue = comp;
                bindCount++;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(targetMb);
            if (go.scene.IsValid()) EditorSceneManager.MarkSceneDirty(go.scene);
            Debug.Log($"[UIPartialGenerator] 自动绑定完成，成功绑定 {bindCount} 个字段。");
        }

        // 旧入口：针对选中根，挑选脚本后再绑定
        [MenuItem("BoomFramework/UI/自动绑定字段（选中根节点）")]
        public static void BindSelected()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                EditorUtility.DisplayDialog("UIPartialGenerator", "请先在 Hierarchy 或 Project 中选中一个带脚本的根物体（Prefab 或场景实例）", "OK");
                return;
            }
            var target = PickTargetScript(go);
            if (target == null)
            {
                EditorUtility.DisplayDialog("UIPartialGenerator", "选中的对象上未找到可用于绑定的脚本组件（优先 UIBase 或 Assets 下的脚本）。", "OK");
                return;
            }
            BindFieldsToComponents(target);
        }

        private static UnityEngine.Object FindComponentByNameAndType(Transform root, string targetName, string typeName)
        {
            var type = ResolveTypeByName(typeName);
            if (type == null) return null;
            var q = new Queue<Transform>();
            q.Enqueue(root);
            while (q.Count > 0)
            {
                var t = q.Dequeue();
                if (t.name == targetName)
                {
                    return t.GetComponent(type);
                }
                for (int i = 0; i < t.childCount; i++) q.Enqueue(t.GetChild(i));
            }
            return null;
        }

        private static Type ResolveTypeByName(string typeName)
        {
            // 常见程序集优先
            var known = new[] { typeof(Image).Assembly, typeof(TMP_Text).Assembly, typeof(Transform).Assembly };
            foreach (var asm in known)
            {
                var t = asm.GetTypes().FirstOrDefault(x => x.Name == typeName);
                if (t != null) return t;
            }
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = null;
                try { t = asm.GetTypes().FirstOrDefault(x => x.Name == typeName); }
                catch { }
                if (t != null) return t;
            }
            return null;
        }


        private sealed class BindEntry
        {
            public string FullNodeName;   // 如 "img@pic"
            public string FieldName;      // 如 "pic"
            public string ComponentType;  // 如 "Image" 或 "TMP_Text"
        }

        private static List<BindEntry> CollectEntries(Transform root)
        {
            var list = new List<BindEntry>();
            var queue = new Queue<Transform>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var t = queue.Dequeue();
                for (int i = 0; i < t.childCount; i++)
                {
                    var c = t.GetChild(i);
                    queue.Enqueue(c);
                }

                var name = t.name;
                int at = name.IndexOf('@');
                if (at <= 0 || at == name.Length - 1)
                    continue; // 无名称或无后缀

                string alias = name.Substring(0, at);
                string suffixPart = name.Substring(at + 1);
                string sanitizedAlias = SanitizeIdentifier(alias);
                if (string.IsNullOrEmpty(sanitizedAlias)) continue;

                // 解析多后缀：suffix1_suffix2_...
                var presentPrefixes = suffixPart.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(p => PrefixToTypeName.ContainsKey(p))
                    .Distinct()
                    .ToList();
                if (presentPrefixes.Count == 0) continue;

                // 按优先级排序
                presentPrefixes.Sort((a, b) => GetPriority(b).CompareTo(GetPriority(a)));

                // 为每个前缀生成一个条目。全部采用 别名_前缀 的字段命名
                for (int i = 0; i < presentPrefixes.Count; i++)
                {
                    string px = presentPrefixes[i];
                    string typeName = PrefixToTypeName[px];
                    string fieldName = SanitizeIdentifier(sanitizedAlias + "_" + px);

                    list.Add(new BindEntry
                    {
                        FullNodeName = name,
                        FieldName = fieldName,
                        ComponentType = typeName
                    });
                }
            }

            // 同名冲突处理：追加序号
            var groups = list.GroupBy(e => e.FieldName);
            foreach (var g in groups)
            {
                int idx = 0;
                foreach (var e in g)
                {
                    if (idx > 0) e.FieldName = e.FieldName + idx;
                    idx++;
                }
            }

            return list;
        }

        private static string BuildCode(string @namespace, string className, List<BindEntry> entries)
        {
            bool useUI = entries.Any(e => e.ComponentType is nameof(Image) or nameof(RawImage) or nameof(Button) or nameof(Toggle) or nameof(Slider) or nameof(ScrollRect));
            bool useTMP = entries.Any(e => e.ComponentType.StartsWith("TMP_", StringComparison.Ordinal));

            var sb = new StringBuilder(1024);
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// 由 BoomFramework.UIPartialGenerator 自动生成，请勿手动修改");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            if (useTMP) sb.AppendLine("using TMPro;");
            sb.AppendLine("using UnityEngine;");
            if (useUI) sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(@namespace))
            {
                sb.AppendLine($"namespace {@namespace}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"    public partial class {className}");
            sb.AppendLine("    {");

            // 字段声明
            foreach (var e in entries)
            {
                sb.AppendLine($"        public {e.ComponentType} {e.FieldName};");
            }

            // Awake 赋值
            sb.AppendLine("        private void Awake()");
            sb.AppendLine("        {");
            foreach (var e in entries)
            {
                sb.AppendLine($"            {e.FieldName} = transform.FindComponent<{e.ComponentType}>(\"{e.FullNodeName}\");");
            }
            sb.AppendLine("        }");

            sb.AppendLine("    }");

            if (!string.IsNullOrEmpty(@namespace))
            {
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        private static string SanitizeIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            var sb = new StringBuilder(name.Length);
            // 首字符：若非法，前置下划线
            char first = name[0];
            if (char.IsLetter(first) || first == '_') sb.Append(first);
            else sb.Append('_');

            for (int i = 1; i < name.Length; i++)
            {
                char ch = name[i];
                if (char.IsLetterOrDigit(ch) || ch == '_') sb.Append(ch);
            }
            return sb.ToString();
        }

        private static int GetPriority(string prefix)
        {
            var rule = PrefixRules.FirstOrDefault(r => r.Prefix == prefix);
            return rule != null ? rule.Priority : 0;
        }
    }
}



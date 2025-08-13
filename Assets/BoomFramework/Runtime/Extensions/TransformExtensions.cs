using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    public static class TransformExtensions
    {
        /// <summary>
        /// 根据名字获取子节点组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="root">根节点</param>
        /// <param name="name">子节点名字</param>
        /// <param name="includeSelf">是否包含自身</param>
        /// <param name="includeInactive">是否包含非激活的子节点</param>
        /// <returns>匹配的组件</returns>
        public static T FindComponent<T>(this Transform root, string name, bool includeSelf = true, bool includeInactive = false) where T : Component
        {
            // 队列存储所有子节点
            var q = new Queue<Transform>();

            if (includeSelf)
                q.Enqueue(root);
            else
                for (int i = 0; i < root.childCount; i++)
                    q.Enqueue(root.GetChild(i));

            while (q.Count > 0)
            {
                var t = q.Dequeue();
                // 如果没激活就跳过
                if (!includeInactive && !t.gameObject.activeInHierarchy) continue;
                // 如果名字匹配就返回组件
                if (t.name == name)
                {
                    T comp = t.GetComponent<T>();
                    if (comp != null)
                        return comp;
                }
                // 否则将子节点加入队列
                for (int i = 0; i < t.childCount; i++)
                    q.Enqueue(t.GetChild(i));
            }

            Debug.LogError($"未找到名为 '{name}' 的子对象, 根节点: '{root.name}'");
            return null;
        }
    }
}

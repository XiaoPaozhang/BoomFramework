using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// Reflection 工具类
    /// </summary>
    public static class ReflectionUtility
    {
        // 使用反射获取所有继承泛型T的类
        public static IEnumerable<Type> GetAllTypes<T>() where T : class
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
        }

        // 通过类型全名查找 Type
        public static Type GetTypeByName(string typeName)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.AssemblyQualifiedName == typeName);
        }

        // 获取所有实现接口T的非抽象类型的名称列表
        public static IEnumerable<string> GetAllTypeNames<T>(bool isFullName = false) where T : class
        {
            return GetAllTypes<T>()
                    .Select(t => isFullName ? t.FullName : t.Name);
        }
    }
}

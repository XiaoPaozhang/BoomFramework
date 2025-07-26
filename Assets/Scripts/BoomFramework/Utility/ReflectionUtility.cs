using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BoomFramework
{
    /// <summary>
    /// Reflection 工具类
    /// </summary>
    public static class ReflectionUtility
    {
        // 使用反射获取所有继承泛型T的类
        public static List<Type> GetAllTypes<T>() where T : class
        {
            var managers = Assembly.GetExecutingAssembly()
                                              .GetTypes()
                                              .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract)
                                              .ToList();
            return managers.ToList();
        }

        // 获取所有实现接口T的非抽象类型的 ValueDropdownItem（用于 Odin Inspector）
        public static IEnumerable<ValueDropdownItem<string>> GetAllTypeDropdownItems<T>(bool isFullName = false)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
              .SelectMany(a => a.GetTypes())
              .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
              .Select(t => new ValueDropdownItem<string>(isFullName ? t.FullName : t.Name, t.AssemblyQualifiedName));
        }

        // 通过类型全名查找 Type
        public static Type GetTypeByName(string typeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
              .SelectMany(a => a.GetTypes())
              .FirstOrDefault(t => t.AssemblyQualifiedName == typeName);
        }
    }
}

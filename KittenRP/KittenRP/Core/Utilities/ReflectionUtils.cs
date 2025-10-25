using System.Reflection;

namespace KittenRP.Core.Utilities;

public static class ReflectionUtils {
  /// <summary>
  /// 调用静态方法
  /// </summary>
  /// <param name="type">类型</param>
  /// <param name="methodName">方法名</param>
  /// <param name="parameters">参数数组</param>
  /// <returns>方法执行结果</returns>
  public static object InvokeStaticMethod(Type type, string methodName, params object[] parameters) {
    if (type == null) throw new ArgumentNullException(nameof(type));
    if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));

    try {
      // 获取方法信息，考虑方法重载
      MethodInfo method = FindMethod(type, methodName, parameters);
      if (method == null) {
        throw new MethodNotFoundException($"方法 '{methodName}' 在类型 '{type.FullName}' 中未找到");
      }

      // 验证是否为静态方法
      if (!method.IsStatic) {
        throw new InvalidOperationException($"方法 '{methodName}' 不是静态方法");
      }

      return method.Invoke(null, parameters);
    } catch (TargetInvocationException ex) {
      // 抛出原始异常而不是TargetInvocationException
      throw ex.InnerException ?? ex;
    }
  }

  /// <summary>
  /// 调用实例方法
  /// </summary>
  /// <param name="instance">对象实例</param>
  /// <param name="methodName">方法名</param>
  /// <param name="parameters">参数数组</param>
  /// <returns>方法执行结果</returns>
  public static object InvokeInstanceMethod(object instance, string methodName, params object[] parameters) {
    if (instance == null) throw new ArgumentNullException(nameof(instance));
    if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));

    try {
      Type type = instance.GetType();
      MethodInfo method = FindMethod(type, methodName, parameters);
      if (method == null) {
        throw new MethodNotFoundException($"方法 '{methodName}' 在类型 '{type.FullName}' 中未找到");
      }

      return method.Invoke(instance, parameters);
    } catch (TargetInvocationException ex) {
      throw ex.InnerException ?? ex;
    }
  }

  /// <summary>
  /// 调用泛型方法
  /// </summary>
  /// <param name="instance">对象实例（如果是静态方法则为null）</param>
  /// <param name="methodName">方法名</param>
  /// <param name="genericTypes">泛型类型参数</param>
  /// <param name="parameters">方法参数</param>
  /// <returns>方法执行结果</returns>
  public static object InvokeGenericMethod(object instance, string methodName, Type[] genericTypes, params object[] parameters) {
    if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));
    if (genericTypes == null || genericTypes.Length == 0) throw new ArgumentNullException(nameof(genericTypes));

    Type type = instance?.GetType() ?? throw new ArgumentNullException(nameof(instance));

    try {
      // 查找泛型方法（先查找非泛型方法定义）
      MethodInfo method = FindGenericMethod(type, methodName, genericTypes, parameters);
      if (method == null) {
        throw new MethodNotFoundException($"泛型方法 '{methodName}' 在类型 '{type.FullName}' 中未找到");
      }

      // 创建泛型方法
      MethodInfo genericMethod = method.MakeGenericMethod(genericTypes);
      return genericMethod.Invoke(instance, parameters);
    } catch (TargetInvocationException ex) {
      throw ex.InnerException ?? ex;
    }
  }

  /// <summary>
  /// 调用静态泛型方法
  /// </summary>
  public static object InvokeStaticGenericMethod(Type type, string methodName, Type[] genericTypes, params object[] parameters) {
    if (type == null) throw new ArgumentNullException(nameof(type));

    try {
      MethodInfo method = FindGenericMethod(type, methodName, genericTypes, parameters);
      if (method == null) {
        throw new MethodNotFoundException($"静态泛型方法 '{methodName}' 在类型 '{type.FullName}' 中未找到");
      }

      if (!method.IsStatic) {
        throw new InvalidOperationException($"方法 '{methodName}' 不是静态方法");
      }

      MethodInfo genericMethod = method.MakeGenericMethod(genericTypes);
      return genericMethod.Invoke(null, parameters);
    } catch (TargetInvocationException ex) {
      throw ex.InnerException ?? ex;
    }
  }

  /// <summary>
  /// 创建类型实例
  /// </summary>
  public static object CreateInstance(Type type, params object[] constructorArgs) {
    if (type == null) throw new ArgumentNullException(nameof(type));

    if (constructorArgs == null || constructorArgs.Length == 0) {
      return Activator.CreateInstance(type);
    }

    Type[] argTypes = constructorArgs.Select(arg => arg?.GetType() ?? typeof(object)).ToArray();
    ConstructorInfo constructor = type.GetConstructor(argTypes);

    if (constructor == null) {
      throw new MissingMethodException($"未找到匹配的构造函数在类型 '{type.FullName}' 中");
    }

    return constructor.Invoke(constructorArgs);
  }

  /// <summary>
  /// 查找匹配的方法
  /// </summary>
  private static MethodInfo FindMethod(Type type, string methodName, object[] parameters) {
    // 获取所有同名方法
    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                  BindingFlags.Static | BindingFlags.Instance)
      .Where(m => m.Name == methodName)
      .ToList();

    if (methods.Count == 0) return null;

    // 如果没有参数，优先选择无参方法
    if (parameters == null || parameters.Length == 0) {
      return methods.FirstOrDefault(m => m.GetParameters().Length == 0)
             ?? methods.FirstOrDefault(m => m.GetParameters().All(p => p.HasDefaultValue));
    }

    // 查找参数匹配的方法
    foreach (var method in methods) {
      var paramInfos = method.GetParameters();

      if (paramInfos.Length != parameters.Length) continue;

      bool isMatch = true;
      for (int i = 0; i < paramInfos.Length; i++) {
        if (parameters[i] == null) {
          if (!IsNullable(paramInfos[i].ParameterType)) {
            isMatch = false;
            break;
          }
        } else if (!paramInfos[i].ParameterType.IsInstanceOfType(parameters[i])) {
          isMatch = false;
          break;
        }
      }

      if (isMatch) return method;
    }

    return null;
  }

  /// <summary>
  /// 查找泛型方法
  /// </summary>
  private static MethodInfo FindGenericMethod(Type type, string methodName, Type[] genericTypes, object[] parameters) {
    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                  BindingFlags.Static | BindingFlags.Instance)
      .Where(m => m.Name == methodName && m.IsGenericMethod)
      .ToList();

    if (methods.Count == 0) return null;

    foreach (var method in methods) {
      if (method.GetGenericArguments().Length != genericTypes.Length) continue;

      // 创建泛型方法以检查参数匹配
      try {
        MethodInfo genericMethod = method.MakeGenericMethod(genericTypes);
        var paramInfos = genericMethod.GetParameters();

        if (paramInfos.Length != parameters?.Length) continue;

        bool isMatch = true;
        for (int i = 0; i < paramInfos.Length; i++) {
          if (parameters[i] == null) {
            if (!IsNullable(paramInfos[i].ParameterType)) {
              isMatch = false;
              break;
            }
          } else if (!paramInfos[i].ParameterType.IsInstanceOfType(parameters[i])) {
            isMatch = false;
            break;
          }
        }

        if (isMatch) return method;
      } catch {
        // 类型约束不匹配，继续查找
        continue;
      }
    }

    return null;
  }

  /// <summary>
  /// 判断类型是否可为null
  /// </summary>
  private static bool IsNullable(Type type) {
    return !type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
  }

  /// <summary>
  /// 获取类型的所有方法信息
  /// </summary>
  public static List<MethodInfo> GetAllMethods(Type type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance) {
    if (type == null) throw new ArgumentNullException(nameof(type));
    return type.GetMethods(bindingFlags).ToList();
  }

  /// <summary>
  /// 检查类型是否包含指定方法
  /// </summary>
  public static bool HasMethod(Type type, string methodName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance) {
    if (type == null) throw new ArgumentNullException(nameof(type));
    if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));

    return type.GetMethod(methodName, bindingFlags) != null;
  }
}

public class MethodNotFoundException : Exception {
  public MethodNotFoundException(string s) {
    throw new NotImplementedException();
  }
}

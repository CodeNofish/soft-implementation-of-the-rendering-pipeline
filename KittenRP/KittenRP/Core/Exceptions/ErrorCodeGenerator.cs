using System.Collections.Concurrent;

namespace KittenRP.Core.Exceptions;

/// <summary>
/// ErrorCode生成和解析工具
/// </summary>
internal static class ErrorCodeGenerator {
  private static readonly ConcurrentDictionary<string, int> SequenceCounters = new();
  private static readonly Lock LockObject = new();

  /// <summary>
  /// 生成错误代码
  /// </summary>
  internal static string Generate(string component, string module, ErrorSeverity severity, int sequence = 0) {
    if (sequence == 0) {
      var key = FormatErrorCodeKey(component, module, severity);
      lock (LockObject) {
        SequenceCounters.TryGetValue(key, out int counter);
        counter++;
        SequenceCounters[key] = counter;
        sequence = counter;
      }
    }
    return FormatErrorCode(component, module, severity, sequence);
  }

  internal static string FormatErrorCodeKey(string component, string module, ErrorSeverity severity) {
    return $"{component}_{module}_{severity.ToShortest()}";
  }

  internal static string FormatErrorCode(string component, string module, ErrorSeverity severity, int sequence) {
    return $"{component}_{module}_{severity.ToShortest()}{sequence:D4}";
  }
}

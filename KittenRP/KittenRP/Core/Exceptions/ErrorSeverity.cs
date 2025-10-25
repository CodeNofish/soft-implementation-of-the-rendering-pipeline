namespace KittenRP.Core.Exceptions;

/// <summary>
/// 错误严重级别
/// </summary>
public enum ErrorSeverity {
  /// <summary>
  /// 信息级别，不影响正常流程
  /// </summary>
  Information,

  /// <summary>
  /// 警告级别，可能存在潜在问题
  /// </summary>
  Warning,

  /// <summary>
  /// 错误级别，功能受限但可继续运行
  /// </summary>
  Error,

  /// <summary>
  /// 严重错误，需要立即停止渲染
  /// </summary>
  Critical,
}

internal static class ErrorSeverityExtensions {
  // internal static string ToUpper(this ErrorSeverity errorSeverity) {
  //   return errorSeverity switch {
  //     ErrorSeverity.Information => "INFO",
  //     ErrorSeverity.Warning => "WARN",
  //     ErrorSeverity.Error => "ERROR",
  //     ErrorSeverity.Critical => "CRITICAL",
  //     _ => throw new ArgumentOutOfRangeException(nameof(errorSeverity), errorSeverity, null)
  //   };
  // }

  internal static string ToShortest(this ErrorSeverity errorSeverity) {
    return errorSeverity switch {
      ErrorSeverity.Information => "I",
      ErrorSeverity.Warning => "W",
      ErrorSeverity.Error => "E",
      ErrorSeverity.Critical => "C",
      _ => throw new ArgumentOutOfRangeException(nameof(errorSeverity), errorSeverity, null)
    };
  }

  // internal static ErrorSeverity FromShortest(string shortest) {
  //   return shortest switch {
  //     "I" => ErrorSeverity.Information,
  //     "W" => ErrorSeverity.Warning,
  //     "E" => ErrorSeverity.Error,
  //     "C" => ErrorSeverity.Critical,
  //     _ => throw new ArgumentException(shortest, nameof(shortest), null),
  //   };
  // }
}

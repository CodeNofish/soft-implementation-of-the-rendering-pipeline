using System.Resources;
using System.Runtime.Serialization;
using System.Text;
using KittenRP.Core.Utilities;

namespace KittenRP.Core.Exceptions;

/// <summary>
/// 渲染框架的基础异常类
/// </summary>
[Serializable]
public abstract class KittenRPException<TErrorMessageResourceManager> : Exception where TErrorMessageResourceManager : ResourceManager {
  // private readonly Lazy<ErrorCodeInfo> _errorCodeInfo;

  public string ErrorCode { get; }
  public DateTime Timestamp { get; }
  // public ErrorCodeInfo CodeInfo => _errorCodeInfo.Value;

  internal KittenRPException(string errorCode, Exception innerException, params object[] args)
    : base(GetFormattedErrorMessage(errorCode, args), innerException) {
    ErrorCode = errorCode;
    Timestamp = DateTime.UtcNow;

    // _errorCodeInfo = new Lazy<ErrorCodeInfo>(() => {
    //   try {
    //     return ErrorCodeGenerator.Parse(errorCode);
    //   } catch (Exception) {
    //     return new ErrorCodeInfo { Component = "UNK", Module = "UNKNOWN", Sequence = 0 };
    //   }
    // });
  }

  /// <summary>
  /// 格式化错误消息
  /// </summary>
  protected static string GetFormattedErrorMessage(string errorCode,  params object[] args) {
    string errorMessage;
    // 使用参数 格式化错误消息
    if (TryGetLocalizedUnformattedErrorMessage(errorCode, out errorMessage, args)) {
      if (args.Length != 0) {
        errorMessage = string.Format(errorMessage, args);
      }
    } else {
      errorMessage = string.Empty;
    }

    // 加入 错误代码 和 错误严重性
    if (string.IsNullOrEmpty(errorMessage)) {
      return $"{errorCode}";
    } else {
      return $"{errorCode}: {errorMessage}";
    }
  }

  /// <summary>
  /// 从资源文件获取本地化错误消息
  /// </summary>
  protected static bool TryGetLocalizedUnformattedErrorMessage(string errorCode, out string errorMessage, params object[] args) {
    try {
      errorMessage = ReflectionUtils.InvokeStaticMethod(typeof(TErrorMessageResourceManager), "GetString", errorCode)
        as string ?? throw new InvalidOperationException();
    } catch {
      // TODO 异常报告
      errorMessage = string.Empty;
      return false;
    }
    return true;
  }

  /// <summary>
  /// 获取错误的详细诊断信息
  /// </summary>
  // public virtual string GetDiagnosticInfo() {
  //   var sb = new StringBuilder();
  //   sb.AppendLine($"错误代码: {ErrorCode}");
  //   sb.AppendLine($"严重级别: {Severity}");
  //   sb.AppendLine($"时间戳: {Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
  //   sb.AppendLine($"组件: {CodeInfo.Component}");
  //   sb.AppendLine($"模块: {CodeInfo.Module}");
  //   sb.AppendLine($"序列号: {CodeInfo.Sequence}");
  //   return sb.ToString();
  // }
}

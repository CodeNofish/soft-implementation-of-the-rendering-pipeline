using KittenRP.Core.Exceptions;

namespace KittenRP.Core;

internal static class ErrorCodes {
  // Core组件
  internal const string CoreComponent = "CORE";

  // Core组件包含的模块
  internal const string ExceptionsModule = "EXCP";
  internal const string LoggingModule = "LOG";
  internal const string UtilitiesModule = "UTIL";


  #region Logging模块

  internal static string S = ErrorCodeGenerator.Generate(CoreComponent, LoggingModule, ErrorSeverity.Critical);

  #endregion
}

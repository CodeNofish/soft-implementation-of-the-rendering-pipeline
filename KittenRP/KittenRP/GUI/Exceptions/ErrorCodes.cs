using KittenRP.Core.Exceptions;

namespace KittenRP.GUI;

internal static class ErrorCodes {
  internal const string GuiComponent = "GUI";
  
  internal const string WindowModule = "WINDOW";
  
  #region Window Module

  internal static string S = ErrorCodeGenerator.Generate(GuiComponent, WindowModule, ErrorSeverity.Critical);

  #endregion
}

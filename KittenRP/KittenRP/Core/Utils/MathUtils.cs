using System.Runtime.CompilerServices;

namespace KittenRP;

public static class MathUtils {
  public const float PI = MathF.PI;
  public const float Deg2Rad = PI / 180f;
  public const float Rad2Deg = 180f / PI;

  /// <summary>
  /// 将值限制在[0, max]范围内循环
  /// </summary>
  public static float Repeat(float value, float max) {
    return value - MathF.Floor(value / max) * max;
  }

  /// <summary>
  /// 线性插值辅助方法
  /// </summary>
  public static float Lerp(float a, float b, float t) {
    t = MathF.Max(0f, MathF.Min(1f, t));
    return a + (b - a) * t;
  }

  /// <summary>
  /// 限制数值在[min, max]范围内
  /// </summary>
  public static float Clamp(float value, float min, float max) {
    return value < min ? min : value > max ? max : value;
  }

  /// <summary>
  /// 限制数值在[0f, 1f]范围内
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Clamp01(float value) {
    return value < 0f ? 0f : value > 1f ? 1f : value;
  }
}

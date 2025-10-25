using System.Runtime.InteropServices;
using JetBrains.Annotations;
using KittenRP.Core.Mathematics;
using KittenRP.Core.Utilities;

namespace KittenRP.Core.Engine;

/// <summary>
/// 颜色类，提供完整的颜色操作功能
/// 支持RGB、HSV、HSL颜色空间转换，颜色混合，颜色运算等
/// </summary>
[PublicAPI]
[StructLayout(LayoutKind.Sequential)]
public struct Color : IEquatable<Color> {
  public float r;
  public float g;
  public float b;
  public float a;

  // 常用颜色常量
  public static readonly Color clear = new Color(0f, 0f, 0f, 0f);
  public static readonly Color black = new Color(0f, 0f, 0f, 1f);
  public static readonly Color white = new Color(1f, 1f, 1f, 1f);
  public static readonly Color red = new Color(1f, 0f, 0f, 1f);
  public static readonly Color green = new Color(0f, 1f, 0f, 1f);
  public static readonly Color blue = new Color(0f, 0f, 1f, 1f);
  public static readonly Color yellow = new Color(1f, 1f, 0f, 1f);
  public static readonly Color cyan = new Color(0f, 1f, 1f, 1f);
  public static readonly Color magenta = new Color(1f, 0f, 1f, 1f);
  public static readonly Color gray = new Color(0.5f, 0.5f, 0.5f, 1f);
  public static readonly Color grey = new Color(0.5f, 0.5f, 0.5f, 1f);

  public Color(float r, float g, float b, float a = 1f) {
    this.r = Math.Max(0f, Math.Min(1f, r));
    this.g = Math.Max(0f, Math.Min(1f, g));
    this.b = Math.Max(0f, Math.Min(1f, b));
    this.a = Math.Max(0f, Math.Min(1f, a));
  }

  public Color(float r, float g, float b) : this(r, g, b, 1f) { }


  #region 基本颜色操作

  /// <summary>
  /// 颜色加法（分量相加，结果限制在[0,1]）
  /// </summary>
  public static Color operator +(Color a, Color b) {
    return new Color(
      Math.Min(1f, a.r + b.r),
      Math.Min(1f, a.g + b.g),
      Math.Min(1f, a.b + b.b),
      Math.Min(1f, a.a + b.a)
    );
  }

  /// <summary>
  /// 颜色减法（分量相减，结果限制在[0,1]）
  /// </summary>
  public static Color operator -(Color a, Color b) {
    return new Color(
      Math.Max(0f, a.r - b.r),
      Math.Max(0f, a.g - b.g),
      Math.Max(0f, a.b - b.b),
      Math.Max(0f, a.a - b.a)
    );
  }

  /// <summary>
  /// 颜色乘法（分量相乘，用于光照计算）
  /// </summary>
  public static Color operator *(Color a, Color b) {
    return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
  }

  /// <summary>
  /// 颜色数乘（每个分量乘以标量）
  /// </summary>
  public static Color operator *(Color a, float scalar) {
    return new Color(a.r * scalar, a.g * scalar, a.b * scalar, a.a * scalar);
  }

  /// <summary>
  /// 颜色数乘（标量 * 颜色）
  /// </summary>
  public static Color operator *(float scalar, Color a) {
    return a * scalar;
  }

  /// <summary>
  /// 颜色数除（每个分量除以标量）
  /// </summary>
  public static Color operator /(Color a, float scalar) {
    if (Math.Abs(scalar) < float.Epsilon)
      throw new DivideByZeroException("Cannot divide color by zero");

    return new Color(a.r / scalar, a.g / scalar, a.b / scalar, a.a / scalar);
  }

  /// <summary>
  /// 线性插值两个颜色
  /// </summary>
  public static Color Lerp(Color a, Color b, float t) {
    t = Math.Max(0f, Math.Min(1f, t));
    return new Color(
      a.r + (b.r - a.r) * t,
      a.g + (b.g - a.g) * t,
      a.b + (b.b - a.b) * t,
      a.a + (b.a - a.a) * t
    );
  }

  /// <summary>
  /// 非限制线性插值
  /// </summary>
  public static Color LerpUnclamped(Color a, Color b, float t) {
    return new Color(
      a.r + (b.r - a.r) * t,
      a.g + (b.g - a.g) * t,
      a.b + (b.b - a.b) * t,
      a.a + (b.a - a.a) * t
    );
  }

  #endregion


  #region 颜色空间转换

  /// <summary>
  /// RGB转HSV颜色空间
  /// </summary>
  public void RGBToHSV(out float h, out float s, out float v) {
    float max = Math.Max(r, Math.Max(g, b));
    float min = Math.Min(r, Math.Min(g, b));
    float delta = max - min;

    v = max;

    if (max < float.Epsilon || delta < float.Epsilon) {
      h = 0f;
      s = 0f;
      return;
    }

    s = delta / max;

    if (Math.Abs(r - max) < float.Epsilon)
      h = (g - b) / delta;
    else if (Math.Abs(g - max) < float.Epsilon)
      h = 2f + (b - r) / delta;
    else
      h = 4f + (r - g) / delta;

    h *= 60f;
    if (h < 0f)
      h += 360f;
  }

  /// <summary>
  /// HSV转RGB颜色空间
  /// </summary>
  public static Color HSVToRGB(float h, float s, float v, float a = 1f) {
    h = MathUtils.Repeat(h, 360f);
    s = Math.Max(0f, Math.Min(1f, s));
    v = Math.Max(0f, Math.Min(1f, v));

    if (s < float.Epsilon)
      return new Color(v, v, v, a);

    h /= 60f;
    int sector = (int)h;
    float fraction = h - sector;
    float p = v * (1f - s);
    float q = v * (1f - s * fraction);
    float t = v * (1f - s * (1f - fraction));

    switch (sector) {
    case 0: return new Color(v, t, p, a);
    case 1: return new Color(q, v, p, a);
    case 2: return new Color(p, v, t, a);
    case 3: return new Color(p, q, v, a);
    case 4: return new Color(t, p, v, a);
    default: return new Color(v, p, q, a);
    }
  }

  /// <summary>
  /// RGB转HSL颜色空间
  /// </summary>
  public void RGBToHSL(out float h, out float s, out float l) {
    float max = Math.Max(r, Math.Max(g, b));
    float min = Math.Min(r, Math.Min(g, b));
    float delta = max - min;

    l = (max + min) * 0.5f;

    if (delta < float.Epsilon) {
      h = 0f;
      s = 0f;
      return;
    }

    s = l > 0.5f ? delta / (2f - max - min) : delta / (max + min);

    if (Math.Abs(r - max) < float.Epsilon)
      h = (g - b) / delta + (g < b ? 6f : 0f);
    else if (Math.Abs(g - max) < float.Epsilon)
      h = (b - r) / delta + 2f;
    else
      h = (r - g) / delta + 4f;

    h *= 60f;
  }

  /// <summary>
  /// HSL转RGB颜色空间
  /// </summary>
  public static Color HSLToRGB(float h, float s, float l, float a = 1f) {
    h = MathUtils.Repeat(h, 360f);
    s = Math.Max(0f, Math.Min(1f, s));
    l = Math.Max(0f, Math.Min(1f, l));

    if (s < float.Epsilon)
      return new Color(l, l, l, a);

    float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
    float p = 2f * l - q;

    float hk = h / 360f;
    float[] t = new float[3];
    t[0] = hk + 1f / 3f;
    t[1] = hk;
    t[2] = hk - 1f / 3f;

    for (int i = 0; i < 3; i++) {
      if (t[i] < 0f) t[i] += 1f;
      if (t[i] > 1f) t[i] -= 1f;

      if (t[i] < 1f / 6f)
        t[i] = p + (q - p) * 6f * t[i];
      else if (t[i] < 1f / 2f)
        t[i] = q;
      else if (t[i] < 2f / 3f)
        t[i] = p + (q - p) * 6f * (2f / 3f - t[i]);
      else
        t[i] = p;
    }

    return new Color(t[0], t[1], t[2], a);
  }

  /// <summary>
  /// 从HSV创建颜色
  /// </summary>
  public static Color FromHSV(float h, float s, float v, float a = 1f) {
    return HSVToRGB(h, s, v, a);
  }

  /// <summary>
  /// 从HSL创建颜色
  /// </summary>
  public static Color FromHSL(float h, float s, float l, float a = 1f) {
    return HSLToRGB(h, s, l, a);
  }

  #endregion


  #region 颜色混合模式

  /// <summary>
  /// 正片叠底混合（Multiply）
  /// </summary>
  public static Color Multiply(Color a, Color b) {
    return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
  }

  /// <summary>
  /// 屏幕混合（Screen）
  /// </summary>
  public static Color Screen(Color a, Color b) {
    return new Color(
      1f - (1f - a.r) * (1f - b.r),
      1f - (1f - a.g) * (1f - b.g),
      1f - (1f - a.b) * (1f - b.b),
      a.a
    );
  }

  /// <summary>
  /// 叠加混合（Overlay）
  /// </summary>
  public static Color Overlay(Color a, Color b) {
    return new Color(
      a.r < 0.5f ? 2f * a.r * b.r : 1f - 2f * (1f - a.r) * (1f - b.r),
      a.g < 0.5f ? 2f * a.g * b.g : 1f - 2f * (1f - a.g) * (1f - b.g),
      a.b < 0.5f ? 2f * a.b * b.b : 1f - 2f * (1f - a.b) * (1f - b.b),
      a.a
    );
  }

  /// <summary>
  /// 柔光混合（Soft Light）
  /// </summary>
  public static Color SoftLight(Color a, Color b) {
    return new Color(
      b.r < 0.5f ? 2f * a.r * b.r + a.r * a.r * (1f - 2f * b.r) : 2f * a.r * (1f - b.r) + (float)Math.Sqrt(a.r) * (2f * b.r - 1f),
      b.g < 0.5f ? 2f * a.g * b.g + a.g * a.g * (1f - 2f * b.g) : 2f * a.g * (1f - b.g) + (float)Math.Sqrt(a.g) * (2f * b.g - 1f),
      b.b < 0.5f ? 2f * a.b * b.b + a.b * a.b * (1f - 2f * b.b) : 2f * a.b * (1f - b.b) + (float)Math.Sqrt(a.b) * (2f * b.b - 1f),
      a.a
    );
  }

  /// <summary>
  /// Alpha混合（标准透明度混合）
  /// </summary>
  public static Color AlphaBlend(Color foreground, Color background) {
    float alpha = foreground.a;
    float invAlpha = 1f - alpha;

    return new Color(
      foreground.r * alpha + background.r * invAlpha,
      foreground.g * alpha + background.g * invAlpha,
      foreground.b * alpha + background.b * invAlpha,
      alpha + background.a * invAlpha
    );
  }

  /// <summary>
  /// 加法混合（Additive）
  /// </summary>
  public static Color Additive(Color a, Color b) {
    return new Color(
      Math.Min(1f, a.r + b.r),
      Math.Min(1f, a.g + b.g),
      Math.Min(1f, a.b + b.b),
      Math.Min(1f, a.a + b.a)
    );
  }

  #endregion


  #region 颜色调整

  /// <summary>
  /// 调整亮度
  /// </summary>
  public Color WithBrightness(float brightness) {
    brightness = Math.Max(-1f, Math.Min(1f, brightness));
    return new Color(r + brightness, g + brightness, b + brightness, a);
  }

  /// <summary>
  /// 调整饱和度
  /// </summary>
  public Color WithSaturation(float saturation) {
    saturation = Math.Max(0f, Math.Min(2f, saturation));
    float gray = Grayscale();
    return new Color(
      gray + (r - gray) * saturation,
      gray + (g - gray) * saturation,
      gray + (b - gray) * saturation,
      a
    );
  }

  /// <summary>
  /// 调整对比度
  /// </summary>
  public Color WithContrast(float contrast) {
    contrast = Math.Max(0f, Math.Min(2f, contrast));
    float factor = (259f * (contrast + 255f)) / (255f * (259f - contrast));
    return new Color(
      factor * (r - 0.5f) + 0.5f,
      factor * (g - 0.5f) + 0.5f,
      factor * (b - 0.5f) + 0.5f,
      a
    );
  }

  /// <summary>
  /// 反转颜色
  /// </summary>
  public Color Invert() {
    return new Color(1f - r, 1f - g, 1f - b, a);
  }

  /// <summary>
  /// 计算灰度值
  /// </summary>
  public float Grayscale() {
    return 0.299f * r + 0.587f * g + 0.114f * b; // 标准亮度公式
  }

  /// <summary>
  /// 转换为灰度颜色
  /// </summary>
  public Color ToGrayscale() {
    float gray = Grayscale();
    return new Color(gray, gray, gray, a);
  }

  #endregion


  #region 颜色空间转换

  /// <summary>
  /// 线性到伽马空间转换
  /// </summary>
  public Color LinearToGamma() {
    return new Color(
      (float)Math.Pow(r, 1f / 2.2f),
      (float)Math.Pow(g, 1f / 2.2f),
      (float)Math.Pow(b, 1f / 2.2f),
      a
    );
  }

  /// <summary>
  /// 伽马到线性空间转换
  /// </summary>
  public Color GammaToLinear() {
    return new Color(
      (float)Math.Pow(r, 2.2f),
      (float)Math.Pow(g, 2.2f),
      (float)Math.Pow(b, 2.2f),
      a
    );
  }

  #endregion


  #region 格式转换

  /// <summary>
  /// 从32位颜色值创建（ARGB格式）
  /// </summary>
  public static Color FromColor32(uint color) {
    float a = ((color >> 24) & 0xFF) / 255f;
    float r = ((color >> 16) & 0xFF) / 255f;
    float g = ((color >> 8) & 0xFF) / 255f;
    float b = (color & 0xFF) / 255f;
    return new Color(r, g, b, a);
  }

  /// <summary>
  /// 转换为32位颜色值（ARGB格式）
  /// </summary>
  public uint ToColor32() {
    uint a32 = (uint)(MathUtils.Clamp(a, 0f, 1f) * 255) << 24;
    uint r32 = (uint)(MathUtils.Clamp(r, 0f, 1f) * 255) << 16;
    uint g32 = (uint)(MathUtils.Clamp(g, 0f, 1f) * 255) << 8;
    uint b32 = (uint)(MathUtils.Clamp(b, 0f, 1f) * 255);
    return a32 | r32 | g32 | b32;
  }

  /// <summary>
  /// 从HTML颜色代码创建（如 "#FF0000"）
  /// </summary>
  public static Color FromHtml(string htmlColor) {
    if (string.IsNullOrEmpty(htmlColor))
      return black;

    string colorStr = htmlColor.Trim().TrimStart('#');

    if (colorStr.Length == 6) // RRGGBB
    {
      return new Color(
        Convert.ToInt32(colorStr.Substring(0, 2), 16) / 255f,
        Convert.ToInt32(colorStr.Substring(2, 2), 16) / 255f,
        Convert.ToInt32(colorStr.Substring(4, 2), 16) / 255f
      );
    } else if (colorStr.Length == 8) // RRGGBBAA
    {
      return new Color(
        Convert.ToInt32(colorStr.Substring(0, 2), 16) / 255f,
        Convert.ToInt32(colorStr.Substring(2, 2), 16) / 255f,
        Convert.ToInt32(colorStr.Substring(4, 2), 16) / 255f,
        Convert.ToInt32(colorStr.Substring(6, 2), 16) / 255f
      );
    } else if (colorStr.Length == 3) // RGB
    {
      return new Color(
        Convert.ToInt32(colorStr.Substring(0, 1) + colorStr.Substring(0, 1), 16) / 255f,
        Convert.ToInt32(colorStr.Substring(1, 1) + colorStr.Substring(1, 1), 16) / 255f,
        Convert.ToInt32(colorStr.Substring(2, 1) + colorStr.Substring(2, 1), 16) / 255f
      );
    }

    return black;
  }

  /// <summary>
  /// 转换为HTML颜色代码
  /// </summary>
  public string ToHtml(bool includeAlpha = false) {
    string rHex = ((int)(r * 255)).ToString("X2");
    string gHex = ((int)(g * 255)).ToString("X2");
    string bHex = ((int)(b * 255)).ToString("X2");

    if (includeAlpha) {
      string aHex = ((int)(a * 255)).ToString("X2");
      return $"#{rHex}{gHex}{bHex}{aHex}";
    }

    return $"#{rHex}{gHex}{bHex}";
  }

  /// <summary>
  /// 转换为Vector4
  /// </summary>
  public Vector4 ToVector4() {
    return new Vector4(r, g, b, a);
  }

  /// <summary>
  /// 从Vector4创建颜色
  /// </summary>
  public static Color FromVector4(Vector4 vector) {
    return new Color(vector.x, vector.y, vector.z, vector.w);
  }

  #endregion


  #region 实用方法

  /// <summary>
  /// 限制颜色分量在有效范围内
  /// </summary>
  public Color Clamp() {
    return new Color(r, g, b, a);
  }

  /// <summary>
  /// 判断颜色是否近似相等
  /// </summary>
  public static bool Approximately(Color a, Color b, float tolerance = 0.001f) {
    return Math.Abs(a.r - b.r) < tolerance &&
           Math.Abs(a.g - b.g) < tolerance &&
           Math.Abs(a.b - b.b) < tolerance &&
           Math.Abs(a.a - b.a) < tolerance;
  }

  public override string ToString() {
    return $"RGBA({r:F3}, {g:F3}, {b:F3}, {a:F3})";
  }

  public string ToString(string format) {
    return $"RGBA({r.ToString(format)}, {g.ToString(format)}, {b.ToString(format)}, {a.ToString(format)})";
  }

  public bool Equals(Color other) {
    return Math.Abs(r - other.r) < float.Epsilon &&
           Math.Abs(g - other.g) < float.Epsilon &&
           Math.Abs(b - other.b) < float.Epsilon &&
           Math.Abs(a - other.a) < float.Epsilon;
  }

  public override bool Equals(object obj) {
    return obj is Color other && Equals(other);
  }

  public override int GetHashCode() {
    return r.GetHashCode() ^ (g.GetHashCode() << 2) ^ (b.GetHashCode() >> 2) ^ (a.GetHashCode() << 1);
  }
  public static bool operator ==(Color left, Color right) {
    return left.Equals(right);
  }

  public static bool operator !=(Color left, Color right) {
    return !(left == right);
  }

  #endregion
}

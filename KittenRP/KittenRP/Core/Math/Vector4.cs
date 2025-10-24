namespace KittenRP;

/// <summary>
/// 四维向量
/// </summary>
public struct Vector4 : IEquatable<Vector4> {
  public float x;
  public float y;
  public float z;
  public float w;

  // 常用常量
  public static readonly Vector4 Zero = new(0f, 0f, 0f, 0f);
  public static readonly Vector4 One = new(1f, 1f, 1f, 1f);
  public static readonly Vector4 UnitX = new(1f, 0f, 0f, 0f);
  public static readonly Vector4 UnitY = new(0f, 1f, 0f, 0f);
  public static readonly Vector4 UnitZ = new(0f, 0f, 1f, 0f);
  public static readonly Vector4 UnitW = new(0f, 0f, 0f, 1f);

  public Vector4(float x, float y, float z, float w) {
    this.x = x;
    this.y = y;
    this.z = z;
    this.w = w;
  }

  public Vector4(float x, float y, float z) {
    this.x = x;
    this.y = y;
    this.z = z;
    this.w = 0f;
  }

  public Vector4(Vector3 xyz, float w) {
    this.x = xyz.x;
    this.y = xyz.y;
    this.z = xyz.z;
    this.w = w;
  }

  public Vector4(Vector2 xy, float z, float w) {
    this.x = xy.x;
    this.y = xy.y;
    this.z = z;
    this.w = w;
  }


  #region MyRegion

  /// <summary>
  /// 将齐次坐标转换为3D坐标（透视除法）
  /// </summary>
  public Vector3 HomogeneousTo3D() {
    if (Math.Abs(w) < float.Epsilon)
      return new Vector3(x, y, z);

    return new Vector3(x / w, y / w, z / w);
  }

  /// <summary>
  /// 将3D坐标转换为齐次坐标（w=1）
  /// </summary>
  public static Vector4 From3DHomogeneous(Vector3 point, float w = 1f) {
    return new Vector4(point.x, point.y, point.z, w);
  }

  /// <summary>
  /// 将3D向量转换为齐次坐标（w=0）
  /// </summary>
  public static Vector4 From3DVector(Vector3 vector) {
    return new Vector4(vector.x, vector.y, vector.z, 0f);
  }

  /// <summary>
  /// 获取XYZ分量作为Vector3
  /// </summary>
  public Vector3 xyz {
    get { return new Vector3(x, y, z); }
    set {
      x = value.x;
      y = value.y;
      z = value.z;
    }
  }

  /// <summary>
  /// 获取XY分量作为Vector2
  /// </summary>
  public Vector2 xy {
    get { return new Vector2(x, y); }
    set {
      x = value.x;
      y = value.y;
    }
  }

  /// <summary>
  /// 作为颜色时的RGBA分量（值范围[0,1]）
  /// </summary>
  public float r {
    get { return x; }
    set { x = value; }
  }
  public float g {
    get { return y; }
    set { y = value; }
  }
  public float b {
    get { return z; }
    set { z = value; }
  }
  public float a {
    get { return w; }
    set { w = value; }
  }

  /// <summary>
  /// 创建颜色向量
  /// </summary>
  public static Vector4 Color(float r, float g, float b, float a = 1f) {
    return new Vector4(r, g, b, a);
  }

  /// <summary>
  /// 从32位颜色值创建向量（ARGB格式）
  /// </summary>
  public static Vector4 FromColor32(uint color) {
    float a = ((color >> 24) & 0xFF) / 255f;
    float r = ((color >> 16) & 0xFF) / 255f;
    float g = ((color >> 8) & 0xFF) / 255f;
    float b = (color & 0xFF) / 255f;
    return new Vector4(r, g, b, a);
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
  /// 伽马校正（用于颜色空间转换）
  /// </summary>
  public Vector4 GammaToLinear() {
    return new Vector4(
      (float)Math.Pow(r, 2.2f),
      (float)Math.Pow(g, 2.2f),
      (float)Math.Pow(b, 2.2f),
      a
    );
  }

  /// <summary>
  /// 线性到伽马空间转换
  /// </summary>
  public Vector4 LinearToGamma() {
    return new Vector4(
      (float)Math.Pow(r, 1f / 2.2f),
      (float)Math.Pow(g, 1f / 2.2f),
      (float)Math.Pow(b, 1f / 2.2f),
      a
    );
  }

  #endregion


  #region 实用方法

  /// <summary>
  /// 投影向量（将a投影到b上）
  /// </summary>
  public static Vector4 Project(Vector4 a, Vector4 b) {
    float sqrMag = b.SqrMagnitude;
    if (sqrMag < float.Epsilon)
      return Zero;

    float dot = Dot(a, b);
    return b * (dot / sqrMag);
  }

  /// <summary>
  /// 限制向量长度
  /// </summary>
  public static Vector4 ClampMagnitude(Vector4 vector, float maxLength) {
    if (vector.SqrMagnitude > maxLength * maxLength)
      return vector.Normalized * maxLength;
    return vector;
  }

  /// <summary>
  /// 限制颜色分量在[0,1]范围内
  /// </summary>
  public Vector4 ClampColor() {
    return new Vector4(
      Math.Max(0f, Math.Min(1f, r)),
      Math.Max(0f, Math.Min(1f, g)),
      Math.Max(0f, Math.Min(1f, b)),
      Math.Max(0f, Math.Min(1f, a))
    );
  }

  /// <summary>
  /// 判断向量是否近似相等（考虑浮点误差）
  /// </summary>
  public static bool Approximately(Vector4 a, Vector4 b, float tolerance = 0.0001f) {
    return Math.Abs(a.x - b.x) < tolerance &&
           Math.Abs(a.y - b.y) < tolerance &&
           Math.Abs(a.z - b.z) < tolerance &&
           Math.Abs(a.w - b.w) < tolerance;
  }

  /// <summary>
  /// 判断是否为有效的齐次坐标（w != 0）
  /// </summary>
  public bool IsValidHomogeneous() {
    return Math.Abs(w) > float.Epsilon;
  }

  /// <summary>
  /// 判断是否为方向向量（w == 0）
  /// </summary>
  public bool IsDirection() {
    return Math.Abs(w) < float.Epsilon;
  }

  /// <summary>
  /// 判断是否为位置向量（w == 1）
  /// </summary>
  public bool IsPosition() {
    return Math.Abs(w - 1f) < float.Epsilon;
  }

  #endregion


  #region 插值运算

  /// <summary>
  /// 线性插值（Lerp）- 在顶点属性间平滑过渡
  /// </summary>
  /// <param name="a">起始点</param>
  /// <param name="b">结束点</param>
  /// <param name="t">插值系数 [0,1]</param>
  public static Vector4 Lerp(Vector4 a, Vector4 b, float t) {
    t = Math.Max(0f, Math.Min(1f, t)); // 限制t在[0,1]范围内
    return new Vector4(
      a.x + (b.x - a.x) * t,
      a.y + (b.y - a.y) * t,
      a.z + (b.z - a.z) * t,
      a.w + (b.w - a.w) * t
    );
  }

  /// <summary>
  /// 非限制线性插值
  /// </summary>
  public static Vector4 LerpUnclamped(Vector4 a, Vector4 b, float t) {
    return new Vector4(
      a.x + (b.x - a.x) * t,
      a.y + (b.y - a.y) * t,
      a.z + (b.z - a.z) * t,
      a.w + (b.w - a.w) * t
    );
  }

  /// <summary>
  /// 球形线性插值（保持向量方向平滑过渡）
  /// </summary>
  public static Vector4 Slerp(Vector4 a, Vector4 b, float t) {
    t = Math.Max(0f, Math.Min(1f, t));

    float dot = Dot(a.Normalized, b.Normalized);
    dot = Math.Max(-1f, Math.Min(1f, dot)); // 防止浮点误差

    float theta = (float)Math.Acos(dot) * t;
    Vector4 relativeVec = b - a * dot;
    relativeVec.Normalize();

    return ((a * (float)Math.Cos(theta)) + (relativeVec * (float)Math.Sin(theta))) *
           MathUtils.Lerp(a.Magnitude, b.Magnitude, t);
  }

  #endregion


  #region 向量运算

  /// <summary>
  /// 点积（Dot Product）- 用于光照计算、投影判断
  /// </summary>
  public static float Dot(Vector4 a, Vector4 b) {
    return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
  }

  /// <summary>
  /// 向量长度（模）
  /// </summary>
  public float Magnitude {
    get { return (float)Math.Sqrt(x * x + y * y + z * z + w * w); }
  }

  /// <summary>
  /// 向量长度的平方（避免开方运算，用于比较长度时更高效）
  /// </summary>
  public float SqrMagnitude {
    get { return x * x + y * y + z * z + w * w; }
  }

  /// <summary>
  /// 归一化向量（单位向量）
  /// </summary>
  public Vector4 Normalized {
    get {
      float magnitude = Magnitude;
      if (magnitude > float.Epsilon)
        return this / magnitude;
      return Zero;
    }
  }

  /// <summary>
  /// 归一化当前向量
  /// </summary>
  public void Normalize() {
    float magnitude = Magnitude;
    if (magnitude > float.Epsilon) {
      x /= magnitude;
      y /= magnitude;
      z /= magnitude;
      w /= magnitude;
    } else {
      x = y = z = w = 0f;
    }
  }

  /// <summary>
  /// 计算两个点之间的距离
  /// </summary>
  public static float Distance(Vector4 a, Vector4 b) {
    return (a - b).Magnitude;
  }

  /// <summary>
  /// 计算两个点之间距离的平方
  /// </summary>
  public static float SqrDistance(Vector4 a, Vector4 b) {
    return (a - b).SqrMagnitude;
  }

  /// <summary>
  /// 计算两个向量之间的角度（弧度）
  /// </summary>
  public static float Angle(Vector4 from, Vector4 to) {
    float denominator = (float)Math.Sqrt(from.SqrMagnitude * to.SqrMagnitude);
    if (denominator < float.Epsilon)
      return 0f;

    float dot = MathUtils.Clamp(Dot(from, to) / denominator, -1f, 1f);
    return (float)Math.Acos(dot);
  }

  /// <summary>
  /// 计算两个向量之间的角度（角度）
  /// </summary>
  public static float AngleDegrees(Vector4 from, Vector4 to) {
    return Angle(from, to) * MathUtils.Rad2Deg;
  }

  #endregion


  #region 基本运算

  /// <summary>
  /// 向量加法
  /// </summary>
  public static Vector4 operator +(Vector4 a, Vector4 b) {
    return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
  }

  /// <summary>
  /// 向量减法
  /// </summary>
  public static Vector4 operator -(Vector4 a, Vector4 b) {
    return new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
  }

  /// <summary>
  /// 向量取负
  /// </summary>
  public static Vector4 operator -(Vector4 a) {
    return new Vector4(-a.x, -a.y, -a.z, -a.w);
  }

  /// <summary>
  /// 数乘（向量 * 标量）
  /// </summary>
  public static Vector4 operator *(Vector4 a, float scalar) {
    return new Vector4(a.x * scalar, a.y * scalar, a.z * scalar, a.w * scalar);
  }

  /// <summary>
  /// 数乘（标量 * 向量）
  /// </summary>
  public static Vector4 operator *(float scalar, Vector4 a) {
    return new Vector4(a.x * scalar, a.y * scalar, a.z * scalar, a.w * scalar);
  }

  /// <summary>
  /// 向量分量乘法（逐分量相乘）
  /// </summary>
  public static Vector4 operator *(Vector4 a, Vector4 b) {
    return new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
  }

  /// <summary>
  /// 数除（向量 / 标量）
  /// </summary>
  public static Vector4 operator /(Vector4 a, float scalar) {
    if (Math.Abs(scalar) < float.Epsilon)
      throw new DivideByZeroException("Cannot divide vector by zero");

    return new Vector4(a.x / scalar, a.y / scalar, a.z / scalar, a.w / scalar);
  }

  /// <summary>
  /// 向量分量除法（逐分量相除）
  /// </summary>
  public static Vector4 operator /(Vector4 a, Vector4 b) {
    return new Vector4(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
  }

  #endregion


  #region 对象方法

  public override string ToString() {
    return $"({x:F3}, {y:F3}, {z:F3}, {w:F3})";
  }

  public string ToString(string format) {
    return $"({x.ToString(format)}, {y.ToString(format)}, {z.ToString(format)}, {w.ToString(format)})";
  }

  public bool Equals(Vector4 other) {
    return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w);
  }

  public override bool Equals(object? obj) {
    return obj is Vector4 other && Equals(other);
  }

  public override int GetHashCode() {
    return HashCode.Combine(x, y, z, w);
  }

  public static bool operator ==(Vector4 left, Vector4 right) {
    return left.Equals(right);
  }

  public static bool operator !=(Vector4 left, Vector4 right) {
    return !(left == right);
  }

  #endregion
}

namespace KittenRP;

/// <summary>
/// 二维向量
/// </summary>
public struct Vector2 : IEquatable<Vector2> {
  public float x;
  public float y;

  public Vector2(float x, float y) {
    this.x = x;
    this.y = y;
  }

  // 常用常量
  public static readonly Vector2 Zero = new(0f, 0f);
  public static readonly Vector2 One = new(1f, 1f);
  public static readonly Vector2 Up = new(0f, 1f);
  public static readonly Vector2 Down = new(0f, -1f);
  public static readonly Vector2 Left = new(-1f, 0f);
  public static readonly Vector2 Right = new(1f, 0f);


  #region 其他运算

  /// <summary>
  /// 反射向量计算（用于碰撞反弹）
  /// </summary>
  /// <param name="inDirection">入射方向</param>
  /// <param name="inNormal">法线方向</param>
  public static Vector2 Reflect(Vector2 inDirection, Vector2 inNormal) {
    float factor = -2f * Dot(inNormal, inDirection);
    return new Vector2(
      factor * inNormal.x + inDirection.x,
      factor * inNormal.y + inDirection.y
    );
  }

  /// <summary>
  /// 投影向量（将a投影到b上）
  /// </summary>
  public static Vector2 Project(Vector2 a, Vector2 b) {
    float sqrMag = b.SqrMagnitude;
    if (sqrMag < float.Epsilon)
      return Zero;

    float dot = Dot(a, b);
    return b * (dot / sqrMag);
  }

  /// <summary>
  /// 限制向量长度
  /// </summary>
  public static Vector2 ClampMagnitude(Vector2 vector, float maxLength) {
    if (vector.SqrMagnitude > maxLength * maxLength)
      return vector.Normalized * maxLength;
    return vector;
  }

  #endregion


  #region 插值运算

  /// <summary>
  /// 线性插值（Lerp）- 在顶点属性间平滑过渡
  /// </summary>
  /// <param name="a">起始点</param>
  /// <param name="b">结束点</param>
  /// <param name="t">插值系数 [0,1]</param>
  public static Vector2 Lerp(Vector2 a, Vector2 b, float t) {
    t = MathUtils.Clamp01(t);
    return new Vector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
  }

  /// <summary>
  /// 非限制线性插值
  /// </summary>
  public static Vector2 LerpUnclamped(Vector2 a, Vector2 b, float t) {
    return new Vector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
  }

  /// <summary>
  /// 球形线性插值（保持向量长度）
  /// </summary>
  public static Vector2 Slerp(Vector2 a, Vector2 b, float t) {
    t = MathUtils.Clamp01(t);

    float dot = Dot(a.Normalized, b.Normalized);
    dot = MathF.Max(-1f, MathF.Min(1f, dot)); // 防止浮点误差

    float theta = MathF.Acos(dot) * t;
    Vector2 relativeVec = b - a * dot;
    relativeVec.Normalize();

    return ((a * MathF.Cos(theta)) + (relativeVec * MathF.Sin(theta))) *
           MathUtils.Lerp(a.Magnitude, b.Magnitude, t);
  }

  #endregion


  #region 向量运算

  /// <summary>
  /// 点积（Dot Product）- 用于光照计算、投影判断
  /// </summary>
  public static float Dot(Vector2 a, Vector2 b) {
    return a.x * b.x + a.y * b.y;
  }

  /// <summary>
  /// 叉积（Cross Product）- 用于计算法线、三角形方向判断
  /// 在2D中返回标量，表示有向面积
  /// </summary>
  public static float Cross(Vector2 a, Vector2 b) {
    return a.x * b.y - a.y * b.x;
  }

  /// <summary>
  /// 向量长度（模）
  /// </summary>
  public float Magnitude {
    get { return MathF.Sqrt(x * x + y * y); }
  }

  /// <summary>
  /// 向量长度的平方（避免开方运算，用于比较长度时更高效）
  /// </summary>
  public float SqrMagnitude {
    get { return x * x + y * y; }
  }

  /// <summary>
  /// 归一化向量（单位向量）
  /// </summary>
  public Vector2 Normalized {
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
    } else {
      x = y = 0f;
    }
  }

  /// <summary>
  /// 计算两个点之间的距离
  /// </summary>
  public static float Distance(Vector2 a, Vector2 b) {
    return (a - b).Magnitude;
  }

  /// <summary>
  /// 计算两个点之间距离的平方
  /// </summary>
  public static float SqrDistance(Vector2 a, Vector2 b) {
    return (a - b).SqrMagnitude;
  }

  #endregion


  #region 基本运算

  /// <summary>
  /// 向量加法
  /// </summary>
  public static Vector2 operator +(Vector2 a, Vector2 b) {
    return new Vector2(a.x + b.x, a.y + b.y);
  }

  /// <summary>
  /// 向量减法
  /// </summary>
  public static Vector2 operator -(Vector2 a, Vector2 b) {
    return new Vector2(a.x - b.x, a.y - b.y);
  }

  /// <summary>
  /// 向量取负
  /// </summary>
  public static Vector2 operator -(Vector2 a) {
    return new Vector2(-a.x, -a.y);
  }

  /// <summary>
  /// 数乘（向量 * 标量）
  /// </summary>
  public static Vector2 operator *(Vector2 a, float scalar) {
    return new Vector2(a.x * scalar, a.y * scalar);
  }

  /// <summary>
  /// 数乘（标量 * 向量）
  /// </summary>
  public static Vector2 operator *(float scalar, Vector2 a) {
    return new Vector2(a.x * scalar, a.y * scalar);
  }

  /// <summary>
  /// 数除（向量 / 标量）
  /// </summary>
  public static Vector2 operator /(Vector2 a, float scalar) {
    if (MathF.Abs(scalar) < float.Epsilon)
      throw new DivideByZeroException("Cannot divide vector by zero");
    return new Vector2(a.x / scalar, a.y / scalar);
  }

  #endregion


  #region 转换方法

  

  #endregion
  

  #region 对象方法

  public override string ToString() {
    return $"({x:F3}, {y:F3})";
  }

  public string ToString(string format) {
    return $"({x.ToString(format)}, {y.ToString(format)})";
  }
  
  public bool Equals(Vector2 other) {
    return x.Equals(other.x) && y.Equals(other.y);
  }

  public override bool Equals(object? obj) {
    return obj is Vector2 other && Equals(other);
  }

  public override int GetHashCode() {
    return HashCode.Combine(x, y);
  }

  public static bool operator ==(Vector2 left, Vector2 right) {
    return left.Equals(right);
  }

  public static bool operator !=(Vector2 left, Vector2 right) {
    return !(left == right);
  }

  #endregion
}

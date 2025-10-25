using System.Runtime.InteropServices;
using JetBrains.Annotations;
using KittenRP.Core.Utilities;

namespace KittenRP.Core.Mathematics;

/// <summary>
/// 三维向量
/// </summary>
[PublicAPI]
[StructLayout(LayoutKind.Sequential)]
public struct Vector3 : IEquatable<Vector3> {
  public float x;
  public float y;
  public float z;

  // 常用常量
  public static readonly Vector3 Zero = new(0f, 0f, 0f);
  public static readonly Vector3 One = new(1f, 1f, 1f);
  public static readonly Vector3 Up = new(0f, 1f, 0f);
  public static readonly Vector3 Down = new(0f, -1f, 0f);
  public static readonly Vector3 Left = new(-1f, 0f, 0f);
  public static readonly Vector3 Right = new(1f, 0f, 0f);
  public static readonly Vector3 Forward = new(0f, 0f, 1f);
  public static readonly Vector3 Back = new(0f, 0f, -1f);

  public Vector3(float x, float y, float z) {
    this.x = x;
    this.y = y;
    this.z = z;
  }

  public Vector3(float x, float y) {
    this.x = x;
    this.y = y;
    this.z = 0f;
  }


  #region 其他方法

  /// <summary>
  /// 反射向量计算（用于光照、碰撞反弹）
  /// </summary>
  /// <param name="inDirection">入射方向</param>
  /// <param name="inNormal">法线方向</param>
  public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal) {
    float factor = -2f * Dot(inNormal, inDirection);
    return new Vector3(
      factor * inNormal.x + inDirection.x,
      factor * inNormal.y + inDirection.y,
      factor * inNormal.z + inDirection.z
    );
  }

  /// <summary>
  /// 投影向量（将a投影到b上）
  /// </summary>
  public static Vector3 Project(Vector3 a, Vector3 b) {
    float sqrMag = b.SqrMagnitude;
    if (sqrMag < float.Epsilon)
      return Zero;

    float dot = Dot(a, b);
    return b * (dot / sqrMag);
  }

  /// <summary>
  /// 在平面上的投影（将向量投影到法线定义的平面上）
  /// </summary>
  public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal) {
    float sqrMag = planeNormal.SqrMagnitude;
    if (sqrMag < float.Epsilon)
      return vector;

    float dot = Dot(vector, planeNormal);
    return vector - planeNormal * (dot / sqrMag);
  }

  /// <summary>
  /// 限制向量长度
  /// </summary>
  public static Vector3 ClampMagnitude(Vector3 vector, float maxLength) {
    if (vector.SqrMagnitude > maxLength * maxLength)
      return vector.Normalized * maxLength;
    return vector;
  }

  /// <summary>
  /// 计算两个向量的叉积大小（有向面积）
  /// </summary>
  public static float CrossMagnitude(Vector3 a, Vector3 b) {
    return Cross(a, b).Magnitude;
  }

  /// <summary>
  /// 判断向量是否近似相等（考虑浮点误差）
  /// </summary>
  public static bool Approximately(Vector3 a, Vector3 b, float tolerance = 0.0001f) {
    return MathF.Abs(a.x - b.x) < tolerance &&
           MathF.Abs(a.y - b.y) < tolerance &&
           MathF.Abs(a.z - b.z) < tolerance;
  }

  #endregion


  #region 插值运算

  /// <summary>
  /// 线性插值（Lerp）- 在顶点属性间平滑过渡
  /// </summary>
  /// <param name="a">起始点</param>
  /// <param name="b">结束点</param>
  /// <param name="t">插值系数 [0,1]</param>
  public static Vector3 Lerp(Vector3 a, Vector3 b, float t) {
    t = MathF.Max(0f, MathF.Min(1f, t)); // 限制t在[0,1]范围内
    return new Vector3(
      a.x + (b.x - a.x) * t,
      a.y + (b.y - a.y) * t,
      a.z + (b.z - a.z) * t
    );
  }

  /// <summary>
  /// 非限制线性插值
  /// </summary>
  public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t) {
    return new Vector3(
      a.x + (b.x - a.x) * t,
      a.y + (b.y - a.y) * t,
      a.z + (b.z - a.z) * t
    );
  }

  /// <summary>
  /// 球形线性插值（保持向量方向平滑过渡）
  /// </summary>
  public static Vector3 Slerp(Vector3 a, Vector3 b, float t) {
    t = MathF.Max(0f, MathF.Min(1f, t));

    float dot = Dot(a.Normalized, b.Normalized);
    dot = MathF.Max(-1f, MathF.Min(1f, dot)); // 防止浮点误差

    float theta = MathF.Acos(dot) * t;
    Vector3 relativeVec = b - a * dot;
    relativeVec.Normalize();

    return ((a * MathF.Cos(theta)) + (relativeVec * MathF.Sin(theta))) *
           MathUtils.Lerp(a.Magnitude, b.Magnitude, t);
  }

  /// <summary>
  /// 平滑阻尼插值（用于相机跟随等平滑移动）
  /// </summary>
  public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed = float.PositiveInfinity) {
    float deltaTime = TimeUtils.DeltaTime;
    smoothTime = MathF.Max(0.0001f, smoothTime);

    float omega = 2f / smoothTime;
    float x = omega * deltaTime;
    float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

    Vector3 change = current - target;
    Vector3 originalTarget = target;

    float maxChange = maxSpeed * smoothTime;
    change = ClampMagnitude(change, maxChange);
    target = current - change;

    Vector3 temp = (currentVelocity + omega * change) * deltaTime;
    currentVelocity = (currentVelocity - omega * temp) * exp;

    Vector3 output = target + (change + temp) * exp;

    if (Dot(originalTarget - current, output - originalTarget) > 0) {
      output = originalTarget;
      currentVelocity = (output - originalTarget) / deltaTime;
    }

    return output;
  }

  #endregion


  #region 向量运算

  /// <summary>
  /// 点积（Dot Product）- 用于光照计算、投影判断、角度计算
  /// </summary>
  public static float Dot(Vector3 a, Vector3 b) {
    return a.x * b.x + a.y * b.y + a.z * b.z;
  }

  /// <summary>
  /// 叉积（Cross Product）- 用于计算法线、确定三角形朝向
  /// 结果向量垂直于输入向量构成的平面
  /// </summary>
  public static Vector3 Cross(Vector3 a, Vector3 b) {
    return new Vector3(
      a.y * b.z - a.z * b.y,
      a.z * b.x - a.x * b.z,
      a.x * b.y - a.y * b.x
    );
  }

  /// <summary>
  /// 向量长度（模）
  /// </summary>
  public float Magnitude {
    get { return MathF.Sqrt(x * x + y * y + z * z); }
  }

  /// <summary>
  /// 向量长度的平方（避免开方运算，用于比较长度时更高效）
  /// </summary>
  public float SqrMagnitude {
    get { return x * x + y * y + z * z; }
  }

  /// <summary>
  /// 归一化向量（单位向量）
  /// </summary>
  public Vector3 Normalized {
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
    } else {
      x = y = z = 0f;
    }
  }

  /// <summary>
  /// 计算两个点之间的距离
  /// </summary>
  public static float Distance(Vector3 a, Vector3 b) {
    return (a - b).Magnitude;
  }

  /// <summary>
  /// 计算两个点之间距离的平方
  /// </summary>
  public static float SqrDistance(Vector3 a, Vector3 b) {
    return (a - b).SqrMagnitude;
  }

  /// <summary>
  /// 计算两个向量之间的角度（弧度）
  /// </summary>
  public static float Angle(Vector3 from, Vector3 to) {
    float denominator = MathF.Sqrt(from.SqrMagnitude * to.SqrMagnitude);
    if (denominator < float.Epsilon)
      return 0f;

    float dot = MathUtils.Clamp(Dot(from, to) / denominator, -1f, 1f);
    return MathF.Acos(dot);
  }

  /// <summary>
  /// 计算两个向量之间的角度（角度）
  /// </summary>
  public static float AngleDegrees(Vector3 from, Vector3 to) {
    return Angle(from, to) * MathUtils.Rad2Deg;
  }

  #endregion


  #region 基本运算

  /// <summary>
  /// 向量加法
  /// </summary>
  public static Vector3 operator +(Vector3 a, Vector3 b) {
    return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
  }

  /// <summary>
  /// 向量减法
  /// </summary>
  public static Vector3 operator -(Vector3 a, Vector3 b) {
    return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
  }

  /// <summary>
  /// 向量取负
  /// </summary>
  public static Vector3 operator -(Vector3 a) {
    return new Vector3(-a.x, -a.y, -a.z);
  }

  /// <summary>
  /// 数乘（向量 * 标量）
  /// </summary>
  public static Vector3 operator *(Vector3 a, float scalar) {
    return new Vector3(a.x * scalar, a.y * scalar, a.z * scalar);
  }

  /// <summary>
  /// 数乘（标量 * 向量）
  /// </summary>
  public static Vector3 operator *(float scalar, Vector3 a) {
    return new Vector3(a.x * scalar, a.y * scalar, a.z * scalar);
  }

  /// <summary>
  /// 向量分量乘法（逐分量相乘）
  /// </summary>
  public static Vector3 operator *(Vector3 a, Vector3 b) {
    return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
  }

  /// <summary>
  /// 数除（向量 / 标量）
  /// </summary>
  public static Vector3 operator /(Vector3 a, float scalar) {
    if (MathF.Abs(scalar) < float.Epsilon)
      throw new DivideByZeroException("Cannot divide vector by zero");

    return new Vector3(a.x / scalar, a.y / scalar, a.z / scalar);
  }

  /// <summary>
  /// 向量分量除法（逐分量相除）
  /// </summary>
  public static Vector3 operator /(Vector3 a, Vector3 b) {
    return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
  }

  #endregion


  #region 转换方法

  /// <summary>
  /// 从Vector2创建Vector3（z=0）
  /// </summary>
  public static Vector3 FromVector2(Vector2 v2, float z = 0f) {
    return new Vector3(v2.x, v2.y, z);
  }

  /// <summary>
  /// 转换为Vector2（丢弃z分量）
  /// </summary>
  public Vector2 ToVector2() {
    return new Vector2(x, y);
  }

  #endregion


  #region 对象方法

  public override string ToString() {
    return $"({x:F3}, {y:F3}, {z:F3})";
  }

  public string ToString(string format) {
    return $"({x.ToString(format)}, {y.ToString(format)}, {z.ToString(format)})";
  }

  public bool Equals(Vector3 other) {
    return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
  }

  public override bool Equals(object? obj) {
    return obj is Vector3 other && Equals(other);
  }

  public override int GetHashCode() {
    return HashCode.Combine(x, y, z);
  }

  public static bool operator ==(Vector3 left, Vector3 right) {
    return left.Equals(right);
  }

  public static bool operator !=(Vector3 left, Vector3 right) {
    return !(left == right);
  }

  #endregion
}

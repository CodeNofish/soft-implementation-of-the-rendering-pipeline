using KittenRP.Core.Mathematics;

namespace KittenRP.Core.Engine;

/// <summary>
/// 顶点信息
/// </summary>
public class Vertex {
  public Vector3 Position;
  public Vector2 TexCoord;
  public Vector3 Normal;

  public Vertex(Vector3 position, Vector2 texCoord, Vector3 normal) {
    Position = position;
    TexCoord = texCoord;
    Normal = normal;
  }
}

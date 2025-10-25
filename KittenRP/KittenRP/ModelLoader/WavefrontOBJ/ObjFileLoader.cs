using System.Text.RegularExpressions;
using KittenRP.Core.Mathematics;

namespace KittenRP.ModelLoader;

public class ObjFileLoader {
  private List<Vector3> _positions = new();
  private List<Vector2> _texCoords = new();
  private List<Vector3> _normals = new();

  // 正则表达式用于高效解析
  private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
  
  
}

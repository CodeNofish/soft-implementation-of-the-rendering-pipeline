using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace KittenRP.GUI;

internal class KittenRPWindow {
  internal bool Created = false;
  internal IWindow? BackendWindow;

  internal void OpenWindow() {
    if (!Created) {
      CreateWindow();
    }
    BackendWindow!.Run();
  }

  internal void CreateWindow() {
    var options = BuildWindowOptions();
    BackendWindow = Window.Create(options);
    Created = true;
  }

  internal WindowOptions BuildWindowOptions() {
    WindowOptions options = new WindowOptions {
      // 窗口外观
      IsVisible = true,
      Position = new Vector2D<int>(50, 50),
      Size = new Vector2D<int>(1280, 720),
      Title = "Kitten Render Pipeline",
      WindowState = WindowState.Normal,
      WindowBorder = WindowBorder.Resizable,
      TopMost = true,

      // 图形渲染
      API = GraphicsAPI.DefaultVulkan,
      VSync = false,
      FramesPerSecond = 0f,
      UpdatesPerSecond = 0f,
      VideoMode = VideoMode.Default,

      // 缓冲区设置
      PreferredDepthBufferBits = null,
      PreferredStencilBufferBits = null,
      PreferredBitDepth = null,
      Samples = null,

      // 高级功能
      TransparentFramebuffer = false,
      SharedContext = null,
      IsEventDriven = false,
    };

    return options;
  }
}

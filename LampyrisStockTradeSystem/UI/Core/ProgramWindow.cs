/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: IMGUI主窗口逻辑 
*/

namespace LampyrisStockTradeSystem;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using ImGuiNET;
using System.Reflection;

public class ProgramWindow : GameWindow
{
    ImGuiController _controller;
    SceneRender _scene;

    public ProgramWindow() : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = new Vector2i(1600, 900), APIVersion = new Version(3, 3) })
    { }

    private static DebugProc _debugProcCallback = DebugCallback;
    private static GCHandle _debugProcCallbackHandle;
    private string inputText = "";
    private static List<StockKLineData> stockDataList = new List<StockKLineData>();

    private ValidCodeInputWindow validCodeInputWindow;

    // 主菜单管理
    private MenuItemManagement m_menuItemManager = new MenuItemManagement();

    private static void DebugCallback(DebugSource source, 
                                      DebugType type, 
                                      int id,
                                      DebugSeverity severity, 
                                      int length, 
                                      IntPtr message, 
                                      IntPtr userParam)
    {
        string messageString = Marshal.PtrToStringAnsi(message, length);
        Console.WriteLine($"{severity} {type} | {messageString}");

        if (type == DebugType.DebugTypeError)
            throw new Exception(messageString);
    }

    void SetupDebugging()
    {
        _debugProcCallbackHandle = GCHandle.Alloc(_debugProcCallback);

        GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        SetupDebugging();

        VSync = VSyncMode.On;

        _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
        _scene = new SceneRender(this);
        Error.Check();

        validCodeInputWindow = (ValidCodeInputWindow)WidgetManagement.GetWidget<ValidCodeInputWindow>();
        validCodeInputWindow.SetValidCodePNGFilePath("D:\\imgValidCode.png");

        WidgetManagement.GetWidget<StockQuoteTableWindow>();

        m_menuItemManager.Scan();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        // Update the opengl viewport
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

        // Tell ImGui of the new size
        _controller.WindowResized(ClientSize.X, ClientSize.Y);
    }

    private void DoMainMenuBar()
    {
        
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        LifecycleManager.Instance.Tick();

        Title = "Lampyris股票行情交易平台 OpenGL Version: " + GL.GetString(StringName.Version) + " FPS = " + (int)ImGui.GetIO().Framerate;

        _controller.Update(this, (float)e.Time);

        GL.ClearColor(new Color4(0, 32, 48, 255));
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        m_menuItemManager.PerformMenuItem();

        _controller.StartDockspace();
        Error.Check();

        WidgetManagement.Update();
        _scene.DrawViewportWindow();

        Error.Check();
        _controller.EndDockspace();
        _controller.Render();

        ImGuiController.CheckGLError("End of frame");

        SwapBuffers();
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        _controller.PressChar((char)e.Unicode);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        _controller.MouseScroll(e.Offset);
    }

    protected override void OnUnload()
    {
        _scene.Dispose();
        _controller.Dispose();

        base.OnUnload();
    }
}

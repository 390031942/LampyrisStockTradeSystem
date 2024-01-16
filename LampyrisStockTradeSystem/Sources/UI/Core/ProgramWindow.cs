/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: IMGUI主窗口逻辑 
*  Reference: https://github.com/Acruid/opentk-imgui-docking/blob/master/ImGuiNET.OpenTK.Sample/Window.cs
*/

namespace LampyrisStockTradeSystem;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Runtime.InteropServices;
using ImGuiNET;

public static class ProgramWindowDebugger
{
    public static void DebugCallback(DebugSource source,
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
}

public class ProgramWindow : GameWindow
{
    private ImGuiController m_controller;

    private static NativeWindowSettings ms_nativeWindowSetting => new NativeWindowSettings()
    { 
        Size = new Vector2i(1600, 900), 
        APIVersion = new Version(3, 3) 
    };

    public ProgramWindow() : base(GameWindowSettings.Default, ms_nativeWindowSetting)
    { 

    }

    private static DebugProc m_debugProcCallback = ProgramWindowDebugger.DebugCallback;

    private static GCHandle m_debugProcCallbackHandle;

    // 主菜单管理
    private MenuItemManagement m_menuItemManager = new MenuItemManagement();

    private void SetupDebugging()
    {
        m_debugProcCallbackHandle = GCHandle.Alloc(m_debugProcCallback);

        GL.DebugMessageCallback(m_debugProcCallback, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        // 设置OpenGL Debug Log回调函数
        SetupDebugging();

        // 垂直同步模式
        VSync = VSyncMode.On;

        // 初始化
        m_controller = new ImGuiController(ClientSize.X, ClientSize.Y);
        Error.Check();

        m_menuItemManager.Scan();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        // Update the opengl viewport
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

        // Tell ImGui of the new size
        m_controller.WindowResized(ClientSize.X, ClientSize.Y);
    }

    private void DoMainMenuBar()
    {
        
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        LifecycleManager.Instance.Tick();

        Title = "Lampyris股票行情交易平台 OpenGL Version: " + GL.GetString(StringName.Version) + " FPS = " + (int)ImGui.GetIO().Framerate;

        m_controller.Update(this, (float)e.Time);

        GL.ClearColor(new Color4(0, 32, 48, 255));
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        m_menuItemManager.PerformMenuItem();

        m_controller.StartDockspace();
        Error.Check();

        WidgetManagement.Update();

        Error.Check();
        m_controller.EndDockspace();
        m_controller.Render();

        ImGuiController.CheckGLError("End of frame");

        SwapBuffers();
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        m_controller.PressChar((char)e.Unicode);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        m_controller.MouseScroll(e.Offset);
    }

    protected override void OnUnload()
    {
        m_controller.Dispose();
        base.OnUnload();
    }
}

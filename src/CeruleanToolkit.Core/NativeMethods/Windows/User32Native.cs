using System.Runtime.InteropServices;

namespace CeruleanToolkit.Core.NativeMethods.Windows;

/// <summary>
/// 为 user32.dll 中的 API 提供 P/Invoke 声明
/// </summary>
public static partial class User32Native
{
    // 获取最后一次 P/Invoke 的错误码
    internal static int GetLastError() => Marshal.GetLastPInvokeError();

    // -------------------------------------------------
    // 光标相关 API
    // -------------------------------------------------

    // 从文件加载光标，返回句柄（失败返回 IntPtr.Zero）
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    public static partial IntPtr LoadCursorFromFile(string fileName);

    // 设置系统光标，成功返回 true
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetSystemCursor(IntPtr cursor, uint cursorId);

    // 销毁光标句柄
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyCursor(IntPtr cursor);
}

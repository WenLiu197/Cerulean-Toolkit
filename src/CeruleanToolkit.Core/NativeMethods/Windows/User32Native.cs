#if WINDOWS
using System.Runtime.InteropServices;

namespace CeruleanToolkit.Core.NativeMethods.Windows;

/// <summary>
/// 为 user32.dll 中的 API 包装
/// </summary>
public static partial class User32Native
{
    /// <summary>
    /// 表示位图资源的资源类型标识符，值为 0，即 .bmp 文件的资源类型。
    /// </summary>
    internal const uint IMAGE_BITMAP = 0;
    /// <summary>
    /// 表示图标资源的资源类型标识符，值为 1，即 .ico 文件的资源类型。
    /// </summary>
    internal const uint IMAGE_ICON = 1;
    /// <summary>
    /// 表示光标资源的资源类型标识符，值为 2，即 .cur 文件的资源类型。
    /// </summary>
    internal const uint IMAGE_CURSOR = 2;
    internal const uint LR_LOADFROMFILE = 0x0010;
    internal const uint LR_DEFAULTSIZE = 0x0040;
    // 获取最后一次 P/Invoke 的错误码
    internal static int GetLastError() => Marshal.GetLastPInvokeError();

    // -------------------------------------------------
    // 光标相关 API
    // -------------------------------------------------

    /// <summary>
    /// 返回加载的图像句柄（光标、图标或位图）
    /// </summary>
    /// <param name="hInst">
    /// <para>指定包含要加载的图像资源的模块实例句柄</para>
    /// <para>如果是从文件加载（而不是从 PE 资源加载），则此参数必须为 <see cref="IntPtr.Zero"/></para>
    /// </param>
    /// <param name="lpszName">如果是从文件加载（fuLoad 包含 LR_LOADFROMFILE），则此参数是文件的完整路径；如果是从资源加载，则是资源名称或 ID</param>
    /// <param name="uType">
    /// 指定要加载的图像类型。有三个可选值:
    /// <list type="bullet">
    /// <item><see cref="IMAGE_BITMAP"/> - 0 - 位图</item>
    /// <item><see cref="IMAGE_ICON"/> - 1 - 图标</item>
    /// <item><see cref="IMAGE_CURSOR"/> - 2 - 光标</item>
    /// </list>
    /// </param>
    /// <param name="cxDesired">指定要加载的图像的预期宽度</param>
    /// <param name="cyDesired">指定要加载的图像的预期高度</param>
    /// <param name="fuLoad">指定加载选项</param>
    /// <returns>
    /// <list type="bullet">成功时：返回图像句柄</list>
    /// <list type="bullet">失败时：返回 <see cref="IntPtr.Zero"/></list>
    /// </returns>
    [LibraryImport("user32.dll", EntryPoint = "LoadImageW", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial IntPtr LoadImage(
        IntPtr hInst,
        string lpszName,
        uint uType,
        int cxDesired,
        int cyDesired,
        uint fuLoad);

    /// <summary>
    /// 设置系统光标，成功返回 true
    /// </summary>
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetSystemCursor(IntPtr cursor, uint cursorId);

    /// <summary>
    /// 销毁光标句柄
    /// </summary>
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyCursor(IntPtr cursor);
}
#endif
#if WINDOWS
using CeruleanToolkit.Core.NativeMethods.Windows;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace CeruleanToolkit.Core.Helpers;

public static class CursorFixingHelper
{
    /// <summary>
    /// 为指定的光标角色加载并设置系统光标
    /// 成功时系统接管句柄，失败时自动释放句柄
    /// </summary>
    /// <param name="cursorFilePath">.cur/.ani 文件路径</param>
    /// <param name="cursorId">OCR_* 常量</param>
    /// <param name="roleName">仅用于错误消息</param>
    public static void ApplyCursor(string cursorFilePath, uint cursorId, string roleName)
    {
        IntPtr rawHandle = User32Native.LoadImage(IntPtr.Zero,
            cursorFilePath,
            User32Native.IMAGE_CURSOR,
            0, 0,
            User32Native.LR_LOADFROMFILE | User32Native.LR_DEFAULTSIZE);

        if (rawHandle == IntPtr.Zero)
        {
            int error = User32Native.GetLastError();
            throw new InvalidOperationException(
                $"LoadImage failed for '{roleName}' ({cursorFilePath}). Win32 error: {error}");
        }

        // 出错时自动释放，爽啊！
        using var safeHandle = new SafeCursorHandle(rawHandle, ownsHandle: true);

        if (!User32Native.SetSystemCursor(safeHandle.DangerousGetHandle(), cursorId))
        {
            int error = User32Native.GetLastError();
            throw new InvalidOperationException(
                $"SetSystemCursor failed for '{roleName}'. Win32 error: {error}");
        }

        // 系统接管句柄
        safeHandle.SetHandleAsInvalid();
    }
}


/// <summary>
/// 光标句柄包装，自动调用 DestroyCursor 释放。
/// <para>
/// <b>注意：调用 SetSystemCursor 成功后请调用 SetHandleAsInvalid()，
/// 因为此时系统已接管句柄所有权，不应再由本对象释放</b>
/// <para><i>你也不想让系统崩掉对吧？</i></para>
/// </para>
/// </summary>
public sealed class SafeCursorHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeCursorHandle(IntPtr preexistingHandle, bool ownsHandle) : base(ownsHandle)
    {
        SetHandle(preexistingHandle);
    }
    protected override bool ReleaseHandle()
    {
        return User32Native.DestroyCursor(handle);
    }
}
#endif
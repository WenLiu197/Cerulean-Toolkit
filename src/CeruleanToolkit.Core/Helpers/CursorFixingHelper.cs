#if WINDOWS
using CeruleanToolkit.Core.NativeMethods.Windows;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace CeruleanToolkit.Core.Helpers;

public static class CursorFixingHelper
{
    /// <summary>
    /// 应用当前注册表中的光标方案
    /// 若某个角色失败则立即抛出异常
    /// <para><i>仅限 Windows……</i></para>
    /// </summary>
    public static void ApplyCurrentCursorScheme()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Cursors");
        if (key == null)
            throw new InvalidOperationException("Cannot open HKCU\\Control Panel\\Cursors");

        foreach (var kvp in RoleToCursorId)
        {
            string role = kvp.Key;
            uint cursorId = kvp.Value;

            string? configuredPath = key.GetValue(role) as string;
            if (string.IsNullOrWhiteSpace(configuredPath))
                continue; // 当前方案未配置此角色，跳过

            string expandedPath = Environment.ExpandEnvironmentVariables(configuredPath);
            if (!File.Exists(expandedPath))
                throw new FileNotFoundException($"Cursor file not found: {expandedPath}", expandedPath);

            ApplyCursor(expandedPath, cursorId, role);
        }
    }

    /// <summary>
    /// 为指定的光标角色加载并设置系统光标
    /// 成功时系统接管句柄，失败时自动释放句柄
    /// </summary>
    /// <param name="cursorFilePath">.cur/.ani 文件路径</param>
    /// <param name="cursorId">OCR_* 常量</param>
    /// <param name="roleName">仅用于错误消息</param>
    private static void ApplyCursor(string cursorFilePath, uint cursorId, string roleName)
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

    /// <summary>
    ///  名称 -> OCR_* ID 的映射
    /// </summary>
    private static readonly Dictionary<string, uint> RoleToCursorId = new()
    {
        // 已完成今日 325 大学习
        ["Arrow"] = 32512,  // OCR_NORMAL
        ["Help"] = 32651,  // OCR_HELP
        ["AppStarting"] = 32650,  // OCR_APPSTARTING
        ["Wait"] = 32514,  // OCR_WAIT
        ["Crosshair"] = 32515,  // OCR_CROSS
        ["IBeam"] = 32513,  // OCR_IBEAM
        ["No"] = 32648,  // OCR_NO

        // 666, 用东南西北表方向
        ["SizeNS"] = 32645,  // OCR_SIZENS
        ["SizeWE"] = 32644,  // OCR_SIZEWE
        ["SizeNWSE"] = 32642,  // OCR_SIZENWSE
        ["SizeNESW"] = 32643,  // OCR_SIZENESW
        ["SizeAll"] = 32646,  // OCR_SIZEALL
        ["UpArrow"] = 32516,  // OCR_UP
        ["Hand"] = 32649,  // OCR_HAND
    };
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
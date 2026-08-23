using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CeruleanToolkit.Core.Helpers;

/// <summary>
/// 通过 Win32 API 获取安装目录，自动适应打包/非打包环境
/// </summary>
public static unsafe partial class InstallPathHelper
{
    // 定义常见的 Win32 错误码

    private const int ErrorSuccess = 0;

    private const int ErrorInsufficientBuffer = 122;

    // 15700 代表进程没有包标识（即普通 exe 运行环境）

    private const int AppModelErrorNoPackage = 15700;


    /// <summary>
    /// 如果在 MSIX 包中运行，返回安装目录；否则返回 exe 所在目录。
    /// </summary>
    public static string GetEffectivePath()
    {
        if (IsRunningAsMsixPackage())
        {
            return GetInstallDirectory() ?? AppContext.BaseDirectory;
        }
        else
        {
            // 🛠️ 开发便利区：直接返回程序基目录
            return AppContext.BaseDirectory;
        }
    }

    /// <summary>
    /// 判断当前是否以 MSIX 包形式运行。
    /// </summary>
    private static bool IsRunningAsMsixPackage()
    {
        int length = 0;
        // 调用真正的官方 API
        int hr = GetCurrentPackageFullName(ref length, IntPtr.Zero);

        // 如果返回 InsufficientBuffer(122)，说明是在包内
        // 如果返回 AppModelErrorNoPackage(15700)，说明是普通 exe
        return hr == ErrorInsufficientBuffer;
    }

    /// <summary>
    /// 获取 MSIX 包的安装目录。
    /// </summary>
    private static string? GetInstallDirectory()
    {
        int length = 0;
        // 第一步：获取需要的缓冲区长度
        int hr = GetCurrentPackageFullName(ref length, IntPtr.Zero);

        if (hr != ErrorInsufficientBuffer)
            return null;

        // 🛡️ 内存安全优化：使用 stackalloc 在栈上分配缓冲区
        Span<char> buffer = stackalloc char[length];

        // 第二步：再次调用，将结果写入缓冲区
        // 既然参数已经是 Span<char>，运行时会自动处理地址，直接传 buffer 即可！
        hr = GetCurrentPackageFullName(ref length, buffer);

        if (hr != ErrorSuccess)
            return null;

        // 切片并拼接路径（去掉末尾的 \0）
        string packageFullName = buffer.Slice(0, length - 1).ToString();
        return System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "WindowsApps",
            packageFullName);
    }

    /// <summary>
    /// 官方原生 API：获取当前包的全名。
    /// </summary>
    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Error)]
    private static partial int GetCurrentPackageFullName(ref int packageFullNameLength, IntPtr packageFullName);

    /// <summary>
    /// 重载版本：支持将字符串写入指定的内存地址（Span）。
    /// 注意：参数直接用 Span<char>，运行时会自动处理指针，代码更清爽！
    /// </summary>
    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Error)]
    private static partial int GetCurrentPackageFullName(ref int packageFullNameLength, Span<char> packageFullName);
}
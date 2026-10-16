#if WINDOWS
using CeruleanToolkit.Core.Helpers;
using CeruleanToolkit.Core.Interfaces.System;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace CeruleanToolkit.Core.Services.SystemService;

public sealed class CursorFixService : ICursorFixService
{
    /// <summary>
    /// 应用当前注册表中的光标方案
    /// 若某个角色失败则立即抛出异常
    /// <para><i>仅限 Windows……</i></para>
    /// </summary>
    public void ApplyCurrentCursorTheme()
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

            CursorFixingHelper.ApplyCursor(expandedPath, cursorId, role);
        }
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
#endif
using CeruleanToolkit.Core.NativeMethods.Windows;
using Microsoft.Win32.SafeHandles;

namespace CeruleanToolkit.Core.Helpers;

public static class CursorHelper
{

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
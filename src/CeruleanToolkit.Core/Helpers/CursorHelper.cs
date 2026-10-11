using System.Runtime.InteropServices;

namespace CeruleanToolkit.Core.Helpers;

public static partial class CursorHelper
{
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    public static partial IntPtr LoadCursorFromFile(string fileName);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetSystemCursor(IntPtr cursor, uint cursorId);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyCursor(IntPtr cursor);
}

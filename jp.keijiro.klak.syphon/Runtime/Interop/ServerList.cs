using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;

namespace Klak.Syphon.Interop {

public sealed class ServerList : SafeHandleZeroOrMinusOneIsInvalid
{
    #region SafeHandle implementation

    ServerList() : base(true) {}

    protected override bool ReleaseHandle()
    {
        _Destroy(handle);
        return true;
    }

    #endregion

    #region Factory method

    public static ServerList Create() => _Create();

    #endregion

    #region Public methods and properties

    public int Count => _GetCount(this);
    public string GetName(int index) => ToString(_GetName(this, index));
    public string GetAppName(int index) => ToString(_GetAppName(this, index));

    #endregion

    #region Helper methods

    public string[] GetCombinedNameArray()
    {
        var list = new string[Count];
        for (var i = 0; i < Count; i++)
        {
            var (app, name) = (GetAppName(i), GetName(i));
            list[i] = string.IsNullOrEmpty(name) ? app : $"{app}/{name}";
        }
        return list;
    }

    #endregion

    #region Unmanaged interface

    string ToString(IntPtr ptr)
      => ptr != IntPtr.Zero ? Marshal.PtrToStringAnsi(ptr) : null;

    [DllImport("KlakSyphon", EntryPoint = "Plugin_CreateServerList")]
    static extern ServerList _Create();

    [DllImport("KlakSyphon", EntryPoint = "Plugin_DestroyServerList")]
    static extern void _Destroy(IntPtr list);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_GetServerListCount")]
    static extern int _GetCount(ServerList list);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_GetNameFromServerList")]
    static extern IntPtr _GetName(ServerList list, int index);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_GetAppNameFromServerList")]
    static extern IntPtr _GetAppName(ServerList list, int index);

    #endregion
}

} // namespace Klak.Syphon.Interop

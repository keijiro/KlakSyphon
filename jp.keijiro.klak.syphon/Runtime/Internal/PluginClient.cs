using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Klak.Syphon.Interop {

sealed class PluginClient : SafeHandleZeroOrMinusOneIsInvalid
{
    #region SafeHandle implementation

    PluginClient() : base(true) {}

    protected override bool ReleaseHandle()
    {
        _DestroyClient(handle);
        return true;
    }

    #endregion

    #region Public methods

    public static PluginClient Create(string name)
      => _CreateClient(name, "");

    public static PluginClient Create(string name, string appName)
      => _CreateClient(name, appName);

    public bool IsValid
      => _IsClientValid(this);

    public IntPtr TexturePointer
      => _GetClientTexture(this);

    public int Width
      => _GetClientTextureWidth(this);

    public int Height
      => _GetClientTextureHeight(this);

    public void Update()
      => _UpdateClient(this);

    #endregion

    #region Unmanaged interface

    [DllImport("KlakSyphon", EntryPoint = "Plugin_CreateClient")]
    public static extern PluginClient _CreateClient(string name, string appName);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_DestroyClient")]
    public static extern void _DestroyClient(IntPtr instance);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_IsClientValid")]
    public static extern bool _IsClientValid(PluginClient instance);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_GetClientTexture")]
    public static extern IntPtr _GetClientTexture(PluginClient instance);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_GetClientTextureWidth")]
    public static extern int _GetClientTextureWidth(PluginClient instance);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_GetClientTextureHeight")]
    public static extern int _GetClientTextureHeight(PluginClient instance);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_UpdateClient")]
    public static extern void _UpdateClient(PluginClient instance);

    #endregion
}

} // namespace Klak.Syphon.Interop

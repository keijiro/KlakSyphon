using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

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

    public static PluginClient Create(string appName, string serverName)
      => _CreateClient(serverName, appName);

    public static PluginClient Create((string app, string server) name)
      => _CreateClient(name.server, name.app);

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

    #region Helper methods

    public bool HasSameTexture(Texture2D texture)
      => texture != null && texture.GetNativeTexturePtr() == TexturePointer;

    public Texture2D CreateTexture()
    {
        var pointer = TexturePointer;
        if (pointer == IntPtr.Zero) return null;
        var tex = Texture2D.CreateExternalTexture
           (Width, Height, TextureFormat.RGBA32, false, false, pointer);
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    #endregion

    #region Unmanaged interface

    [DllImport("KlakSyphon", EntryPoint = "Plugin_CreateClient")]
    public static extern PluginClient _CreateClient(string serverName, string appName);

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

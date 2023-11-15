using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Klak.Syphon.Interop {

public sealed class PluginServer : SafeHandleZeroOrMinusOneIsInvalid
{
    #region SafeHandle implementation

    PluginServer() : base(true) {}

    protected override bool ReleaseHandle()
    {
        _Destroy(handle);
        return true;
    }

    #endregion

    #region Factory method

    public static PluginServer Create(string name, int width, int height)
      => _Create(name, width, height);

    #endregion

    #region Public methods and properties

    public IntPtr TexturePointer => _GetTexture(this);
    public void PublishTexture() => _PublishTexture(this);

    #endregion

    #region Helper methods

    public static (PluginServer, Texture2D)
      CreateWithBackedTexture(string name, int width, int height)
    {
        var instance = Create(name, width, height);
        var texture = Texture2D.CreateExternalTexture
           (width, height, TextureFormat.RGBA32,
            false, false, instance.TexturePointer);
        return (instance, texture);
    }

    #endregion

    #region Unmanaged interface

    [DllImport("KlakSyphon", EntryPoint = "Plugin_CreateServer")]
    static extern PluginServer _Create(string name, int width, int height);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_DestroyServer")]
    static extern void _Destroy(IntPtr instance);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_GetServerTexture")]
    static extern IntPtr _GetTexture(PluginServer instance);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_PublishServerTexture")]
    static extern void _PublishTexture(PluginServer instance);

    #endregion
}

} // namespace Klak.Syphon.Interop

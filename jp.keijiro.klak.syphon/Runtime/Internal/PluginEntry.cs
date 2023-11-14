using System;
using System.Runtime.InteropServices;

namespace Klak.Syphon {

static class Plugin
{
    [DllImport("KlakSyphon", EntryPoint = "Plugin_CreateClient")]
    public static extern IntPtr CreateClient(string name, string appName);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_DestroyClient")]
    public static extern void DestroyClient(IntPtr instance);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_IsClientValid")]
    public static extern bool IsClientValid(IntPtr instance);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_GetClientTexture")]
    public static extern IntPtr GetClientTexture(IntPtr instance);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_GetClientTextureWidth")]
    public static extern int GetClientTextureWidth(IntPtr instance);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_GetClientTextureHeight")]
    public static extern int GetClientTextureHeight(IntPtr instance);

    [DllImport("KlakSyphon", EntryPoint = "Plugin_UpdateClient")]
    public static extern void UpdateClient(IntPtr instance);
}

} // namespace Klak.Syphon

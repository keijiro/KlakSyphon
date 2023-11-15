using System.Runtime.InteropServices;

namespace Klak.Syphon.Interop {

public static class PluginCommon
{
    [DllImport("KlakSyphon", EntryPoint = "Plugin_EnableColorSpaceConversion")]
    public static extern void EnableColorSpaceConversion();

    [DllImport("KlakSyphon", EntryPoint = "Plugin_DisableColorSpaceConversion")]
    public static extern void DisableColorSpaceConversion();
}

} // namespace Klak.Sypho.Interopn

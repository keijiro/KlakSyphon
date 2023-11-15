using UnityEngine;
using Klak.Syphon.Interop;

namespace Klak.Syphon {

internal static class SyphonCommon
{
    // Apply the current color space setting.
    // Actually this is needed only once, but we do every time for simplicity.
    internal static void ApplyCurrentColorSpace()
    {
        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            PluginCommon.EnableColorSpaceConversion();
        else
            PluginCommon.DisableColorSpaceConversion();
    }
}

} // namespace Klak.Syphon

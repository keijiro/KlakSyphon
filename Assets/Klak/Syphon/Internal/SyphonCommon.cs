// KlakSyphon - Syphon plugin for Unity
// https://github.com/keijiro/KlakSyphon

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Klak.Syphon
{
    internal static class SyphonCommon
    {
        // Apply the current color space setting.
        // Actually this is needed only once, but we do every time for simplicity.
        internal static void ApplyCurrentColorSpace()
        {
            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                Plugin_EnableColorSpaceConversion();
            else
                Plugin_DisableColorSpaceConversion();
        }

        [DllImport("KlakSyphon")]
        static extern void Plugin_EnableColorSpaceConversion();

        [DllImport("KlakSyphon")]
        static extern void Plugin_DisableColorSpaceConversion();
    }
}

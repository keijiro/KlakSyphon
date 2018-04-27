// KlakSyphon - Syphon plugin for Unity
// https://github.com/keijiro/KlakSyphon

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Klak.Syphon
{
    public class SyphonServer : MonoBehaviour
    {
        #region Private variables

        IntPtr _serverInstance;
        Texture _serverTexture;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _serverInstance = Plugin_CreateServer("Test", 512, 512);

            _serverTexture = Texture2D.CreateExternalTexture(
                512, 512, TextureFormat.RGBA32, false, false,
                Plugin_GetServerTexture(_serverInstance)
            );
        }

        void OnDestroy()
        {
            Plugin_DestroyServer(_serverInstance);
            Destroy(_serverTexture);
        }

        void Update()
        {
            Plugin_PublishServerTexture(_serverInstance);
        }

        void OnRenderImage(RenderTexture source, RenderTexture dest)
        {
            var temp = RenderTexture.GetTemporary(
                _serverTexture.width, _serverTexture.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.Linear
            );

            Graphics.Blit(source, temp);
            Graphics.CopyTexture(temp, _serverTexture);

            RenderTexture.ReleaseTemporary(temp);

            Graphics.Blit(source, dest);
        }

        #endregion

        #region Native plugin entry points

        [DllImport("KlakSyphon")]
        private static extern IntPtr Plugin_CreateServer(string name, int width, int height);

        [DllImport("KlakSyphon")]
        private static extern void Plugin_DestroyServer(IntPtr instance);

        [DllImport("KlakSyphon")]
        private static extern IntPtr Plugin_GetServerTexture(IntPtr instance);

        [DllImport("KlakSyphon")]
        private static extern void Plugin_PublishServerTexture(IntPtr instance);

        #endregion
    }
}

// Syphon server test

using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class ServerTest : MonoBehaviour
{
    #region Native plugin entry points

    [DllImport("KlakSyphon")]
    private static extern IntPtr Klak_CreateServer(string name, int width, int height);

    [DllImport("KlakSyphon")]
    private static extern void Klak_DestroyServer(IntPtr instance);

    [DllImport("KlakSyphon")]
    private static extern IntPtr Klak_GetServerTexture(IntPtr instance);

    [DllImport("KlakSyphon")]
    private static extern void Klak_PublishServerTexture(IntPtr instance);

    #endregion

    #region Private variables

    IntPtr _serverInstance;
    Texture _serverTexture;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _serverInstance = Klak_CreateServer("Test", 512, 512);

        _serverTexture = Texture2D.CreateExternalTexture(
            512, 512, TextureFormat.RGBA32, false, false,
            Klak_GetServerTexture(_serverInstance)
        );
    }

    void OnDestroy()
    {
        Klak_DestroyServer(_serverInstance);
        Destroy(_serverTexture);
    }

    void Update()
    {
        Klak_PublishServerTexture(_serverInstance);
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
}

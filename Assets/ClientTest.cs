// Syphon client test

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class ClientTest : MonoBehaviour
{
    #region Native plugin entry points

    [DllImport("KlakSyphon")]
    static extern IntPtr Klak_CreateClient();

    [DllImport("KlakSyphon")]
    static extern void Klak_DestroyClient(IntPtr instance);

    [DllImport("KlakSyphon")]
    static extern IntPtr Klak_GetClientTexture(IntPtr instance);

    [DllImport("KlakSyphon")]
    static extern int Klak_GetClientTextureWidth(IntPtr instance);

    [DllImport("KlakSyphon")]
    static extern int Klak_GetClientTextureHeight(IntPtr instance);

    [DllImport("KlakSyphon")]
    static extern IntPtr Klak_GetClientUpdateCallback();

    #endregion

    #region Editable attributes

    [SerializeField] Renderer _renderer;

    #endregion

    #region Private variables

    IntPtr _clientInstance;
    Texture _clientTexture;
    CommandBuffer _updateCommand;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _updateCommand = new CommandBuffer();
    }

    void OnDestroy()
    {
        if (_clientInstance != IntPtr.Zero) Klak_DestroyClient(_clientInstance);
        if (_clientTexture != null) Destroy(_clientTexture);
        _updateCommand.Dispose();
    }

    void Update()
    {
        if (_clientInstance == IntPtr.Zero) _clientInstance = Klak_CreateClient();

        if (_clientInstance != IntPtr.Zero)
        {
            var pluginTexture = Klak_GetClientTexture(_clientInstance);

            if (_clientTexture != null &&
                _clientTexture.GetNativeTexturePtr() != pluginTexture)
            {
                Destroy(_clientTexture);
                _clientTexture = null;
            }

            if (_clientTexture == null && pluginTexture != IntPtr.Zero)
            {
                _clientTexture = Texture2D.CreateExternalTexture(
                    Klak_GetClientTextureWidth(_clientInstance),
                    Klak_GetClientTextureHeight(_clientInstance),
                    TextureFormat.RGBA32, false, false, pluginTexture
                );
                _renderer.material.mainTexture = _clientTexture;
            }

            _updateCommand.Clear();
            _updateCommand.IssuePluginEventAndData(
                Klak_GetClientUpdateCallback(), 0, _clientInstance
            );

            Graphics.ExecuteCommandBuffer(_updateCommand);
        }
    }

    #endregion
}

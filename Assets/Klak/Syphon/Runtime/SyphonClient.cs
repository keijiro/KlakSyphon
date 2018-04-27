// KlakSyphon - Syphon plugin for Unity
// https://github.com/keijiro/KlakSyphon

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Klak.Syphon
{
    public class SyphonClient : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] string _appName;
        [SerializeField] string _name;
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
            if (_clientInstance != IntPtr.Zero) Plugin_DestroyClient(_clientInstance);
            if (_clientTexture != null) Destroy(_clientTexture);
            _updateCommand.Dispose();
        }

        void Update()
        {
            if (_clientInstance == IntPtr.Zero)
                _clientInstance = Plugin_CreateClient(_name, _appName);

            if (_clientInstance != IntPtr.Zero)
            {
                var pluginTexture = Plugin_GetClientTexture(_clientInstance);

                if (_clientTexture != null &&
                    _clientTexture.GetNativeTexturePtr() != pluginTexture)
                {
                    Destroy(_clientTexture);
                    _clientTexture = null;
                }

                if (_clientTexture == null && pluginTexture != IntPtr.Zero)
                {
                    _clientTexture = Texture2D.CreateExternalTexture(
                        Plugin_GetClientTextureWidth(_clientInstance),
                        Plugin_GetClientTextureHeight(_clientInstance),
                        TextureFormat.RGBA32, false, false, pluginTexture
                    );
                    _renderer.material.mainTexture = _clientTexture;
                }

                _updateCommand.Clear();
                _updateCommand.IssuePluginEventAndData(
                    Plugin_GetClientUpdateCallback(), 0, _clientInstance
                );

                Graphics.ExecuteCommandBuffer(_updateCommand);
            }
        }

        #endregion

        #region Native plugin entry points

        [DllImport("KlakSyphon")]
        static extern IntPtr Plugin_CreateClient(string name, string appName);

        [DllImport("KlakSyphon")]
        static extern void Plugin_DestroyClient(IntPtr instance);

        [DllImport("KlakSyphon")]
        static extern IntPtr Plugin_GetClientTexture(IntPtr instance);

        [DllImport("KlakSyphon")]
        static extern int Plugin_GetClientTextureWidth(IntPtr instance);

        [DllImport("KlakSyphon")]
        static extern int Plugin_GetClientTextureHeight(IntPtr instance);

        [DllImport("KlakSyphon")]
        static extern IntPtr Plugin_GetClientUpdateCallback();

        #endregion
    }
}

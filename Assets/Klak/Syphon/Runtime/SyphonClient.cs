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
        #region Source settings

        [SerializeField] string _appName;

        public string appName {
            get { return _appName; }
            set { _appName = value; }
        }

        [SerializeField] string _serverName;

        public string serverName {
            get { return _serverName; }
            set { _serverName = value; }
        }

        #endregion

        #region Target settings

        [SerializeField] RenderTexture _targetTexture;

        public RenderTexture targetTexture {
            get { return _targetTexture; }
            set { _targetTexture = value; }
        }

        [SerializeField] Renderer _targetRenderer;

        public Renderer targetRenderer {
            get { return _targetRenderer; }
            set { _targetRenderer = value; }
        }

        [SerializeField] string _targetMaterialProperty;

        public string targetMaterialProperty {
            get { return _targetMaterialProperty; }
            set { targetMaterialProperty = value; }
        }

        #endregion

        #region Public properties

        public Texture receivedTexture {
            get { return _clientTexture; }
        }

        #endregion

        #region Private variables

        IntPtr _clientInstance;
        Texture _clientTexture;
        CommandBuffer _updateCommand;
        MaterialPropertyBlock _propertyBlock;

        #endregion

        #region MonoBehaviour implementation

        void OnDisable()
        {
            // Stop the client plugin.
            if (_clientInstance != IntPtr.Zero)
            {
                Plugin_DestroyClient(_clientInstance);
                _clientInstance = IntPtr.Zero;
            }

            // Dispose the client texture.
            if (_clientTexture != null)
            {
                if (Application.isPlaying)
                    Destroy(_clientTexture);
                else
                    DestroyImmediate(_clientTexture);
            }
        }

        void OnDestroy()
        {
            // Dispose the internal objects.
            _updateCommand.Dispose();
        }

        void Update()
        {
            // If we have no connection yet, keep trying to connect to the server.
            if (_clientInstance == IntPtr.Zero)
                _clientInstance = Plugin_CreateClient(_serverName, _appName);

            // Break and return if there is no connection at this point.
            if (_clientInstance == IntPtr.Zero) return;

            // Retrieve the native texture pointer from the client.
            var nativeTexture = Plugin_GetClientTexture(_clientInstance);

            // If the texture seems to be changed, release the current texture.
            if (_clientTexture != null &&
                _clientTexture.GetNativeTexturePtr() != nativeTexture)
            {
                Destroy(_clientTexture);
                _clientTexture = null;
            }

            // If the client texture is not present, create a new one.
            if (_clientTexture == null && nativeTexture != IntPtr.Zero)
            {
                _clientTexture = Texture2D.CreateExternalTexture(
                    Plugin_GetClientTextureWidth(_clientInstance),
                    Plugin_GetClientTextureHeight(_clientInstance),
                    TextureFormat.RGBA32, false, false, nativeTexture
                );
            }

            // Issue the plugin update event.
            if (_updateCommand == null) _updateCommand = new CommandBuffer();

            _updateCommand.Clear();
            _updateCommand.IssuePluginEventAndData(
                Plugin_GetClientUpdateCallback(), 0, _clientInstance
            );

            Graphics.ExecuteCommandBuffer(_updateCommand);

            // Break and return if not client texture is ready.
            if (_clientTexture == null) return;

            // Update external objects.
            if (_targetTexture != null)
                Graphics.Blit(_clientTexture, _targetTexture);

            if (_targetRenderer != null)
            {
                if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();
                _targetRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetTexture(_targetMaterialProperty, receivedTexture);
                _targetRenderer.SetPropertyBlock(_propertyBlock);
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

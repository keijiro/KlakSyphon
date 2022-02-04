// KlakSyphon - Syphon plugin for Unity
// https://github.com/keijiro/KlakSyphon

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Klak.Syphon
{
    [ExecuteInEditMode]
    public class SyphonClient : MonoBehaviour
    {
        #region Source settings

        [SerializeField] string _appName;

        public string appName {
            get { return _appName; }
            set {
                if (_appName == value) return;
                _appName = value;
                OnDisable(); // Force reconnection
            }
        }

        [SerializeField] string _serverName;

        public string serverName {
            get { return _serverName; }
            set {
                if (_serverName == value) return;
                _serverName = value;
                OnDisable(); // Force reconnection
            }
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

        [SerializeField] Texture _nullTexture;

        IntPtr _clientInstance;
        Texture _clientTexture;
        MaterialPropertyBlock _propertyBlock;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            SyphonCommon.ApplyCurrentColorSpace();
        }

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
                _clientTexture = null;

                // Cancel texture use in the target renderer.
                if (_targetRenderer != null && _propertyBlock != null)
                {
                    _targetRenderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetTexture(_targetMaterialProperty, _nullTexture);
                    _targetRenderer.SetPropertyBlock(_propertyBlock);
                }
            }
        }

        void Update()
        {
            // If we have no connection yet, keep trying to connect to the server.
            if (_clientInstance == IntPtr.Zero)
                _clientInstance = Plugin_CreateClient(_serverName, _appName);

            // Break and return if there is no connection at this point.
            if (_clientInstance == IntPtr.Zero) return;

            // If the client has been invalidated, destroy it.
            if (!Plugin_IsClientValid(_clientInstance))
            {
                OnDisable();
                return;
            }

            // Update the client.
            Plugin_UpdateClient(_clientInstance);

            // Retrieve the native texture pointer from the client.
            var nativeTexture = Plugin_GetClientTexture(_clientInstance);

            // If the texture seems to be changed, release the current texture.
            if (_clientTexture != null &&
                _clientTexture.GetNativeTexturePtr() != nativeTexture)
            {
                if (Application.isPlaying)
                    Destroy(_clientTexture);
                else
                    DestroyImmediate(_clientTexture);
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
                _clientTexture.wrapMode = TextureWrapMode.Clamp;
            }

            if (_clientTexture != null)
            {
                // Blit to the target render texture.
                if (_targetTexture != null)
                    Graphics.Blit(_clientTexture, _targetTexture);

                // Override the target renderer material.
                if (_targetRenderer != null)
                {
                    if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();
                    _targetRenderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetTexture(_targetMaterialProperty, _clientTexture);
                    _targetRenderer.SetPropertyBlock(_propertyBlock);
                }
            }
            else
            {
                // No texture ready: Cancel material overriding.
                if (_targetRenderer != null && _propertyBlock != null)
                {
                    _targetRenderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetTexture(_targetMaterialProperty, _nullTexture);
                    _targetRenderer.SetPropertyBlock(_propertyBlock);
                }
            }
        }

        #endregion

        #region Native plugin entry points

        [DllImport("KlakSyphon")]
        static extern IntPtr Plugin_CreateClient(string name, string appName);

        [DllImport("KlakSyphon")]
        static extern void Plugin_DestroyClient(IntPtr instance);

        [DllImport("KlakSyphon")]
        static extern bool Plugin_IsClientValid(IntPtr instance);

        [DllImport("KlakSyphon")]
        static extern IntPtr Plugin_GetClientTexture(IntPtr instance);

        [DllImport("KlakSyphon")]
        static extern int Plugin_GetClientTextureWidth(IntPtr instance);

        [DllImport("KlakSyphon")]
        static extern int Plugin_GetClientTextureHeight(IntPtr instance);

        [DllImport("KlakSyphon")]
        static extern void Plugin_UpdateClient(IntPtr instance);

        #endregion
    }
}

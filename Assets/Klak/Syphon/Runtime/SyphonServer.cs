// KlakSyphon - Syphon plugin for Unity
// https://github.com/keijiro/KlakSyphon

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Klak.Syphon
{
    [ExecuteInEditMode]
    public class SyphonServer : MonoBehaviour
    {
        #region Source texture

        [SerializeField] RenderTexture _sourceTexture;

        public RenderTexture sourceTexture {
            get { return _sourceTexture; }
            set { _sourceTexture = value; }
        }

        #endregion

        #region Format option

        [SerializeField] bool _alphaSupport;

        public bool alphaSupport {
            get { return _alphaSupport; }
            set { _alphaSupport = value; }
        }

        #endregion

        #region Internal objects and variables

        IntPtr _serverInstance;
        Texture _serverTexture;
        Material _blitMaterial;
        bool _hasCamera;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            SyphonCommon.ApplyCurrentColorSpace();
        }

        void OnDisable()
        {
            // We prefer disposing the plugin objects here not OnDestroy
            // to make the server disappear from the Syphon server directory.

            if (_serverInstance != IntPtr.Zero)
            {
                Plugin_DestroyServer(_serverInstance);
                _serverInstance = IntPtr.Zero;
            }

            if (_serverTexture != null)
            {
                if (Application.isPlaying)
                    Destroy(_serverTexture);
                else
                    DestroyImmediate(_serverTexture);
            }
        }

        void OnDestroy()
        {
            // Dispose the internal objects.
            if (_blitMaterial != null)
            {
                if (Application.isPlaying)
                    Destroy(_blitMaterial);
                else
                    DestroyImmediate(_blitMaterial);
            }
        }

        void Update()
        {
            // Lazy initialization for the server plugin.
            if (_serverInstance == IntPtr.Zero)
            {
                // Camera capture mode?
                var camera = GetComponent<Camera>();
                _hasCamera = (camera != null);

                // We can do nothing if no source is given.
                if (!_hasCamera && _sourceTexture == null) return;

                var width = _hasCamera ? camera.pixelWidth : _sourceTexture.width;
                var height = _hasCamera ? camera.pixelHeight : _sourceTexture.height;

                // Create the server instance.
                _serverInstance = Plugin_CreateServer(gameObject.name, width, height);

                // Create the server texture as an external 2D texture.
                _serverTexture = Texture2D.CreateExternalTexture(
                    width, height, TextureFormat.RGBA32, false, false,
                    Plugin_GetServerTexture(_serverInstance)
                );
            }

            // Lazy initialization for the internal objects.
            if (_blitMaterial == null)
            {
                _blitMaterial = new Material(Shader.Find("Hidden/Klak/Syphon/Blit"));
                _blitMaterial.hideFlags = HideFlags.DontSave;
            }

            // Render texture mode update
            if (!_hasCamera)
            {
                // Special case: Uninitialize the plugin when the source
                // texture is lost.
                if (_sourceTexture == null)
                {
                    OnDisable();
                    return;
                }

                // Request texture copy.
                Graphics.CopyTexture(_sourceTexture, _serverTexture);
            }

            // Publish the new frame.
            Plugin_PublishServerTexture(_serverInstance);
        }

        void OnRenderImage(RenderTexture source, RenderTexture dest)
        {
            if (_serverTexture != null && _blitMaterial != null)
            {
                // Capture the camera render.
                var temp = RenderTexture.GetTemporary(
                    _serverTexture.width, _serverTexture.height, 0,
                    RenderTextureFormat.Default, RenderTextureReadWrite.Default
                );
                Graphics.Blit(source, temp, _blitMaterial, _alphaSupport ? 1 : 0);
                Graphics.CopyTexture(temp, _serverTexture);
                RenderTexture.ReleaseTemporary(temp);
            }

            // Dumb blit
            Graphics.Blit(source, dest);
        }

        #endregion

        #region Native plugin entry points

        [DllImport("KlakSyphon")]
        static extern IntPtr Plugin_CreateServer(string name, int width, int height);

        [DllImport("KlakSyphon")]
        static extern void Plugin_DestroyServer(IntPtr instance);

        [DllImport("KlakSyphon")]
        static extern IntPtr Plugin_GetServerTexture(IntPtr instance);

        [DllImport("KlakSyphon")]
        static extern void Plugin_PublishServerTexture(IntPtr instance);

        #endregion
    }
}

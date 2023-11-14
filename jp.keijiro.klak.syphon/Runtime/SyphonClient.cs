using UnityEngine;
using UnityEngine.Rendering;
using Klak.Syphon.Interop;

namespace Klak.Syphon
{
    [ExecuteInEditMode]
    public class SyphonClient : MonoBehaviour
    {
        #region Source settings

        [SerializeField] string _serverName;

        public string ServerName {
            get { return _serverName; }
            set {
                if (_serverName == value) return;
                _serverName = value;
                OnDisable(); // Force reconnection
            }
        }

        #endregion

        #region Target settings

        [field:SerializeField] RenderTexture TargetTexture { get; set; }
        [field:SerializeField] Renderer TargetRenderer { get; set; }
        [field:SerializeField] string TargetMaterialProperty { get; set; }

        #endregion

        #region Public properties

        public Texture ReceivedTexture => _clientTexture;

        #endregion

        #region Private variables

        [SerializeField] Texture _nullTexture = null;

        PluginClient _clientInstance;
        Texture _clientTexture;
        MaterialPropertyBlock _propertyBlock;

        #endregion

        #region MonoBehaviour implementation

        void Start()
          => SyphonCommon.ApplyCurrentColorSpace();

        void OnDisable()
        {
            // Stop the client plugin.
            _clientInstance?.Dispose();
            _clientInstance = null;

            // Dispose the client texture.
            if (_clientTexture != null)
            {
                if (Application.isPlaying)
                    Destroy(_clientTexture);
                else
                    DestroyImmediate(_clientTexture);
                _clientTexture = null;

                // Cancel texture use in the target renderer.
                if (TargetRenderer != null && _propertyBlock != null)
                {
                    TargetRenderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetTexture(TargetMaterialProperty, _nullTexture);
                    TargetRenderer.SetPropertyBlock(_propertyBlock);
                }
            }
        }

        void Update()
        {
            // If we have no connection yet, keep trying to connect to the server.
            if (_clientInstance == null)
            {
                var pair = _serverName.Split('/');
                var name = pair[1] == "(no name)" ? "" : pair[1];
                _clientInstance = PluginClient.Create(name, pair[0]);
            }

            // Break and return if there is no connection at this point.
            // If the client has been invalidated, destroy it.
            if (!(_clientInstance?.IsValid ?? false))
            {
                OnDisable();
                return;
            }

            // Update the client.
            _clientInstance.Update();

            // Retrieve the native texture pointer from the client.
            var nativeTexture = _clientInstance.TexturePointer;

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
            if (_clientTexture == null && nativeTexture != System.IntPtr.Zero)
            {
                _clientTexture = Texture2D.CreateExternalTexture(
                    _clientInstance.Width, _clientInstance.Height,
                    TextureFormat.RGBA32, false, false, nativeTexture
                );
                _clientTexture.wrapMode = TextureWrapMode.Clamp;
            }

            if (_clientTexture != null)
            {
                // Blit to the target render texture.
                if (TargetTexture != null)
                    Graphics.Blit(_clientTexture, TargetTexture);

                // Override the target renderer material.
                if (TargetRenderer != null)
                {
                    if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();
                    TargetRenderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetTexture(TargetMaterialProperty, _clientTexture);
                    TargetRenderer.SetPropertyBlock(_propertyBlock);
                }
            }
            else
            {
                // No texture ready: Cancel material overriding.
                if (TargetRenderer != null && _propertyBlock != null)
                {
                    TargetRenderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetTexture(TargetMaterialProperty, _nullTexture);
                    TargetRenderer.SetPropertyBlock(_propertyBlock);
                }
            }
        }

        #endregion
    }
}

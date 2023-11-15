using UnityEngine;
using Plugin = Klak.Syphon.Interop.PluginServer;

namespace Klak.Syphon {

[ExecuteInEditMode]
public sealed class SyphonServer : MonoBehaviour
{
    #region Public properties

    [field:SerializeField] public RenderTexture SourceTexture { get; set; }
    [field:SerializeField] public bool KeepAlpha { get; set; }

    #endregion

    #region Private members

    (Plugin instance, Texture2D texture) _plugin;
    Material _blitMaterial;
    bool _hasCamera;

    #endregion

    #region MonoBehaviour implementation

    void Start()
      => SyphonCommon.ApplyCurrentColorSpace();

    void OnDisable()
    {
        // Server plugin/texture disposal
        _plugin.instance?.Dispose();
        Utility.Destroy(_plugin.texture);
        _plugin = (null, null);

        // Blitter disposal
        Utility.Destroy(_blitMaterial);
        _blitMaterial = null;
    }

    void Update()
    {
        // Server plugin laxy initialization
        if (_plugin.instance == null)
        {
            // Camera capture mode detection
            var camera = GetComponent<Camera>();
            _hasCamera = (camera != null);

            // Input check
            if (!_hasCamera && SourceTexture == null) return;

            var (width, height) = _hasCamera ?
                (camera.pixelWidth, camera.pixelHeight) :
                (SourceTexture.width, SourceTexture.height);

            // Server plugin initialization
            _plugin = Plugin.CreateWithBackedTexture(gameObject.name, width, height);
        }

        // Blitter lazy initialization
        if (_blitMaterial == null)
        {
            _blitMaterial = new Material(Shader.Find("Hidden/Klak/Syphon/Blit"));
            _blitMaterial.hideFlags = HideFlags.DontSave;
        }

        // Render texture mode update
        if (!_hasCamera)
        {
            // Source texture liveness test
            if (SourceTexture == null)
            {
                OnDisable();
                return;
            }

            // Texture copy
            Graphics.CopyTexture(SourceTexture, _plugin.texture);
        }

        // New frame publishing
        _plugin.instance.PublishTexture();
    }

    void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        if (_plugin.texture != null && _blitMaterial != null)
        {
            // Camera render capture
            var temp = RenderTexture.GetTemporary(
                _plugin.texture.width, _plugin.texture.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.Default
            );
            Graphics.Blit(source, temp, _blitMaterial, KeepAlpha ? 1 : 0);
            Graphics.CopyTexture(temp, _plugin.texture);
            RenderTexture.ReleaseTemporary(temp);
        }

        // Dumb blit
        Graphics.Blit(source, dest);
    }

    #endregion
}

} // namespace Klak.Syphon

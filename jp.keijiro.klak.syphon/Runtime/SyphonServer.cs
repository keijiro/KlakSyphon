using UnityEngine;
using UnityEngine.Rendering;
using Plugin = Klak.Syphon.Interop.PluginServer;

namespace Klak.Syphon {

public enum CaptureMethod { GameView, Camera, Texture }

[ExecuteInEditMode]
public sealed class SyphonServer : MonoBehaviour
{
    #region Public properties

    [SerializeField] string _serverName = "Syphon Server";

    public string ServerName
      { get => _serverName;
        set { ResetState(); _serverName = value; } }

    [field:SerializeField] public bool KeepAlpha { get; set; }

    [SerializeField] CaptureMethod _captureMethod;

    public CaptureMethod CaptureMethod
      { get => _captureMethod;
        set { ResetState(); _captureMethod = value; } }

    [SerializeField] Camera _sourceCamera;

    public Camera SourceCamera
      { get => _sourceCamera;
        set { ResetState(); _sourceCamera = value; } }

    [SerializeField] Texture _sourceTexture;

    public Texture SourceTexture
      { get => _sourceTexture;
        set { ResetState(); _sourceTexture = value; } }

    #endregion

    #region Private members

    (Plugin instance, Texture2D texture) _plugin;

    #if KLAK_SYPHON_HAS_SRP
    Camera _attachedCamera;
    #endif

    Material _blitMaterial;

    void ResetState()
    {
        // Server plugin/texture disposal
        _plugin.instance?.Dispose();
        Utility.Destroy(_plugin.texture);
        _plugin = (null, null);

        #if KLAK_SYPHON_HAS_SRP
        // Camera capture callback reset
        if (_attachedCamera != null)
            CameraCaptureBridge.RemoveCaptureAction(_attachedCamera, OnCameraCapture);
        _attachedCamera = null;
        #endif
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
      => InternalCommon.ApplyCurrentColorSpace();

    void OnValidate()
      => ResetState();

    void OnDisable()
      => ResetState();

    void OnDestroy()
    {
        Utility.Destroy(_blitMaterial);
        _blitMaterial = null;
    }

    void Update()
    {
        // Server plugin lazy initialization
        if (_plugin.instance == null)
        {
            if (string.IsNullOrEmpty(_serverName)) return;
            if (_captureMethod == CaptureMethod.Texture)
            {
                if (_sourceTexture == null) return;
                _plugin = Plugin.CreateWithBackedTexture
                  (_serverName, _sourceTexture.width, _sourceTexture.height);
            }
            else if (_captureMethod == CaptureMethod.Camera)
            {
                if (_sourceCamera == null) return;
                _plugin = Plugin.CreateWithBackedTexture
                  (_serverName, _sourceCamera.pixelWidth, _sourceCamera.pixelHeight);
                #if KLAK_SYPHON_HAS_SRP
                // Camera capture callback setup
                CameraCaptureBridge.AddCaptureAction(_sourceCamera, OnCameraCapture);
                _attachedCamera = _sourceCamera;
                #endif
            }
            else // CaptureMethod == CaptureMethod.GameView
            {
                _plugin = Plugin.CreateWithBackedTexture
                  (_serverName, Screen.width, Screen.height);
            }
        }

        // Blitter lazy initialization
        if (_blitMaterial == null)
        {
            _blitMaterial = new Material(Shader.Find("Hidden/Klak/Syphon/Blit"));
            _blitMaterial.hideFlags = HideFlags.DontSave;
        }

        // Texture mode update
        if (_captureMethod == CaptureMethod.Texture)
            Graphics.CopyTexture(_sourceTexture, _plugin.texture);

        // Game View mode update
        if (_captureMethod == CaptureMethod.GameView)
        {
            var rt1 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
            var rt2 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(rt1);
            Graphics.Blit(rt1, rt2, _blitMaterial, KeepAlpha ? 1 : 0);
            Graphics.CopyTexture(rt2, _plugin.texture);
            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
        }

        // Frame update notification
        _plugin.instance.PublishTexture();
    }

    #if KLAK_SYPHON_HAS_SRP
    void OnCameraCapture(RenderTargetIdentifier source, CommandBuffer cb)
    {
        if (_attachedCamera == null) return;
        // TO BE IMPLEMENTED
    }
    #endif

    #endregion
}

} // namespace Klak.Syphon

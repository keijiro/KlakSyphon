using UnityEngine;
using UnityEngine.Rendering;
using Plugin = Klak.Syphon.Interop.PluginServer;

namespace Klak.Syphon {

[ExecuteInEditMode]
public sealed class SyphonServer : MonoBehaviour
{
    #region Public properties

    public string ServerName
      { get => _serverName;
        set { TeardownPlugin(); _serverName = value; } }

    public CaptureMethod CaptureMethod
      { get => _captureMethod;
        set { TeardownPlugin(); _captureMethod = value; } }

    public Camera SourceCamera
      { get => _sourceCamera;
        set { TeardownPlugin(); _sourceCamera = value; } }

    public Texture SourceTexture
      { get => _sourceTexture;
        set { TeardownPlugin(); _sourceTexture = value; } }

    [field:SerializeField] public bool KeepAlpha { get; set; }

    [field:SerializeField] public SyphonResources Resources { get; set; }

    #endregion

    #region Property backing fields

    [SerializeField] string _serverName = "Syphon Server";
    [SerializeField] CaptureMethod _captureMethod;
    [SerializeField] Camera _sourceCamera;
    [SerializeField] Texture _sourceTexture;

    #endregion

    #region Private members

    (Plugin instance, Texture2D texture) _plugin;
    Material _blitMaterial;

    #if KLAK_SYPHON_HAS_SRP
    Camera _attachedCamera;
    #endif

    void SetupPlugin()
    {
        if (_plugin.instance != null) return;

        // Server name validity check
        if (string.IsNullOrEmpty(_serverName)) return;

        // Texture capture mode
        if (_captureMethod == CaptureMethod.Texture)
        {
            if (_sourceTexture == null) return;
            _plugin = Plugin.CreateWithBackedTexture
              (_serverName, _sourceTexture.width, _sourceTexture.height);
        }

        // Camera capture mode
        if (_captureMethod == CaptureMethod.Camera)
        {
            if (_sourceCamera == null) return;
            _plugin = Plugin.CreateWithBackedTexture
              (_serverName, _sourceCamera.pixelWidth, _sourceCamera.pixelHeight);

            #if KLAK_SYPHON_HAS_SRP
            // Callback registration
            CameraCaptureBridge.AddCaptureAction(_sourceCamera, OnCameraCapture);
            _attachedCamera = _sourceCamera;
            #endif
        }

        // Game View capture mode
        if (_captureMethod == CaptureMethod.GameView)
            _plugin = Plugin.CreateWithBackedTexture
              (_serverName, Screen.width, Screen.height);
    }

    void TeardownPlugin()
    {
        // Plugin instance/texture disposal
        _plugin.instance?.Dispose();
        Utility.Destroy(_plugin.texture);
        _plugin = (null, null);

        #if KLAK_SYPHON_HAS_SRP
        // Camera capture callback reset
        if (_attachedCamera != null)
        {
            CameraCaptureBridge.RemoveCaptureAction(_attachedCamera, OnCameraCapture);
            _attachedCamera = null;
        }
        #endif
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
      => InternalCommon.ApplyCurrentColorSpace();

    void OnValidate()
      => TeardownPlugin();

    void OnDisable()
      => TeardownPlugin();

    void OnDestroy()
    {
        Utility.Destroy(_blitMaterial);
        _blitMaterial = null;
    }

    void Update()
    {
        SetupPlugin();

        if (_plugin.instance == null) return;

        // Blitter lazy initialization
        if (_blitMaterial == null)
        {
            _blitMaterial = new Material(Resources.blitShader);
            _blitMaterial.hideFlags = HideFlags.DontSave;
        }

        // Texture capture mode update
        if (_captureMethod == CaptureMethod.Texture)
        {
            var rt = RenderTexture.GetTemporary(_sourceTexture.width, _sourceTexture.height, 0);
            Graphics.Blit(_sourceTexture, rt, _blitMaterial, KeepAlpha ? 1 : 0);
            Graphics.CopyTexture(rt, _plugin.texture);
            RenderTexture.ReleaseTemporary(rt);
        }

        if (_attachedCamera != null && _attachedCamera.targetTexture != null)
        {
            var tex = _attachedCamera.targetTexture;
            var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0);
            Graphics.Blit(tex, rt, _blitMaterial, KeepAlpha ? 1 : 0);
            Graphics.CopyTexture(rt, _plugin.texture);
            RenderTexture.ReleaseTemporary(rt);
        }

        // Game View capture mode update
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
        if (_attachedCamera == null || _plugin.texture == null) return;
        if (_attachedCamera.targetTexture != null) return;
        var rtID = Shader.PropertyToID("SyphonTemp");
        cb.GetTemporaryRT(rtID, _attachedCamera.pixelWidth, _attachedCamera.pixelHeight, 0);
        cb.Blit(source, rtID, _blitMaterial, KeepAlpha ? 1 : 0);
        cb.CopyTexture(rtID, _plugin.texture);
        cb.ReleaseTemporaryRT(rtID);
    }
    #endif

    #endregion
}

} // namespace Klak.Syphon

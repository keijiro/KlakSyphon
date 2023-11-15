using UnityEngine;
using Plugin = Klak.Syphon.Interop.PluginClient;

namespace Klak.Syphon {

[ExecuteInEditMode]
public sealed class SyphonClient : MonoBehaviour
{
    #region Public properties

    [field:SerializeField] public string ServerName { get; set; }
    [field:SerializeField] public RenderTexture TargetTexture { get; set; }
    [field:SerializeField] public Renderer TargetRenderer { get; set; }
    [field:SerializeField] public string TargetMaterialProperty { get; set; }
    public Texture Texture => _texture;

    #endregion

    #region Private members

    (Plugin instance, string name) _plugin;
    Texture2D _texture;

    #endregion

    #region MonoBehaviour implementation

    void Start()
      => InternalCommon.ApplyCurrentColorSpace();

    void OnDisable()
    {
        // Client plugin disposal
        _plugin.instance?.Dispose();
        _plugin = (null, null);

        // Texture disposal
        Utility.Destroy(_texture);
        _texture = null;

        // Renderer reset
        Utility.SetTexture(TargetRenderer, TargetMaterialProperty, null);
    }

    void Update()
    {
        // Plugin invalidation on name changes
        if (_plugin.name != ServerName) OnDisable();

        // Plugin lazy initialization
        if (_plugin.instance == null)
            _plugin = (Plugin.Create(ServerName), ServerName);

        // Plugin lazy disposal
        if (_plugin.instance == null || !_plugin.instance.IsValid)
        {
            OnDisable();
            return;
        }

        // Plugin update
        _plugin.instance.Update();

        // Plugin texture invalidation on plugin-side changes
        if (!_plugin.instance.HasSameTexture(_texture))
        {
            Utility.Destroy(_texture);
            _texture = null;
        }

        // Plugin texture lazy initialization
        if (_texture == null) _texture = _plugin.instance.CreateTexture();

        // Target texture blit
        if (_texture != null && TargetTexture != null)
            Graphics.Blit(_texture, TargetTexture);

        // Renderer override
        Utility.SetTexture(TargetRenderer, TargetMaterialProperty, _texture);
    }

    #endregion
}

} // namespace Klak.Syphon

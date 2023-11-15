using UnityEngine;

namespace Klak.Syphon {

static class Utility
{
    #region Object lifecycle

    public static void Destroy(Object obj)
    {
        if (obj == null) return;
        if (Application.isPlaying)
            Object.Destroy(obj);
        else
            Object.DestroyImmediate(obj);
    }

    #endregion

    #region Material property

    static MaterialPropertyBlock _sharedPropertyBlock;
    static Texture2D _blackTexture;

    static MaterialPropertyBlock GetSharedPropertyBlock()
    {
        if (_sharedPropertyBlock == null)
            _sharedPropertyBlock = new MaterialPropertyBlock();
        return _sharedPropertyBlock;
    }

    static Texture2D GetBlackTexture()
    {
        if (_blackTexture == null) _blackTexture = new Texture2D(8, 8);
        return _blackTexture;
    }

    public static void SetTexture(Renderer renderer, string name, Texture texture)
    {
        if (renderer == null || string.IsNullOrEmpty(name)) return;
        var sheet = GetSharedPropertyBlock();
        renderer.GetPropertyBlock(sheet);
        sheet.SetTexture(name, texture != null ? texture : GetBlackTexture());
        renderer.SetPropertyBlock(sheet);
    }

    #endregion
}

} // namespace Klak.Syphon

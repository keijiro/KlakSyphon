using UnityEngine;

namespace Klak.Syphon {

static class Utility
{
    #region Object lifecycle

    public static void Destroy(Object obj)
    {
        if (obj != null)
        {
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }
    }

    #endregion

    #region Syphon name

    public static (string app, string server) SplitName(string name)
    {
        if (string.IsNullOrEmpty(name)) return (null, null);
        var pair = name.Split('/');
        if (pair.Length < 2 || pair[1] == "(no name)") return (pair[0], null);
        return (pair[0], pair[1]);
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
        if (_blackTexture == null)
            _blackTexture = new Texture2D(8, 8);
        return _blackTexture;
    }

    public static void
      SetTexture(Renderer renderer, string name, Texture texture)
    {
        if (renderer == null || string.IsNullOrEmpty(name)) return;
        var sheet = GetSharedPropertyBlock();
        renderer.GetPropertyBlock(sheet);
        sheet.SetTexture(name, texture != null ? texture : GetBlackTexture());
        renderer.SetPropertyBlock(sheet);
    }

    #endregion

    #region Common texture


    #endregion
}

} // namespace Klak.Syphon

using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace Klak.Syphon {

static class Utility
{
    #region Object lifecycle

    public static void Destroy(UnityEngine.Object obj)
    {
        if (obj == null) return;
        if (Application.isPlaying)
            UnityEngine.Object.Destroy(obj);
        else
            UnityEngine.Object.DestroyImmediate(obj);
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

static class Blitter
{
    static Material _material;

    public static void Prepare(SyphonResources resources)
    {
        if (_material != null) return;
        _material =  new Material(resources.blitShader);
        _material.hideFlags = HideFlags.DontSave;
    }

    public static void Blit(Texture source, Texture destination, bool keepAlpha)
    {
        var rt = RenderTexture.GetTemporary(source.width, source.height, 0);
        Graphics.Blit(source, rt, _material, keepAlpha ? 1 : 0);
        Graphics.CopyTexture(rt, destination);
        RenderTexture.ReleaseTemporary(rt);
    }

    public static void Blit(CommandBuffer cb, RenderTargetIdentifier source,
              Texture destination, bool keepAlpha,
              int sourceWidth, int sourceHeight)
    {
        var rtID = Shader.PropertyToID("SyphonTemp");
        cb.GetTemporaryRT(rtID, sourceWidth, sourceHeight, 0);
        cb.Blit(source, rtID, _material, keepAlpha ? 1 : 0);
        cb.CopyTexture(rtID, destination);
        cb.ReleaseTemporaryRT(rtID);
    }
}

} // namespace Klak.Syphon

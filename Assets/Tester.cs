using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class Tester : MonoBehaviour
{
    [DllImport("KlakSyphon")]
    private static extern IntPtr Klak_GetCallback();

    [DllImport("KlakSyphon")]
    private static extern IntPtr Klak_CreateServer(string name, int width, int height, IntPtr texture);

    [DllImport("KlakSyphon")]
    private static extern void Klak_DestroyServer(IntPtr instance);

    [SerializeField] Material _material;

    RenderTexture _texture;
    IntPtr _plugin;

    CommandBuffer _command1;
    CommandBuffer _command2;

    void Start()
    {
        _texture = new RenderTexture(512, 512, 24);
        _texture.autoGenerateMips = false;
        _texture.Create();

        _plugin = Klak_CreateServer("Test", _texture.width, _texture.height, _texture.GetNativeTexturePtr());

        _command1 = new CommandBuffer();
        _command1.IssuePluginEventAndData(Klak_GetCallback(), 0, _plugin);

        _command2 = new CommandBuffer();
        _command2.IssuePluginEventAndData(Klak_GetCallback(), 1, _plugin);
    }

    void OnDestroy()
    {
        Destroy(_texture);
        Klak_DestroyServer(_plugin);
    }

    void Update()
    {
    }

    void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        Graphics.SetRenderTarget(_texture);
        Graphics.Blit(source, _material, 0);
        Graphics.ExecuteCommandBuffer(_command1);

        Graphics.Blit(source, dest);
    }
}

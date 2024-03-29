using UnityEngine;
using Klak.Syphon;

class ServerBenchmark : MonoBehaviour
{
    [SerializeField] SyphonResources _resources = null;

    RenderTexture _targetRT;
    GameObject[] _instances = new GameObject[16];

    System.Collections.IEnumerator Start()
    {
        _targetRT = new RenderTexture(256, 256, 32);

        for (var index = 0; index < 16; index++)
            _instances[index] = CreateInstance(index);

        var interval = new WaitForSeconds(0.3f);

        while (true)
        {
            var index = Random.Range(0, 15);

            if (_instances[index] == null)
            {
                _instances[index] = CreateInstance(index);
            }
            else
            {
                Destroy(_instances[index]);
                _instances[index] = null;
            }

            yield return interval;
        }
    }

    GameObject CreateInstance(int index)
    {
        var components = new [] { typeof(Camera), typeof(SyphonServer) };

        var go = new GameObject($"Server {index}", components);

        var x = (index % 4 + 0.5f) / 4 - 0.5f;
        var y = (index / 4 + 0.5f) / 4 - 0.5f;

        go.transform.parent = transform;
        go.transform.localPosition = new Vector3(x, y, -10);

        var camera = go.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 0.5f / 4;
        camera.targetTexture = _targetRT;

        var server = go.GetComponent<SyphonServer>();
        server.Resources = _resources;
        server.ServerName = go.name;
        server.KeepAlpha = true;
        server.CaptureMethod = CaptureMethod.Camera;
        server.SourceCamera = camera;

        return go;
    }
}

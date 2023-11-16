using UnityEngine;
using UI = UnityEngine.UI;
using Klak.Syphon;

public sealed class SetClientImage : MonoBehaviour
{
    [SerializeField] SyphonClient _client = null;

    void Update()
      => GetComponent<UI.RawImage>().texture = _client.Texture;
}

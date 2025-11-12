using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;
using Klak.Syphon;

public sealed class SourceSelector : MonoBehaviour
{
    [SerializeField] SyphonClient _client = null;

    [CreateProperty]
    public List<string> SourceList => SyphonServerDirectory.ServerNames.ToList();

    VisualElement UIRoot
      => GetComponent<UIDocument>().rootVisualElement;

    DropdownField UISelector
      => UIRoot.Q<DropdownField>("source-selector");

    void Start()
    {
        UISelector.dataSource = this;
        UISelector.RegisterValueChangedCallback
          (evt => _client.ServerName = evt.newValue);
    }
}

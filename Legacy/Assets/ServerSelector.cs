using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Klak.Syphon;

public class ServerSelector : MonoBehaviour
{
    [SerializeField] Dropdown _dropdown = null;

    SyphonClient _client;
    List<string> _serverNames;
    bool _disableCallback;

    // HACK: Assuming that the dropdown has more than
    // three child objects only while it's opened.
    bool IsOpened => _dropdown.transform.childCount > 3;

    void Start() => _client = GetComponent<SyphonClient>();

    void Update()
    {
        // Do nothing if the menu is opened.
        if (IsOpened) return;

        // Syphon server name retrieval
        _serverNames = SyphonServerDirectory.ServerNames.ToList();

        // Currect selection
        var index = _serverNames.IndexOf(_client.ServerName);

        // Append the current name to the list if it's not found.
        if (index < 0)
        {
            index = _serverNames.Count;
            _serverNames.Add(_client.ServerName);
        }

        // Disable the callback while updating the menu options.
        _disableCallback = true;

        // Menu option update
        _dropdown.ClearOptions();
        _dropdown.AddOptions(_serverNames);
        _dropdown.value = index;
        _dropdown.RefreshShownValue();

        // Resume the callback.
        _disableCallback = false;
    }

    public void OnChangeValue(int value)
    {
        if (_disableCallback) return;
        _client.ServerName = _serverNames[value];
    }
}

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using ServerList = Klak.Syphon.Interop.ServerList;

namespace Klak.Syphon {

[CanEditMultipleObjects]
[CustomEditor(typeof(SyphonClient))]
public class SyphonClientEditor : Editor
{
    static class Labels
    {
        public static Label Property = "Property";
        public static Label Select = "Select";
    }

    #pragma warning disable CS0649

    AutoProperty ServerName;
    AutoProperty TargetTexture;
    AutoProperty TargetRenderer;
    AutoProperty TargetMaterialProperty;

    #pragma warning restore

    // Server name dropdown
    void ShowServerNameDropdown(Rect rect)
    {
        var menu = new GenericMenu();
        using var list = ServerList.Create();

        foreach (var name in list.GetCombinedNameArray())
            menu.AddItem(new GUIContent(name), false, OnSelectName, name);

        if (list.Count == 0)
            menu.AddItem(new GUIContent("No server available"), false, null);

        menu.DropDown(rect);
    }

    // NDI source name selection callback
    void OnSelectName(object name)
    {
        serializedObject.Update();
        ServerName.Target.stringValue = (string)name;
        serializedObject.ApplyModifiedProperties();
    }

    void OnEnable() => AutoProperty.Scan(this);

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();

        // Server name
        EditorGUILayout.DelayedTextField(ServerName);

        // Server name dropdown
        var rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(60));
        if (EditorGUI.DropdownButton(rect, Labels.Select, FocusType.Keyboard))
            ShowServerNameDropdown(rect);

        EditorGUILayout.EndHorizontal();

        // Force reconnection on modification to name properties.
        if (EditorGUI.EndChangeCheck())
            foreach (MonoBehaviour client in targets)
                client.SendMessage("OnDisable");

        // Target Texture/Renderer
        EditorGUILayout.PropertyField(TargetTexture);
        EditorGUILayout.PropertyField(TargetRenderer);

        EditorGUI.indentLevel++;

        if (TargetRenderer.Target.hasMultipleDifferentValues)
        {
            // Multiple renderers selected: Show the simple text field.
            EditorGUILayout.
              PropertyField(TargetMaterialProperty, Labels.Property);
        }
        else if (TargetRenderer.Target.objectReferenceValue != null)
        {
            // Single renderer: Show the material property selection dropdown.
            MaterialPropertySelector.
              DropdownList(TargetRenderer, TargetMaterialProperty);
        }

        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }
}

} // namespace Klak.Syphon

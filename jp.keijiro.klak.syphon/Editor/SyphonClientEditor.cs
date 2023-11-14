using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;

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

        var list = Plugin_CreateServerList();
        var count = Plugin_GetServerListCount(list);

        if (count > 0)
        {
            for (var i = 0; i < count; i++)
            {
                var pName = Plugin_GetNameFromServerList(list, i);
                var pAppName = Plugin_GetAppNameFromServerList(list, i);

                var name = (pName != IntPtr.Zero) ? Marshal.PtrToStringAnsi(pName) : "(no name)";
                var appName = (pAppName != IntPtr.Zero) ? Marshal.PtrToStringAnsi(pAppName) : "(no app name)";

                var text = $"{appName}/{name}";
                menu.AddItem(new GUIContent(text), false, OnSelectName, text);
            }
        }
        else
        {
            menu.AddItem(new GUIContent("No source available"), false, null);
        }

        Plugin_DestroyServerList(list);

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

    #region Native plugin entry points

    [DllImport("KlakSyphon")]
    static extern IntPtr Plugin_CreateServerList();

    [DllImport("KlakSyphon")]
    static extern void Plugin_DestroyServerList(IntPtr list);

    [DllImport("KlakSyphon")]
    static extern int Plugin_GetServerListCount(IntPtr list);

    [DllImport("KlakSyphon")]
    static extern IntPtr Plugin_GetNameFromServerList(IntPtr list, int index);

    [DllImport("KlakSyphon")]
    static extern IntPtr Plugin_GetAppNameFromServerList(IntPtr list, int index);

    #endregion
}

} // namespace Klak.Syphon

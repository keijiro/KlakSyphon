using UnityEngine;
using UnityEditor;
using ServerList = Klak.Syphon.Interop.ServerList;

namespace Klak.Syphon {

[CanEditMultipleObjects]
[CustomEditor(typeof(SyphonClient))]
public class SyphonClientEditor : Editor
{
    #region Private members

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

    #endregion

    #region Server name dropdown

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

    void OnSelectName(object name)
    {
        serializedObject.Update();
        ServerName.Target.stringValue = (string)name;
        serializedObject.ApplyModifiedProperties();
    }

    #endregion

    #region Editor implementation

    void OnEnable() => AutoProperty.Scan(this);

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Server name selector
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.DelayedTextField(ServerName);
        var rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(60));
        if (EditorGUI.DropdownButton(rect, Labels.Select, FocusType.Keyboard))
            ShowServerNameDropdown(rect);
        EditorGUILayout.EndHorizontal();

        // Target Texture/Renderer
        EditorGUILayout.PropertyField(TargetTexture);
        EditorGUILayout.PropertyField(TargetRenderer);

        // Material property (text field or dropdown)
        EditorGUI.indentLevel++;
        if (TargetRenderer.Target.hasMultipleDifferentValues)
            EditorGUILayout.PropertyField(TargetMaterialProperty, Labels.Property);
        else if (TargetRenderer.Target.objectReferenceValue != null)
            MaterialPropertySelector.DropdownList(TargetRenderer, TargetMaterialProperty);
        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }

    #endregion
}

} // namespace Klak.Syphon

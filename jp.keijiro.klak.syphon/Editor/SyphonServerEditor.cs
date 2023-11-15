using UnityEngine;
using UnityEditor;

namespace Klak.Syphon {

[CustomEditor(typeof(SyphonServer))]
public class SyphonServerEditor : Editor
{
    #region Private members

    #pragma warning disable CS0649

    AutoProperty SourceTexture;
    AutoProperty KeepAlpha;

    #pragma warning restore

    static class Labels
    {
        public static Label CameraCapture =
          "Syphon Server is running in camera capture mode.";
        public static Label RenderTexture =
          "Syphon Server is running in render texture mode.";
    }

    #endregion

    #region Editor implementation

    void OnEnable() => AutoProperty.Scan(this);

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var server = (SyphonServer)target;
        var camera = server.GetComponent<Camera>();

        if (camera != null)
        {
            EditorGUILayout.HelpBox(Labels.CameraCapture);
            EditorGUILayout.PropertyField(KeepAlpha);
        }
        else
        {
            EditorGUILayout.HelpBox(Labels.RenderTexture);
            EditorGUILayout.PropertyField(SourceTexture);
        }

        serializedObject.ApplyModifiedProperties();
    }

    #endregion
}

} // namespace Klak.Syphon

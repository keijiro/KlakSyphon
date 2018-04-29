// KlakSyphon - Syphon plugin for Unity
// https://github.com/keijiro/KlakSyphon

using UnityEngine;
using UnityEditor;

namespace Klak.Syphon
{
    [CustomEditor(typeof(SyphonServer))]
    public class SyphonServerEditor : Editor
    {
        SerializedProperty _sourceTexture;
        SerializedProperty _alphaSupport;

        void OnEnable()
        {
            _sourceTexture = serializedObject.FindProperty("_sourceTexture");
            _alphaSupport = serializedObject.FindProperty("_alphaSupport");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var server = (SyphonServer)target;
            var camera = server.GetComponent<Camera>();

            if (camera != null)
            {
                EditorGUILayout.HelpBox(
                    "Syphon Server is running in camera capture mode.",
                    MessageType.None
                );

                EditorGUILayout.PropertyField(_alphaSupport);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Syphon Server is running in render texture mode.",
                    MessageType.None
                );

                EditorGUILayout.PropertyField(_sourceTexture);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

// KlakSyphon - Syphon plugin for Unity
// https://github.com/keijiro/KlakSyphon

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;

namespace Klak.Syphon
{
    // Syphon server list window
    public class SyphonServerListWindow : EditorWindow
    {
        #region Menu item

        [MenuItem("Window/Klak/Syphon Server List")]
        static void Init()
        {
            EditorWindow.GetWindow<SyphonServerListWindow>("Syphon Servers").Show();
        }

        #endregion

        #region EditorWindow implementation

        int _updateCount;

        void OnInspectorUpdate()
        {
            // Update once per eight calls.
            if ((_updateCount++ & 7) == 0) Repaint();
        }

        void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;

            var list = Plugin_CreateServerList();
            var count = Plugin_GetServerListCount(list);

            if (count == 0)
                EditorGUILayout.LabelField("No server found.");
            else
                EditorGUILayout.LabelField(count + " server(s) found.");

            for (var i = 0; i < count; i++)
            {
                var pName = Plugin_GetNameFromServerList(list, i);
                var pAppName = Plugin_GetAppNameFromServerList(list, i);

                var name = (pName != IntPtr.Zero) ? Marshal.PtrToStringAnsi(pName) : "(no name)";
                var appName = (pAppName != IntPtr.Zero) ? Marshal.PtrToStringAnsi(pAppName) : "(no app name)";

                EditorGUILayout.LabelField(String.Format("- {0} / {1}", appName, name));
            }

            Plugin_DestroyServerList(list);

            EditorGUI.indentLevel--;
        }

        #endregion

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
}

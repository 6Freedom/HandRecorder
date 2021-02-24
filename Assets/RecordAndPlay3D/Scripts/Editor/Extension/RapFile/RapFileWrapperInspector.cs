using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EliCDavis.RecordAndPlay.Editor.Extension.RapFile
{
    [CustomEditor(typeof(RapFileWrapper))]
    public class RapFileWrapperInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            RapFileWrapper rapTarget = (RapFileWrapper)target;

            GUILayout.Label(string.Format("Viewing: {0}", rapTarget.fileName));
            GUILayout.Label(string.Format("Number of Recordings: {0}", rapTarget.NumberOfRecordings().ToString()));
            if (GUILayout.Button("Show In Explorer"))
            {
                EditorUtility.RevealInFinder(rapTarget.fileName);
            }
            if (GUILayout.Button("Import Into Project"))
            {
                ImportWindow.Init(rapTarget.fileName);
            }
        }
    }
}

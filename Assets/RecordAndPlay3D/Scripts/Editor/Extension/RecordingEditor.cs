using UnityEngine;
using UnityEditor;

using System.Text;
using System.IO;

using EliCDavis.RecordAndPlay.IO;

namespace EliCDavis.RecordAndPlay.Editor.Extension
{

    [CustomEditor(typeof(Recording))]
    [CanEditMultipleObjects]
    public class RecordingEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (targets == null)
            {
                return;
            }

            if (targets.Length == 1)
            {
                RenderSingleRecordingView((Recording)target);
            }
            else if (targets.Length > 1)
            {
                Recording[] recordings = new Recording[targets.Length];
                for (int i = 0; i < targets.Length; i++)
                {
                    recordings[i] = (Recording)targets[i];
                }
                RenderMultipleRecordingsView(recordings);
            }
        }

        public void RenderSingleRecordingView(Recording recording)
        {
            EditorUtil
                .RenderSingleRecordingInfo(recording);

            if (GUILayout.Button("View Playback"))
            {
                PlaybackWindow
                    .Init()
                    .SetRecordingForPlayback(recording);
            }

            if (GUILayout.Button("Export As JSON"))
            {
                string path = EditorUtility.SaveFilePanel("Export Recording As JSON", "", recording.RecordingName + ".json", "json");
                System.IO.File.WriteAllText(path, recording.ToJSON());
                EditorUtility.RevealInFinder(path);
            }

            if (GUILayout.Button("Export As CSV"))
            {
                ExportCSVWindow.Init(recording);
            }

            if (GUILayout.Button("Export As RAP"))
            {
                string path = EditorUtility.SaveFilePanel("Export Recording As RAP", "", recording.RecordingName + ".rap", "rap");
                using(FileStream fs = File.Create(path))
                {
                    Packager.Package(fs, recording);
                }
                EditorUtility.RevealInFinder(path);
            }

            if (GUILayout.Button("Export As Unity Animation Clip"))
            {
                ExportAnimationClipWindow.Init(recording);
            }

            if (recording.Metadata != null && recording.Metadata.Count > 0)
            {
                EditorGUILayout.LabelField("Metadata:", new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold,
                });
                foreach (var keyvaluePair in recording.Metadata)
                {
                    EditorGUILayout.LabelField(keyvaluePair.Key, keyvaluePair.Value);
                }
            }
        }

        public void RenderMultipleRecordingsView(Recording[] recordings)
        {
             EditorUtil
                .RenderMultipleRecordingsInfo(recordings);

            if (GUILayout.Button("Export All As JSON"))
            {
                string path = EditorUtility.SaveFilePanel("Export Recordings As JSON", "", "", "json");

                StringBuilder json = new StringBuilder("[");
                for (var i = 0; i < recordings.Length; i++)
                {
                    json.Append(recordings[i].ToJSON());
                    if (i < recordings.Length - 1)
                    {
                        json.Append(",");
                    }
                }

                json.Append("]");

                System.IO.File.WriteAllText(path, json.ToString());
            }

            if (GUILayout.Button("Export All As CSV"))
            {
                ExportCSVWindow.Init(recordings);
            }

            if (GUILayout.Button("Export As RAP"))
            {
                string path = EditorUtility.SaveFilePanel("Export Recording As RAP", "", "", "rap");
                using (FileStream fs = File.Create(path))
                {
                    Packager.Package(fs, recordings);
                }
                EditorUtility.RevealInFinder(path);
            }
        }

    }

}
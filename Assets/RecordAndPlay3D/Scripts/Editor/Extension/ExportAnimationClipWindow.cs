using UnityEngine;
using UnityEditor;

using System.IO;


namespace EliCDavis.RecordAndPlay.Editor.Extension
{

    public class ExportAnimationClipWindow : EditorWindow
    {

        Recording recordingToExport;

        string folderName = "";

        bool smooth = false;

        public static ExportAnimationClipWindow Init(Recording recordingToExport)
        {
            ExportAnimationClipWindow window = (ExportAnimationClipWindow)GetWindow(typeof(ExportAnimationClipWindow));
            window.recordingToExport = recordingToExport;
            window.folderName = string.IsNullOrEmpty(recordingToExport.RecordingName) ?  "AnimationClips" : recordingToExport.RecordingName;
            window.Show();
            window.Repaint();
            return window;
        }

        void OnGUI()
        {
            if (recordingToExport == null)
            {
                return;
            }

            EditorGUILayout.LabelField("Recording:", recordingToExport.RecordingName);

            folderName = EditorGUILayout.TextField("Folder Name", folderName);

            smooth = EditorGUILayout.Toggle("Smooth", smooth);

            if(smooth)
            {
                EditorGUILayout.HelpBox("Choosing to smooth the animation over frames will cause the object to be animated to positions and rotations that where never recorded for the sake of smoothness. These inacuracies are most noticable in recordings where the object stays still for long periods of time before finally moving. If you value accuracy in playback, do not smooth the anmiation.", MessageType.Warning);
            }

            string error = Error();

            if (error != "")
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
                return;
            }

            if (GUILayout.Button("Export"))
            {

                Export(string.Format("Assets/{0}", folderName), recordingToExport);
            }
        }

        void Export(string path, Recording recording)
        {
            Directory.CreateDirectory(path);
            var clips = RecordingUtil.GetAnimationClips(recording, smooth);
            foreach(var clip in clips)
            {
                var uniquePath = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.anim", path, clip.name));
                AssetDatabase.CreateAsset(clip, uniquePath);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Export Animation Clips");
        }

        private string Error()
        {
            if (string.IsNullOrEmpty(folderName))
            {
                return "Please provide a name to represent the animation clips";
            }

            if (Directory.Exists(string.Format("Assets/{0}", folderName)))
            {
                return "Folder name is already taken";
            }

            return "";
        }
    }
}

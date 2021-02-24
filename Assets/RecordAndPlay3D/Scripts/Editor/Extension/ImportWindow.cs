using UnityEngine;
using UnityEditor;
using System.IO;
using EliCDavis.RecordAndPlay.IO;

namespace EliCDavis.RecordAndPlay.Editor.Extension
{

    /// <summary>
    /// Window for assisting in the importing of recordings from binary format.
    /// </summary>
    public class ImportWindow : EditorWindow
    {
        public static ImportWindow Init(string filePath)
        {
            ImportWindow window = (ImportWindow)GetWindow(typeof(ImportWindow));
            if(filePath != "")
            {
                window.InitLoadSelection(filePath);
            }
            window.Show();
            window.Repaint();
            return window;
        }


        [MenuItem("Window/Record And Play/Import Recordings")]
        public static ImportWindow Init()
        {
            return Init("");
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Import Recordings");
        }

        class LoadSelection
        {
            string path;

            int numberRecordings;

            string error;

            public LoadSelection(string path)
            {
                this.path = path;
                error = "";
                try
                {
                    using (FileStream fs = File.OpenRead(path))
                    {
                        numberRecordings = Unpackager.Peak(fs);
                    }
                }
                catch (System.Exception e)
                {
                    error = e.ToString();
                    numberRecordings = -1;
                }
            }

            public string FileName()
            {
                return Path.GetFileNameWithoutExtension(path);
            }

            public void Render()
            {
                if (HasError())
                {
                    EditorGUILayout.LabelField(error, new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.UpperCenter,
                        fontStyle = FontStyle.Bold
                    });
                }
                else
                {
                    EditorGUILayout.LabelField("File", path);
                    EditorGUILayout.LabelField("Recordings", numberRecordings.ToString());
                }
            }

            public bool HasError()
            {
                return error != "";
            }

            public void Load(string dir)
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();

                using (FileStream fs = File.OpenRead(path))
                {
                    var recordings = Unpackager.Unpackage(fs);
                    foreach (var record in recordings)
                    {
                        RecordingUtil.SaveToAssets(record, record.RecordingName, dir);
                    }
                }
            }
        }

        LoadSelection loadSelection = null;

        string folderName = "";

        private void InitLoadSelection(string file)
        {
            loadSelection = new LoadSelection(file);
            folderName = loadSelection.FileName();
        }

        void OnGUI()
        {
            if (GUILayout.Button((loadSelection == null ? "Select" : "Change") + " Recordings To Load"))
            {
                InitLoadSelection(EditorUtility.OpenFilePanelWithFilters("Load Recordings", "", new string[] { "FileType", "rap" }));
            }

            if (loadSelection == null)
            {
                return;
            }

            loadSelection.Render();

            if (loadSelection.HasError() == false)
            {
                folderName = EditorGUILayout.TextField("Folder Name", folderName);

                string error = Error();
                if (error != "")
                {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
                else if (GUILayout.Button("Import"))
                {
                    loadSelection.Load(Path.Combine("Assets", folderName));
                }
            }

        }

        private string Error()
        {
             if (folderName == "")
            {
                return "Please provide a folder name to store the recordings";
            }

            if (Directory.Exists(Path.Combine("Assets", folderName)))
            {
                return "Folder name is already taken";
            }

            return "";
        }

    }

}
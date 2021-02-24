using UnityEngine;
using UnityEditor;

using System.IO;
using System.Text;

using EliCDavis.RecordAndPlay.Util;

namespace EliCDavis.RecordAndPlay.Editor.Extension
{

    /// <summary>
    /// Window for assisting in the exportation of recordings to csv format.
    /// </summary>
    public class ExportCSVWindow : EditorWindow
    {
        Recording[] recordingsToExport;

        string path = "";

        string folderName = "";

        public static ExportCSVWindow Init(params Recording[] recordingToExport)
        {
            ExportCSVWindow window = (ExportCSVWindow)GetWindow(typeof(ExportCSVWindow));
            window.recordingsToExport = recordingToExport;
            window.folderName = recordingToExport.Length == 1 ? recordingToExport[0].name : "Recordings";
            window.Show();
            window.Repaint();
            return window;
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Export CSV");
        }

        void OnGUI()
        {
            if (recordingsToExport == null || recordingsToExport.Length == 0)
            {
                return;
            }

            if (path != "")
            {
                EditorGUILayout.LabelField("Location", path);
            }

            if (GUILayout.Button("Select Location To Save Recordings"))
            {
                path = EditorUtility.SaveFolderPanel("Export Recording As CSV", "", "");
            }

            if (path != "")
            {
                folderName = EditorGUILayout.TextField("Folder Name", folderName);
            }

            string error = Error();

            if (error != "")
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
                return;
            }

            if (GUILayout.Button("Export"))
            {
                Export(Path.Combine(path, folderName), recordingsToExport);
            }
        }

        private void Export(string dir, Recording[] recordings)
        {
            if (recordings == null || recordings.Length == 0)
            {
                throw new System.Exception("Can't export null or empty recordings.");
            }

            Directory.CreateDirectory(dir);

            if (recordings.Length == 1)
            {
                ExportSingle(dir, recordings[0]);
            }
            else
            {
                ExportMultiple(dir, recordings);
            }
        }

        private void ExportSingle(string dir, Recording recording)
        {
            StringBuilder subjects = new StringBuilder("ID, Name\n");
            foreach (var rec in recording.SubjectRecordings)
            {
                subjects.AppendFormat(
                    "{0}, {1}\n",
                    rec.SubjectID,
                    FormattingUtil.StringToCSVCell(rec.SubjectName)
                );
            }

            StringBuilder customEvents = new StringBuilder("SubjectID, Time, Name, Contents\n");
            foreach (var rec in recording.SubjectRecordings)
            {
                foreach (var e in rec.CapturedCustomEvents)
                {
                    customEvents.AppendFormat(
                        "{0}, {1}\n",
                        rec.SubjectID,
                        e.ToCSV()
                    );
                }
            }
            foreach (var e in recording.CapturedCustomEvents)
            {
                customEvents.AppendFormat(
                    "{0}, {1}\n",
                    -1,
                    e.ToCSV()
                );
            }

            StringBuilder positionData = new StringBuilder("SubjectID, Time, X, Y, Z\n");
            foreach (var rec in recording.SubjectRecordings)
            {
                foreach (var e in rec.CapturedPositions)
                {
                    positionData.AppendFormat(
                        "{0}, {1}\n",
                        rec.SubjectID,
                        e.ToCSV()
                    );
                }
            }

            StringBuilder rotationData = new StringBuilder("SubjectID, Time, X, Y, Z\n");
            foreach (var rec in recording.SubjectRecordings)
            {
                foreach (var e in rec.CapturedRotations)
                {
                    rotationData.AppendFormat(
                        "{0}, {1}\n",
                        rec.SubjectID,
                        e.ToCSV()
                    );
                }
            }

            StringBuilder lifecycleData = new StringBuilder("SubjectID, Time, Event\n");
            foreach (var rec in recording.SubjectRecordings)
            {
                foreach (var e in rec.CapturedLifeCycleEvents)
                {
                    lifecycleData.AppendFormat(
                        "{0}, {1}\n",
                        rec.SubjectID,
                        e.ToCSV()
                    );
                }
            }

            StringBuilder subjectMetaData = new StringBuilder("SubjectID, Key, Value\n");
            foreach (var rec in recording.SubjectRecordings)
            {
                foreach (var e in rec.Metadata)
                {
                    subjectMetaData.AppendFormat(
                        "{0}, {1}, {2}\n",
                        rec.SubjectID,
                        FormattingUtil.StringToCSVCell(e.Key),
                        FormattingUtil.StringToCSVCell(e.Value)
                    );
                }
            }
            foreach (var e in recording.Metadata)
            {
                subjectMetaData.AppendFormat(
                    "{0}, {1}, {2}\n",
                    -1,
                    FormattingUtil.StringToCSVCell(e.Key),
                    FormattingUtil.StringToCSVCell(e.Value)
                );
            }

            File.WriteAllText(Path.Combine(dir, "Subjects.csv"), subjects.ToString());
            File.WriteAllText(Path.Combine(dir, "SubjectMetaData.csv"), subjectMetaData.ToString());
            File.WriteAllText(Path.Combine(dir, "CustomEvents.csv"), customEvents.ToString());
            File.WriteAllText(Path.Combine(dir, "PositionData.csv"), positionData.ToString());
            File.WriteAllText(Path.Combine(dir, "RotationData.csv"), rotationData.ToString());
            File.WriteAllText(Path.Combine(dir, "LifeCycleEvents.csv"), lifecycleData.ToString());
            EditorUtility.RevealInFinder(dir);
        }

        private void ExportMultiple(string dir, Recording[] recordings)
        {
            StringBuilder recordingsFile = new StringBuilder("ID, Name\n");
            for (int i = 0; i < recordings.Length; i++)
            {
                recordingsFile.AppendFormat(
                    "{0}, {1}\n",
                    i,
                    FormattingUtil.StringToCSVCell(recordings[i].RecordingName)
                );
            }

            StringBuilder subjects = new StringBuilder("Recoring, ID, Name\n");
            for (int i = 0; i < recordings.Length; i++)
            {
                foreach (var subj in recordings[i].SubjectRecordings)
                {
                    subjects.AppendFormat(
                        "{0}, {1}, {2}\n",
                        i,
                        subj.SubjectID,
                        FormattingUtil.StringToCSVCell(subj.SubjectName)
                    );
                }
            }

            StringBuilder customEvents = new StringBuilder("Recording, SubjectID, Time, Name, Contents\n");
            for (int i = 0; i < recordings.Length; i++)
            {
                foreach (var subj in recordings[i].SubjectRecordings)
                {
                    foreach (var e in subj.CapturedCustomEvents)
                    {
                        customEvents.AppendFormat(
                            "{0}, {1}, {2}\n",
                            i,
                            subj.SubjectID,
                            e.ToCSV()
                        );
                    }
                }

                foreach (var e in recordings[i].CapturedCustomEvents)
                {
                    customEvents.AppendFormat(
                        "{0}, {1}, {2}\n",
                        i,
                        -1,
                        e.ToCSV()
                    );
                }
            }

            StringBuilder positionData = new StringBuilder("Recording, SubjectID, Time, X, Y, Z\n");
            for (int i = 0; i < recordings.Length; i++)
            {
                foreach (var subj in recordings[i].SubjectRecordings)
                {
                    foreach (var e in subj.CapturedPositions)
                    {
                        positionData.AppendFormat(
                            "{0}, {1}, {2}\n",
                            i,
                            subj.SubjectID,
                            e.ToCSV()
                        );
                    }
                }
            }

            StringBuilder rotationData = new StringBuilder("Recording, SubjectID, Time, X, Y, Z\n");
            for (int i = 0; i < recordings.Length; i++)
            {
                foreach (var subj in recordings[i].SubjectRecordings)
                {
                    foreach (var e in subj.CapturedRotations)
                    {
                        rotationData.AppendFormat(
                            "{0}, {1}, {2}\n",
                            i,
                            subj.SubjectID,
                            e.ToCSV()
                        );
                    }
                }
            }

            StringBuilder lifecycleData = new StringBuilder("Recording, SubjectID, Time, Event\n");
            for (int i = 0; i < recordings.Length; i++)
            {
                foreach (var subj in recordings[i].SubjectRecordings)
                {
                    foreach (var e in subj.CapturedLifeCycleEvents)
                    {
                        lifecycleData.AppendFormat(
                            "{0}, {1}, {2}\n",
                            i,
                            subj.SubjectID,
                            e.ToCSV()
                        );
                    }
                }
            }

            StringBuilder subjectMetaData = new StringBuilder("Recording, SubjectID, Key, Value\n");
            for (int i = 0; i < recordings.Length; i++)
            {
                foreach (var rec in recordings[i].SubjectRecordings)
                {
                    foreach (var e in rec.Metadata)
                    {
                        subjectMetaData.AppendFormat(
                            "{0}, {1}, {2}, {3}\n",
                            i,
                            rec.SubjectID,
                            FormattingUtil.StringToCSVCell(e.Key),
                            FormattingUtil.StringToCSVCell(e.Value)
                        );
                    }
                }
                foreach (var e in recordings[i].Metadata)
                {
                    subjectMetaData.AppendFormat(
                        "{0}, {1}, {2}, {3}\n",
                        i,
                        -1,
                        FormattingUtil.StringToCSVCell(e.Key),
                        FormattingUtil.StringToCSVCell(e.Value)
                    );
                }
            }

            File.WriteAllText(Path.Combine(dir, "Recordings.csv"), recordingsFile.ToString());
            File.WriteAllText(Path.Combine(dir, "Subjects.csv"), subjects.ToString());
            File.WriteAllText(Path.Combine(dir, "SubjectMetaData.csv"), subjectMetaData.ToString());
            File.WriteAllText(Path.Combine(dir, "CustomEvents.csv"), customEvents.ToString());
            File.WriteAllText(Path.Combine(dir, "PositionData.csv"), positionData.ToString());
            File.WriteAllText(Path.Combine(dir, "RotationData.csv"), rotationData.ToString());
            File.WriteAllText(Path.Combine(dir, "LifeCycleEvents.csv"), lifecycleData.ToString());
            EditorUtility.RevealInFinder(dir);
        }

        private string Error()
        {
            if (path == "")
            {
                return "A location must be set for saving csv data";
            }
            else if (folderName == "")
            {
                return "Please provide a name to represent the csv data";
            }

            if (Directory.Exists(Path.Combine(path, folderName)))
            {
                return "Folder name is already taken";
            }

            return "";
        }

    }

}
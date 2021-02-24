using UnityEngine;
using UnityEditor;
using EliCDavis.RecordAndPlay.Record;
using System.Collections.Generic;


namespace EliCDavis.RecordAndPlay.Editor.Extension
{
    /// <summary>
    /// An Editor window meant for allowing a user access to recording controls without having to write code.  Useful to those such as researchers who want control over the recording features while a subject plays the game. Access through Window/Record And Play/Record
    /// </summary>
    [InitializeOnLoad]
    internal class RecordingWindow : EditorWindow
    {

        enum RecordingMethod
        {
            /// <summary>
            /// We specify actors and build our own recorder
            /// </summary>
            Subjects,

            /// <summary>
            /// We will use a specific recorder provided and manually toggle
            /// </summary>
            Recorder
        }

        private static string recordingName = "";

        private static Recorder recorder;

        private RecordingMethod currentRecordingMethod = RecordingMethod.Subjects;

        static RecordingWindow()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (recorder != null && recorder.CurrentlyStopped() == false)
            {
                RecordingUtil.SaveToAssets(recorder.Finish(), recordingName);
            }
        }

        [MenuItem("Window/Record And Play/Record")]
        static void Init()
        {
            RecordingWindow window = (RecordingWindow)GetWindow(typeof(RecordingWindow));
            window.Show();
        }

        private string ReasonCantRecord()
        {
            if (recorder != null && recorder.CurrentlyRecording())
            {
                return "";
            }

            if (recordingName == "")
            {
                return "You need to set a recording name";
            }

            switch (currentRecordingMethod)
            {
                case RecordingMethod.Subjects:
                    if (subjects.Count == 0)
                    {
                        return "There's no subjects to record";
                    }
                    foreach (var subj in subjects)
                    {
                        if (subj == null)
                        {
                            return "Can't record when a subject is null";
                        }
                    }
                    break;
                case RecordingMethod.Recorder:
                    if (recorder == null)
                    {
                        return "Can't record without a recorder set";
                    }
                    break;
                default:
                    return string.Format("Unknown state: {0}, contact unity package author at eli.davis1995@gmail.com if you did not manually put in this state", currentRecordingMethod);
            }

            if (EditorApplication.isPlaying == false)
            {
                return "Can't record when scene is not playing";
            }

            return "";
        }

        // Update is called once per frame
        void OnGUI()
        {
            titleContent = new GUIContent("Recording");
            if (ReasonCantRecord() == "")
            {
                DisplayRecordingControls();
            }
            else
            {
                EditorGUILayout.HelpBox(ReasonCantRecord(), MessageType.Error);
            }
            if (recorder == null || recorder.CurrentlyStopped())
            {
                recordingName = EditorGUILayout.TextField("Recording Name", recordingName);
                RecordingMethod newRecordingMethod = (RecordingMethod)EditorGUILayout.EnumPopup("Recording method", currentRecordingMethod);

                // Clear recording if method has changed
                if (newRecordingMethod != currentRecordingMethod)
                {
                    recorder = null;
                }

                currentRecordingMethod = newRecordingMethod;

                switch (currentRecordingMethod)
                {
                    case RecordingMethod.Subjects:
                        SetSubjectsSection();
                        break;

                    case RecordingMethod.Recorder:
                        SetRecorderField();
                        break;
                }
            }


        }

        List<GameObject> subjects = new List<GameObject>();

        Vector2 scrollPos;

        private void SetSubjectsSection()
        {
            int newNumSubjects = EditorGUILayout.IntField("Number of Subjects", subjects.Count);
            if (newNumSubjects != subjects.Count)
            {
                if (newNumSubjects < subjects.Count)
                {
                    subjects.RemoveRange(newNumSubjects, subjects.Count - newNumSubjects);
                }
                else if (newNumSubjects > subjects.Count)
                {
                    if (newNumSubjects > subjects.Capacity)
                    {
                        subjects.Capacity = newNumSubjects;
                    }

                    subjects.Capacity = newNumSubjects;
                    var newObj = new GameObject[newNumSubjects - subjects.Count];
                    for (int i = 0; i < newObj.Length; i++)
                    {
                        newObj[i] = null;
                    }
                    subjects.AddRange(newObj);
                }

            }
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            for (int i = 0; i < subjects.Count; i++)
            {
                subjects[i] = (GameObject)EditorGUILayout.ObjectField(subjects[i], typeof(GameObject), true);
            }
            EditorGUILayout.EndScrollView();
        }

        private void SetRecorderField()
        {
            recorder = (Recorder)EditorGUILayout.ObjectField("Recorder", recorder, typeof(Recorder), false);
        }

        float timeStartedRecording;

        private void DisplayRecordingControls()
        {
            if (currentRecordingMethod == RecordingMethod.Recorder)
            {
                if (recorder == null)
                {
                    return;
                }
                switch (recorder.CurrentState())
                {
                    case RecordingState.Stopped:
                        if (GUILayout.Button("Start Recording"))
                        {
                            recorder.Start();
                            timeStartedRecording = Time.realtimeSinceStartup;
                        }
                        break;
                    case RecordingState.Recording:
                        RenderCurrentlyRecordingSection();
                        break;
                }
            }
            else if (currentRecordingMethod == RecordingMethod.Subjects)
            {
                if ((recorder == null || recorder.CurrentlyStopped()) && GUILayout.Button("Start Recording"))
                {
                    if (recorder == null)
                    {
                        recorder = ScriptableObject.CreateInstance<Recorder>();
                    }
                    recorder.ClearSubjects();
                    foreach (var subject in subjects)
                    {
                        SubjectBehavior.Build(subject, recorder);
                    }
                    recorder.Start();
                    timeStartedRecording = Time.realtimeSinceStartup;
                }
                else if (recorder != null && recorder.CurrentlyRecording())
                {
                    RenderCurrentlyRecordingSection();
                }
            }

        }

        private void RenderCurrentlyRecordingSection()
        {
            if (GUILayout.Button("Stop Recording"))
            {
                RecordingUtil.SaveToAssets(recorder.Finish(), recordingName);
            }
            var centerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
            };
            EditorGUILayout.LabelField(string.Format("{0:0.00}", Time.realtimeSinceStartup - timeStartedRecording), centerStyle);
        }

        void Update()
        {
            if (recorder != null && recorder.CurrentlyRecording())
            {
                Repaint();
            }
        }

        void OnDestroy()
        {
            if (recorder != null && recorder.CurrentlyStopped() == false)
            {
                recorder.Finish();
            }
        }

    }

}
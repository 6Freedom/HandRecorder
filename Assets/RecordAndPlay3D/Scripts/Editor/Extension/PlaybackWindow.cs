using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using EliCDavis.RecordAndPlay.Playback;

namespace EliCDavis.RecordAndPlay.Editor.Extension
{

    /// <summary>
    /// A window for manual playback of saved recordings. Access through Window/Record And Play/Playback
    /// </summary>
    [InitializeOnLoad]
    public class PlaybackWindow : EditorWindow
    {

        class EditorActorBuilder : IActorBuilder
        {
            public Dictionary<int, GameObject> mapping;

            public EditorActorBuilder()
            {
                mapping = new Dictionary<int, GameObject>();
            }

            public Actor Build(int actorId, string actorName, Dictionary<string, string> metadata)
            {
                if (mapping.ContainsKey(actorId) && mapping[actorId] != null)
                {
                    return new Actor(Instantiate(mapping[actorId]));
                }

                return null;
            }
        }

        private EditorActorBuilder actorBuilder;

        private EditorActorBuilder ActorBuilderReference()
        {
            if (actorBuilder == null)
            {
                actorBuilder = new EditorActorBuilder();
            }
            return actorBuilder;
        }

        private Recording loadedRecording = null;

        /// <summary>
        /// Keep for the InitializeOnLoad attribute
        /// </summary>
        static PlaybackWindow()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        [MenuItem("Window/Record And Play/Playback")]
        public static PlaybackWindow Init()
        {
            PlaybackWindow window = (PlaybackWindow)GetWindow(typeof(PlaybackWindow));
            window.Show();
            window.Repaint();

            return window;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (playbackService != null)
            {
                DestroyImmediate(playbackService.gameObject);
            }
        }

        public void SetRecordingForPlayback(Recording recording)
        {
            if (playbackService != null)
            {
                playbackService.Stop(true);
                DestroyImmediate(playbackService.gameObject);
            }
            loadedRecording = recording;
            actorRepresentationsToUse = new GameObject[loadedRecording.SubjectRecordings.Length];
            Repaint();
        }

        public void Awake()
        {
            actorBuilder = new EditorActorBuilder();
        }

        void OnGUI()
        {
            recordingColor = new Color(1f, 1f, 1f, .5f);

            titleContent = new GUIContent("Playback");

            if (loadedRecording == null)
            {
                RecordingSelection(3, 3);
            }
            else
            {
                int currentHeight = 3;

                currentHeight += LoadedRecordingDetails(3, currentHeight);

                if (playbackService == null)
                {
                    currentHeight += RecordingPlaybackOptions(3, currentHeight);
                    currentHeight += ControlPlaybackOptions(3, currentHeight);
                    currentHeight += RecordingSelection(3, currentHeight);
                }
                else
                {
                    ControlPlaybackOptions(3, currentHeight);
                }
            }
        }

        private int RecordingSelection(int x, int y)
        {
            var last = loadedRecording;
            loadedRecording = (Recording)EditorGUI.ObjectField(new Rect(x, y, position.width - (x * 2), 17), "Recording To Playback", loadedRecording, typeof(Recording), false);
            if (last != loadedRecording)
            {
                actorRepresentationsToUse = new GameObject[loadedRecording.SubjectRecordings.Length];
            }
            return 30;
        }

        void Update()
        {
            if (playbackService != null)
            {
                playbackService.UpdateState();
                Repaint();
            }
        }

        Color recordingColor = new Color(1f, 1f, 1f, 1f);

        static PlaybackBehavior playbackService = null;

        private int ControlPlaybackOptions(int x, int y)
        {
            Rect dimensions = new Rect(x, y, position.width - (x * 2), 70);

            GUI.BeginGroup(dimensions);

            if (playbackService == null)
            {
                if (GUI.Button(new Rect(0, 0, dimensions.width, 40), "Play"))
                {
                    playbackService = PlaybackBehavior.Build(loadedRecording, useCustomActors ? ActorBuilderReference() : null, null, loop);
                    playbackService.Play();
                }

                GUI.EndGroup();
                return 50;
            }

            if (GUI.Button(new Rect(dimensions.width / 2 + 5, 0, dimensions.width / 2 - 10, 30), "Stop"))
            {
                playbackService.Stop(true);
                DestroyImmediate(playbackService.gameObject);
            }

            if (playbackService.CurrentlyPlaying() && GUI.Button(new Rect(5, 0, dimensions.width / 2 - 10, 30), "Pause"))
            {
                playbackService.Pause();
            }

            if (playbackService.CurrentlyPaused() && GUI.Button(new Rect(5, 0, dimensions.width / 2 - 10, 30), "Resume"))
            {
                playbackService.Play();
            }

            playbackService.SetPlaybackSpeed(EditorGUI.Slider(new Rect(3, 35, dimensions.width - 6, 20), "Playback Speed", playbackService.GetPlaybackSpeed(), -8, 8));

            float curTime = playbackService.GetTimeThroughPlayback();
            float newTime = EditorGUI.Slider(new Rect(3, 55, dimensions.width - 6, 20), "Current Time", curTime, 0, loadedRecording.GetDuration());

            if (newTime != curTime)
            {
                playbackService.SetTimeThroughPlayback(newTime);
            }

            GUI.EndGroup();
            return 70;
        }

        private bool useCustomActors = false;

        private bool loop = true;

        GameObject[] actorRepresentationsToUse;

        Vector2 actorSetScrollPos;

        private int RecordingPlaybackOptions(int x, int y)
        {
            if (loadedRecording == null)
            {
                return 0;
            }

            int height = 60;
            if (useCustomActors)
            {
                height = (int)Mathf.Min((loadedRecording.SubjectRecordings.Length * 20) + height, position.height - y - 80);
            }

            Rect dimensions = new Rect(x, y, position.width - (x * 2), height);
            GUI.BeginGroup(dimensions);
            loop = EditorGUILayout.Toggle("Loop", loop);
            useCustomActors = EditorGUILayout.Toggle("Set Custom Actors", useCustomActors);

            int spaceTakenUp = 0;
            if (useCustomActors)
            {
                actorSetScrollPos = GUI.BeginScrollView(new Rect(0, 40, position.width - (x * 2), height - 50), actorSetScrollPos, new Rect(0, 0, position.width - (x * 10), 20 * loadedRecording.SubjectRecordings.Length));
                for (int actorIndex = 0; actorIndex < loadedRecording.SubjectRecordings.Length; actorIndex++)
                {
                    actorRepresentationsToUse[actorIndex] = (GameObject)EditorGUI.ObjectField(
                        new Rect(3, 20 * actorIndex, position.width - 30, 17),
                        loadedRecording.SubjectRecordings[actorIndex].SubjectName,
                        actorRepresentationsToUse[actorIndex],
                        typeof(GameObject),
                        false
                    );

                    if (ActorBuilderReference().mapping.ContainsKey(loadedRecording.SubjectRecordings[actorIndex].SubjectID))
                    {
                        ActorBuilderReference().mapping[loadedRecording.SubjectRecordings[actorIndex].SubjectID] = actorRepresentationsToUse[actorIndex];
                    }
                    else
                    {
                        ActorBuilderReference().mapping.Add(loadedRecording.SubjectRecordings[actorIndex].SubjectID, actorRepresentationsToUse[actorIndex]);
                    }
                    spaceTakenUp += 20;
                }
                GUI.EndScrollView();
            }

            GUI.EndGroup();
            return height;
        }

        void OnDestroy()
        {
            if (playbackService != null)
            {
                playbackService.Stop(true);
                DestroyImmediate(playbackService.gameObject);
            }
        }

        private int LoadedRecordingDetails(int x, int y)
        {
            if (loadedRecording == null)
            {
                return 0;
            }

            Rect dimensions = new Rect(x, y, position.width - (x * 2), 70);

            GUI.BeginGroup(dimensions);

            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 18,
            };

            var centerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
            };

            EditorGUI.DrawRect(new Rect(0, 0, dimensions.width, dimensions.height), recordingColor);
            EditorGUI.LabelField(new Rect(3, 0, dimensions.width - 6, 30), loadedRecording.RecordingName, titleStyle);
            EditorGUI.LabelField(new Rect(3, 19, dimensions.width - 6, 30), loadedRecording.GetDuration().ToString("##.## 'seconds'"), centerStyle);
            EditorGUI.LabelField(new Rect(3, 53, dimensions.width - 6, 30), "Actors: " + loadedRecording.SubjectRecordings.Length.ToString());
            EditorGUI.LabelField(new Rect(3 + (dimensions.width / 2), 53, dimensions.width / 2 - 6, 30), "Events: " + EditorUtil.NumberCustomEvents(loadedRecording).ToString());

            GUI.EndGroup();
            return 80;
        }

    }

}
using UnityEngine;

using System.IO;

using EliCDavis.RecordAndPlay.Record;
using EliCDavis.RecordAndPlay.Playback;
using System.Collections.Generic;

namespace EliCDavis.RecordAndPlay.Demo
{

    public class GameManager : MonoBehaviour, IActorBuilder, IPlaybackCustomEventHandler
    {

        [SerializeField]
        private PlayerBehavior player;

        [SerializeField]
        private GameObject[] objectsToTrack;

        private Vector3[] positions;

        private Recorder recorder;

        private PlaybackBehavior playback;

        [SerializeField]
        private GameObject bulletActor;

        [SerializeField]
        private GameObject playerActor;

        [SerializeField]
        private GameObject boxActor;

        [SerializeField]
        private GameObject deathEffect;

        [SerializeField]
        private GameObject collisionEffect;

        public Actor Build(int actorId, string actorName, Dictionary<string, string> metadata)
        {
            switch (actorName)
            {
                case "Player":
                    return new Actor(Instantiate(playerActor), this);

                case "Cube":
                    return new Actor(Instantiate(boxActor), this);

                case "Bullet":
                    return new Actor(Instantiate(bulletActor), this);
            }
            return null;
        }

        public void OnCustomEvent(SubjectRecording subject, CustomEventCapture customEvent)
        {
            switch (customEvent.Name)
            {
                case "Box Destroyed":
                    var dirtyCords = customEvent.Contents.Split(' ');
                    Destroy(Instantiate(deathEffect, new Vector3(float.Parse(dirtyCords[0]), -10, float.Parse(dirtyCords[1])), Quaternion.Euler(-90, 0, 0)), 3f);
                    break;

                case "Collision":
                    var dirtyCollisionCords = customEvent.Contents.Split(' ');
                    Destroy(Instantiate(collisionEffect, new Vector3(float.Parse(dirtyCollisionCords[0]), float.Parse(dirtyCollisionCords[1]), float.Parse(dirtyCollisionCords[2])), Quaternion.identity), 3f);
                    break;

                default:
                    Debug.LogWarningFormat("Don't know how to handle event type: {0}", customEvent.Name);
                    break;
            }
        }

        void Start()
        {
            recorder = ScriptableObject.CreateInstance<Recorder>();
            player.Initialize(recorder);
            positions = new Vector3[objectsToTrack.Length];
            for (int i = objectsToTrack.Length - 1; i >= 0; i--)
            {
                positions[i] = objectsToTrack[i].transform.position;
            }
            ClearObjectsToTrack();
        }

        void OnGUI()
        {
            switch (recorder.CurrentState())
            {
                case RecordingState.Recording:
                    if (recorder.CurrentlyRecording() && GUI.Button(new Rect(10, 10, 120, 25), "Pause"))
                    {
                        recorder.Pause();
                    }


                    if (GUI.Button(new Rect(10, 40, 120, 25), "Finish"))
                    {
                        playback = PlaybackBehavior.Build(recorder.Finish(), this, this, true);
                        ClearObjectsToTrack();
                    }
                    break;

                case RecordingState.Paused:
                    if (recorder.CurrentlyPaused() && GUI.Button(new Rect(10, 10, 120, 25), "Resume"))
                    {
                        recorder.Resume();
                    }

                    if (GUI.Button(new Rect(10, 40, 120, 25), "Finish"))
                    {
                        playback = PlaybackBehavior.Build(recorder.Finish(), this, this, true);
                        ClearObjectsToTrack();
                    }
                    break;

                case RecordingState.Stopped:
                    if (GUI.Button(new Rect(10, 10, 120, 25), "Start Recording"))
                    {
                        recorder.ClearSubjects();
                        SetUpObjectsToTrack();
                        recorder.Start();
                        if (playback != null)
                        {
                            playback.Stop();
                            Destroy(playback.gameObject);
                        }
                    }
                    if (playback != null)
                    {
                        GUI.Box(new Rect(10, 50, 120, 250), "Playback");
                        if (playback.CurrentlyPlaying() == false && GUI.Button(new Rect(15, 75, 110, 25), "Start"))
                        {
                            playback.Play();
                        }

                        if (playback.CurrentlyPlaying() && GUI.Button(new Rect(15, 75, 110, 25), "Pause"))
                        {
                            playback.Pause();
                        }

                        GUI.Label(new Rect(55, 105, 100, 30), "Time");
                        GUI.Label(new Rect(55, 125, 100, 30), playback.GetTimeThroughPlayback().ToString("0.00"));
                        playback.SetTimeThroughPlayback(GUI.HorizontalSlider(new Rect(15, 150, 100, 30), playback.GetTimeThroughPlayback(), 0.0F, playback.RecordingDuration()));

                        GUI.Label(new Rect(20, 170, 100, 30), "Playback Speed");
                        GUI.Label(new Rect(55, 190, 100, 30), playback.GetPlaybackSpeed().ToString("0.00"));
                        playback.SetPlaybackSpeed(GUI.HorizontalSlider(new Rect(15, 215, 100, 30), playback.GetPlaybackSpeed(), -8, 8));

                        if (GUI.Button(new Rect(15, 250, 110, 25), "Save"))
                        {

                            //SaveToAssets(playback.GetRecording(), "Demo");
                            using (FileStream fs = File.Create(string.Format("{0}/demo.rap", Application.dataPath)))
                            {
                                var rec = playback.GetRecording();
                                rec.RecordingName = "Demo";
                                IO.Packager.Package(fs, rec);
                            }

                        }
                    }
                    break;
            }
        }

        private void ClearObjectsToTrack()
        {
            for (int i = objectsToTrack.Length - 1; i >= 0; i--)
            {
                Destroy(objectsToTrack[i]);
            }
        }

        int boxesDestroyed;

        void Update()
        {
            if (recorder.CurrentlyRecording())
            {
                for (int i = objectsToTrack.Length - 1; i >= 0; i--)
                {
                    if (objectsToTrack[i] != null && objectsToTrack[i].transform.position.y < -10)
                    {
                        Destroy(objectsToTrack[i]);
                        boxesDestroyed++;
                        recorder.SetMetaData("Boxes Destroyed", boxesDestroyed.ToString());
                        var pos = objectsToTrack[i].transform.position;
                        recorder.CaptureCustomEvent("Box Destroyed", string.Format("{0} {1}", pos.x, pos.z));
                    }
                }

            }
        }

        private void SetUpObjectsToTrack()
        {
            for (int i = 0; i < positions.Length; i++)
            {
                objectsToTrack[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                objectsToTrack[i].AddComponent<Rigidbody>();
                objectsToTrack[i].transform.position = positions[i];
                SubjectBehavior.Build(objectsToTrack[i], recorder);
            }
            recorder.Register(player.gameObject.GetComponent<SubjectBehavior>().GetSubjectRecorder());
            boxesDestroyed = 0;
        }

    }
}
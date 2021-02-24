using UnityEngine;
using System.Collections.Generic;

namespace EliCDavis.RecordAndPlay.Record
{

    /// <summary>
    /// Attatch this script to any Gameobject that you want to record. 
    /// </summary>
    /// <example>
    /// If you are creating instances of the SubjectBehavior at runtime, I would suggest looking into the <a page="EliCDavis.RecordAndPlay.Record.SubjectBehavior.Build(GameObject,%20Recorder)">Build()</a> function.
    /// 
    /// <code>var recorder = ScriptableObject.CreateInstance<Recorder>();
    /// var objectToTrack = GameObject.CreatePrimitive(PrimitiveType.Cube);
    /// SubjectBehavior.Build(objectToTrack, recorder);</code>
    /// </example>
    /// <remarks>
    /// The recorder tries to be smart with the captured positional and rotational values it saves. For details on how that works look at the <a page="EliCDavis.RecordAndPlay.Record.SubjectRecorder">SubjectRecorder</a>.
    /// </remarks>
    public class SubjectBehavior : MonoBehaviour
    {

        /// <summary>
        /// The name of the subject in the recording.
        /// </summary>
        [SerializeField]
        private string nameForRecording;

        /// <summary>
        /// Store anything extra about the object if you'd like
        /// </summary>
        [SerializeField]
        private Dictionary<string, string> metaData;

        /// <summary>
        /// How many times per second the position and orrientation will be 
        /// recorded. Setting to 0 will never record position, setting it to a 
        /// negative number will  record position every Update() call. 
        /// </summary>
        [SerializeField]
        private float framerateToRecordAt = 10;

        [SerializeField]
        private Recorder recorder;

        /// <summary>
        /// The minimum positional or rotational change that must occur before
        /// we store the new orientation information. The smaller the number, 
        /// the less the object has to move before it's considered an entirely
        /// new position.
        /// </summary>
        [SerializeField]
        [Tooltip("The minimum positional or rotational change that must occur before we store the new orientation information. The smaller the number, the less the object has to move before it's considered an entirely new position")]
        private float minimumDelta = 0.0001f;

        private SubjectRecorder subjectRecorder;

        private float timeOfLastFrameCapture;

        /// <summary>
        /// If you want to keep up with something special that occured at a certain time in your recording, then you can call this function with the details of the special event.
        /// </summary>
        /// <param name="name">Name of the event.</param>
        /// <param name="contents">Details of the event.</param>
        public void CaptureCustomEvent(string name, string contents)
        {
            if (subjectRecorder == null)
            {
                return;
            }
            subjectRecorder.CaptureCustomEvent(Time.time, name, contents);
        }

        /// <summary>
        /// Builds a SubjectBehavior and it's corrresponding SubjectRecorder and attatches itself to the gameobject passed in.
        /// </summary>
        /// <param name="subject">The gameobject to record.</param>
        /// <param name="recorder">The recorder that is incharge of all subjects.</param>
        /// <param name="frameRate">How many times a second the recorder will observe the position and rotation of the subject.</param>
        /// <param name="name">The name of the subject to be used when saved to file.</param>
        /// <param name="metadata">Any starting metadata on the specific subject.</param>
        /// <param name="minimumDelta">How much the position or rotation must change before it's considered moved.</param>
        /// <returns>The subject behavior that's been attatched to the gameobject passed in.</returns>
        public static SubjectBehavior Build(GameObject subject, Recorder recorder, int frameRate, string name, Dictionary<string, string> metadata, float minimumDelta)
        {
            var subjectBehavior = subject.AddComponent<SubjectBehavior>();

            subjectBehavior.subjectRecorder = new SubjectRecorder(subject, name, metadata, minimumDelta);
            recorder.Register(subjectBehavior.subjectRecorder);

            subjectBehavior.recorder = recorder;
            subjectBehavior.framerateToRecordAt = frameRate;
            subjectBehavior.nameForRecording = name;
            subjectBehavior.metaData = metadata;
            subjectBehavior.minimumDelta = minimumDelta;
            return subjectBehavior;
        }

        /// <summary>
        /// Builds a SubjectBehavior and it's corrresponding SubjectRecorder and attatches itself to the gameobject passed in.
        /// </summary>
        /// <param name="subject">The gameobject to record.</param>
        /// <param name="recorder">The recorder that is incharge of all subjects.</param>
        /// <param name="frameRate">How many times a second the recorder will observe the position and rotation of the subject.</param>
        /// <param name="minimumDelta">How much the position or rotation must change before it's considered moved.</param>
        /// <returns>The subject behavior that's been attatched to the gameobject passed in.</returns>
        public static SubjectBehavior Build(GameObject subject, Recorder recorder, int frameRate, float minimumDelta)
        {
            return Build(subject, recorder, frameRate, "", null, minimumDelta);
        }

        /// <summary>
        /// Builds a SubjectBehavior and it's corrresponding SubjectRecorder and attatches itself to the gameobject passed in.
        /// </summary>
        /// <param name="subject">The gameobject to record.</param>
        /// <param name="recorder">The recorder that is incharge of all subjects.</param>
        /// <returns>The subject behavior that's been attatched to the gameobject passed in.</returns>
        public static SubjectBehavior Build(GameObject subject, Recorder recorder)
        {
            return Build(subject, recorder, 10, "", null, 0.0001f);
        }

        /// <summary>
        /// Builds a SubjectBehavior and it's corrresponding SubjectRecorder and attatches itself to the gameobject passed in.
        /// </summary>
        /// <param name="subject">The gameobject to record.</param>
        /// <param name="recorder">The recorder that is incharge of all subjects.</param>
        /// <param name="name">The name of the subject to be used when saved to file.</param>
        /// <param name="metadata">Any starting metadata on the specific subject.</param>
        /// <returns>The subject behavior that's been attatched to the gameobject passed in.</returns>
        public static SubjectBehavior Build(GameObject subject, Recorder recorder, string name, Dictionary<string, string> metadata)
        {
            return Build(subject, recorder, 10, name, metadata, 0.0001f);
        }

        /// <summary>
        /// Builds a SubjectBehavior and it's corrresponding SubjectRecorder and attatches itself to the gameobject passed in.
        /// </summary>
        /// <param name="subject">The gameobject to record.</param>
        /// <param name="recorder">The recorder that is incharge of all subjects.</param>
        /// <param name="name">The name of the subject to be used when saved to file.</param>
        /// <returns>The subject behavior that's been attatched to the gameobject passed in.</returns>
        public static SubjectBehavior Build(GameObject subject, Recorder recorder, string name)
        {
            return Build(subject, recorder, 10, name, null, 0.0001f);
        }

        void Start()
        {
            if (recorder == null)
            {
                throw new System.Exception("Can't record the subject without a recorder");
            }

            if (subjectRecorder == null)
            {
                subjectRecorder = new SubjectRecorder(gameObject, nameForRecording, metaData, minimumDelta);
                recorder.Register(subjectRecorder);
            }

            CaptureUnityEvent(UnityLifeCycleEvent.Start);
        }

        void OnDisable()
        {
            CaptureUnityEvent(UnityLifeCycleEvent.Disable);
        }

        void OnDestroy()
        {
            CaptureUnityEvent(UnityLifeCycleEvent.Destroy);
        }

        void OnEnable()
        {
            CaptureUnityEvent(UnityLifeCycleEvent.Enable);
        }

        /// <summary>
        /// Keep up with custom data associated with the subject.
        /// </summary>
        /// <param name="key">Name of the custom data.</param>
        /// <param name="value">Details of the custom data.</param>
        public void SetMetaData(string key, string value)
        {
            subjectRecorder.SetMetaData(key, value);
        }

        void Update()
        {
            if (framerateToRecordAt == 0 || subjectRecorder == null)
            {
                return;
            }

            if (framerateToRecordAt < 0 || Time.time - timeOfLastFrameCapture >= 1f / framerateToRecordAt)
            {
                timeOfLastFrameCapture = Time.time;
                subjectRecorder.Capture(timeOfLastFrameCapture);
            }
        }

        /// <summary>
        /// How many times the recorder takes snapshots of the state of the subject while recording.
        /// </summary>
        /// <returns>How many times a second the state is captured.</returns>
        public float GetFPS()
        {
            return framerateToRecordAt;
        }

        /// <summary>
        /// The recorder that the subject specifically uses for recording orientations and lifecycle events.
        /// </summary>
        /// <returns>The recorder.</returns>
        public SubjectRecorder GetSubjectRecorder()
        {
            return subjectRecorder;
        }

        private void CaptureUnityEvent(UnityLifeCycleEvent e)
        {
            if (subjectRecorder != null)
            {
                subjectRecorder.CaptureUnityEvent(Time.time, e);
            }
        }

    }

}
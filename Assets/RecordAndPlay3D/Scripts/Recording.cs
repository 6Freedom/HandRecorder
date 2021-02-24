using System.Text;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using EliCDavis.RecordAndPlay.Playback;
using EliCDavis.RecordAndPlay.Util;

namespace EliCDavis.RecordAndPlay
{

    /// <summary>
    /// A collection of different events and objects that where captured with a recorder. 
    /// </summary>
    /// <remarks>
    /// This is a <a href="https://docs.unity3d.com/ScriptReference/ScriptableObject.html">scriptable object</a>, so to create it you should call the static method <b>Recording.CreateInstance()</b>. However, creation of recordings is probably best left up to the <a page="EliCDavis.RecordAndPlay.Record.Recorder">Recorder</a> object.
    /// 
    /// If you wish to save this recording to the projects assets for ease of playback in the editor, then look into <a page="EliCDavis.RecordAndPlay.Recording.SaveToAssets(string)">SaveToAssets</a>. Doing this will create a recording object in your assets folder that can be viewed directly in the editor, or can used as a variable in the inspector of some script.
    /// 
    /// If you wish to save the recording efficiently to disk for purposes such as saving memory or transmitting over the web, look into the <a page="EliCDavis.RecordAndPlay.IO.Packager">Packager</a> class.
    /// 
    /// The first event of a recording does not have to occur at timestamp 0. The recorder object uses <a href="https://docs.unity3d.com/ScriptReference/Time-time.html">Time.time</a> to timestamp events, so the first event of the recording can take place at any point in time.
    /// </remarks>
    [System.Serializable]
    public class Recording : ScriptableObject, ISerializationCallbackReceiver
    {

        [SerializeField]
        private SubjectRecording[] subjectRecordings;

        /// <summary>
        /// All events that where logged while we where recording the scene.
        /// </summary>
        [SerializeField]
        private CustomEventCapture[] capturedCustomEvents;

        private Dictionary<string, string> metadata;

        [SerializeField]
        private List<string> metadataKeys;

        [SerializeField]
        private List<string> metadataValues;

        private float? startTime = null;

        private float? duration = null;

        /// <summary>
        /// Acts as the constructor of the recording class since it inherits from <a href="https://docs.unity3d.com/ScriptReference/ScriptableObject.html">ScriptableObject</a>.
        /// </summary>
        /// <param name="subjectRecordings">The different subjects that whose position and rotation where recorded.</param>
        /// <param name="capturedCustomEvents">Different global custom events that occured during capture.</param>
        /// <param name="metadata">Global key value pairs with no associated timestamp.</param>
        /// <returns>A Recording that can be used for playback.</returns>
        public static Recording CreateInstance(SubjectRecording[] subjectRecordings, CustomEventCapture[] capturedCustomEvents, Dictionary<string, string> metadata)
        {
            var data = CreateInstance<Recording>();
            data.metadata = metadata == null ? new Dictionary<string, string>() : metadata;
            data.metadataKeys = new List<string>();
            data.metadataValues = new List<string>();
            data.subjectRecordings = subjectRecordings;
            data.capturedCustomEvents = capturedCustomEvents;
            return data;
        }

        /// <summary>
        /// Gets the duration of the recording by comparing the timestamps of the first and last captured events.
        /// </summary>
        /// <returns>The duration of the recording.</returns>
        public float GetDuration()
        {
            if (duration == null)
            {
                duration = CalculateDuration();
            }
            return (float)duration;
        }

        private float CalculateDuration()
        {
            float max = -1;

            if (subjectRecordings != null)
            {
                foreach (var subject in subjectRecordings)
                {
                    if (subject != null)
                    {
                        max = Mathf.Max(max, subject.GetDuration());
                    }
                }
            }

            return max;
        }

        /// <summary>
        /// Builds actors meant to act out the recorded objects and what they did. 
        /// </summary>
        /// <param name="actorBuilder">What will take the subject data and return us actors for playback.</param>
        /// <param name="parent">What all created actors will be parented to.</param>
        /// <returns>Controls for controlling the playback of each individual actor.</returns>
        /// <remarks> If the actorBuilder is null then cubes will be used to represent the subject. If an actorBuilder is supplied but they supply a null Actor, then that subject will be excluded from the playback. It' generally best to not call this method directly, but use an instance of <a page="EliCDavis.RecordAndPlay.Playback.PlaybackBehavior">PlaybackBehavior</a> to call this method for you and manage all the actors.</remarks>
        public ActorPlaybackControl[] BuildActors(IActorBuilder actorBuilder, Transform parent)
        {
            List<ActorPlaybackControl> actors = new List<ActorPlaybackControl>();

            for (int actorIndex = 0; actorIndex < SubjectRecordings.Length; actorIndex++)
            {
                SubjectRecording subject = SubjectRecordings[actorIndex];
                GameObject actorRepresentation = null;
                Actor actor = null;

                if (actorBuilder != null)
                {
                    actor = actorBuilder.Build(subject.SubjectID, subject.SubjectName, subject.Metadata);
                    actorRepresentation = actor == null ? null : actor.Representation;
                }
                else
                {
                    actorRepresentation = GameObject.CreatePrimitive(PrimitiveType.Cube);
                }

                if (actorRepresentation != null)
                {
                    actorRepresentation.transform.SetParent(parent);
                    actorRepresentation.transform.name = subject.SubjectName;
                    actorRepresentation.transform.position = subject.GetStartingPosition();
                    actorRepresentation.transform.rotation = subject.GetStartingRotation();
                    actors.Add(new ActorPlaybackControl(actorRepresentation, actor == null ? null : actor.CustomEventHandler, this, subject));
                }

            }

            return actors.ToArray();
        }

        /// <summary>
        /// The name of the recording/scriptable object asset
        /// </summary>
        public string RecordingName
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// All objects whose positions and rotations where recorded in the scene.
        /// </summary>
        public SubjectRecording[] SubjectRecordings
        {
            get
            {
                return subjectRecordings;
            }
        }

        /// <summary>
        /// All custom events global to the recording. 
        /// </summary>
        public CustomEventCapture[] CapturedCustomEvents
        {
            get
            {
                return capturedCustomEvents;
            }
        }

        /// <summary>
        /// Global key value pairs with no associated timestamp. 
        /// </summary>
        public Dictionary<string, string> Metadata
        {
            get
            {
                return metadata;
            }
        }

        /// <summary>
        /// Saves the recording as an asset to the asset folder of the project. <b>Editor Only</b>
        /// </summary>
        /// <param name="name">Name of the asset.</param>
        /// <param name="path">Where in the project for the asset to be saved.</param>
        /// <remarks>Will append a number to the end of the name if another asset already uses the name passed in.</remarks>
        [System.Obsolete("SaveToAssets is deprecated as it makes editor based calls, please use EliCDavis.RecordAndPlay.Editor.RecordingUtil.SaveToAssets instead.")]
        public void SaveToAssets(string name, string path)
        {
            Debug.LogWarning("SaveToAssets has been depreciated and will be removed in 2.0, please use EliCDavis.RecordAndPlay.Editor.RecordingUtil.SaveToAssets instead.");
#if UNITY_EDITOR
            if (path == "")
            {
                path = "Assets";
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, string.Format("{0}.asset", name)));

            AssetDatabase.CreateAsset(this, assetPathAndName);
            AssetDatabase.SaveAssets();
#endif
        }

        /// <summary>
        /// Saves the recording as an asset to the root of the asset folder of the project. <b>Editor Only</b>
        /// </summary>
        /// <param name="name">Name of the asset.</param>
        /// <remarks>Will append a number to the end of the name if another asset already uses the name passed in.</remarks>
        [System.Obsolete("SaveToAssets is deprecated as it makes editor based calls, please use EliCDavis.RecordAndPlay.Editor.RecordingUtil.SaveToAssets instead.")]
        public void SaveToAssets(string name)
        {
#if UNITY_EDITOR
            SaveToAssets(name, "");
#endif
        }


        /// <summary>
        /// Converts the Recording to a json formatted string.
        /// </summary>
        /// <returns>Json formatted String.</returns>
        public string ToJSON()
        {

            StringBuilder sb = new StringBuilder("{");
            sb.AppendFormat("\"Name\": \"{0}\", ", FormattingUtil.JavaScriptStringEncode(RecordingName));
            sb.AppendFormat("\"Duration\": {0}, ", GetDuration());
            sb.AppendFormat("\"Metadata\": {0}, ", FormattingUtil.ToJSON(metadata));

            sb.Append("\"CustomEvents\": [");
            for (int i = 0; i < capturedCustomEvents.Length; i++)
            {
                sb.Append(capturedCustomEvents[i].ToJSON());
                if (i < capturedCustomEvents.Length - 1)
                {
                    sb.Append(",");
                }
            }

            sb.Append("], \"Subjects\": [");
            for (int i = 0; i < subjectRecordings.Length; i++)
            {
                sb.Append(subjectRecordings[i].ToJSON());
                if (i < subjectRecordings.Length - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append("] }");

            return sb.ToString();
        }
        

        /// <summary>
        /// The first event of a recording does not have to occur at timestamp 0. This method calculates and caches the time of the first event captured in the recording.
        /// </summary>
        /// <returns>The time of the first event captured.</returns>
        public float GetStartTime()
        {
            if (startTime == null)
            {
                startTime = CalculateStartTime();
            }
            return (float)startTime;
        }

        private float CalculateStartTime()
        {
            var currentStart = Mathf.Infinity;
            for (int i = 0; i < subjectRecordings.Length; i++)
            {
                currentStart = Mathf.Min(currentStart, subjectRecordings[i].GetStartTime());
            }
            return currentStart;
        }

        /// <summary>
        /// Used for custom Unity serialization. <b>Do not call this method</b>.
        /// </summary>
        public void OnBeforeSerialize()
        {
            metadataKeys.Clear();
            metadataValues.Clear();
            if (metadata != null)
            {
                foreach (KeyValuePair<string, string> pair in metadata)
                {
                    metadataKeys.Add(pair.Key);
                    metadataValues.Add(pair.Value);
                }
            }

        }

        /// <summary>
        /// Used for custom Unity serialization. <b>Do not call this method</b>.
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (metadata == null)
            {
                metadata = new Dictionary<string, string>();
            }
            metadata.Clear();

            if (metadataKeys.Count != metadataValues.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

            for (int i = 0; i < metadataKeys.Count; i++)
                metadata.Add(metadataKeys[i], metadataValues[i]);
        }

    }

}
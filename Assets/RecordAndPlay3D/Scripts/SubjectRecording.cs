using UnityEngine;

using System.Text;
using System.Collections.Generic;

using EliCDavis.RecordAndPlay.Util;

/// <summary>
/// Namespace that contains the recording class and all it's associated classes.
/// </summary>
namespace EliCDavis.RecordAndPlay
{

    /// <summary>
    /// A recording with information pertaining to a single subject.
    /// </summary>
    /// <remarks>
    /// The number of captured positions does not have to match the number of captured rotations. The recorder is smart and aims to only capture changes in velocity of position and rotation. So even if the recorder is set to capture the subject's position 30 times a second and runs for 5 seconds, if the subject is moving in a straight line, then only 2 captures are kept to represent that line.
    /// </remarks>
    [System.Serializable]
    public class SubjectRecording : ISerializationCallbackReceiver
    {
        [SerializeField]
        private int subjectID;

        [SerializeField]
        private string subjectName;

        private Dictionary<string, string> metadata;

        [SerializeField]
        private List<string> metadataKeys;

        [SerializeField]
        private List<string> metadataValues;

        [SerializeField]
        private VectorCapture[] capturedPositions;

        [SerializeField]
        private VectorCapture[] capturedRotations;

        [SerializeField]
        private UnityLifeCycleEventCapture[] capturedLifeCycleEvents;

        [SerializeField]
        private CustomEventCapture[] capturedCustomSubjectEvents;

        private float? duration = null;

        /// <summary>
        /// Creates a new subject recording
        /// </summary>
        /// <param name="subjectID">Unique identifier for the subject in a recording.</param>
        /// <param name="subjectName">Name to refer to in the recording.</param>
        /// <param name="metadata">Custom key value pairs.</param>
        /// <param name="capturedPositions">Captured positions.</param>
        /// <param name="capturedRotations">Captured rotations.</param>
        /// <param name="capturedLifeCycleEvents">All lifecycle events that occured during the recording.</param>
        /// <param name="capturedCustomActorEvents">Custom key value pairs with associated time stamps.</param>
        public SubjectRecording(int subjectID, string subjectName, Dictionary<string, string> metadata, VectorCapture[] capturedPositions, VectorCapture[] capturedRotations, UnityLifeCycleEventCapture[] capturedLifeCycleEvents, CustomEventCapture[] capturedCustomActorEvents)
        {
            this.subjectID = subjectID;
            this.subjectName = subjectName;
            this.metadata = metadata == null ? new Dictionary<string, string>() : metadata;
            metadataKeys = new List<string>();
            metadataValues = new List<string>();
            this.capturedPositions = capturedPositions == null ? new VectorCapture[0] : capturedPositions;
            this.capturedRotations = capturedRotations == null ? new VectorCapture[0] : capturedRotations;
            this.capturedLifeCycleEvents = capturedLifeCycleEvents == null ? new UnityLifeCycleEventCapture[0] : capturedLifeCycleEvents;
            this.capturedCustomSubjectEvents = capturedCustomActorEvents == null ? new CustomEventCapture[0] : capturedCustomActorEvents;
        }

        /// <summary>
        /// The first ever recorded position made by the recorder.
        /// </summary>
        /// <returns>First ever recorded position.</returns>
        public Vector3 GetStartingPosition()
        {
            return capturedPositions.Length > 0 ? capturedPositions[0].Vector : Vector3.zero;
        }

        /// <summary>
        /// The last recorded position made by the recorder.
        /// </summary>
        /// <returns>Last recorded position.</returns>
        public Vector3 GetEndingPosition()
        {
            return capturedPositions.Length > 0 ? capturedPositions[capturedPositions.Length-1].Vector : Vector3.zero;
        }

        /// <summary>
        /// The first ever recorded rotation made by the recorder.
        /// </summary>
        /// <returns>First ever recorded rotation.</returns>
        public Quaternion GetStartingRotation()
        {
            return capturedRotations.Length > 0 ? Quaternion.Euler(capturedRotations[0].Vector) : Quaternion.identity;
        }

        /// <summary>
        /// The last recorded rotation made by the recorder.
        /// </summary>
        /// <returns>Last recorded rotation.</returns>
        public Quaternion GetEndingRotation()
        {
            return capturedRotations.Length > 0 ? Quaternion.Euler(capturedRotations[capturedRotations.Length - 1].Vector) : Quaternion.identity;
        }

        /// <summary>
        /// Refers to the unity object's <a href="https://docs.unity3d.com/ScriptReference/Object.GetInstanceID.html">instance ID</a>. Guaranteed to be unique within a single recording.
        /// </summary>
        public int SubjectID
        {
            get
            {
                return subjectID;
            }
        }

        /// <summary>
        /// The gameobject's name unless specified.
        /// </summary>
        public string SubjectName
        {
            get
            {
                return subjectName;
            }
        }

        /// <summary>
        /// Key value pairs with no associated timestamp specific to the subject 
        /// </summary>
        public Dictionary<string, string> Metadata
        {
            get
            {
                return metadata;
            }
        }

        /// <summary>
        /// All captured positions.
        /// </summary>
        public VectorCapture[] CapturedPositions
        {
            get
            {
                return capturedPositions;
            }
        }

        /// <summary>
        /// All captured rotations.
        /// </summary>
        public VectorCapture[] CapturedRotations
        {
            get
            {
                return capturedRotations;
            }
        }

        /// <summary>
        /// All lifecycle events that occured to the subject during a recording.
        /// </summary>
        /// <remarks>
        /// If a start event does not exist, that means the subject existsed before the recording started. An absence of a death event means that the subject was not destroyed while being recorded. 
        /// </remarks>
        public UnityLifeCycleEventCapture[] CapturedLifeCycleEvents
        {
            get
            {
                return capturedLifeCycleEvents;
            }
        }

        /// <summary>
        /// All custom events that only happened to this specific subject specifically. 
        /// </summary>
        public CustomEventCapture[] CapturedCustomEvents
        {
            get
            {
                return capturedCustomSubjectEvents;
            }
        }

        private float CaptureDuration<T>(T[] original) where T : Capture
        {
            if (original == null || original.Length == 0)
            {
                return -1;
            }
            return original[original.Length - 1].Time - original[0].Time;
        }

        private float MinimumTime<T>(T[] original) where T : Capture
        {
            if (original == null || original.Length == 0)
            {
                return Mathf.Infinity;
            }
            return original[0].Time;
        }

        private float CalculateDuration()
        {
            return Mathf.Max(
                CaptureDuration(capturedPositions),
                CaptureDuration(capturedRotations),
                CaptureDuration(capturedLifeCycleEvents),
                CaptureDuration(capturedCustomSubjectEvents)
            );
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

        /// <summary>
        /// The first event of a recording does not have to occur at timestamp 0. This method calculates and caches the time of the first event captured in the recording.
        /// </summary>
        /// <returns>The time of the first event captured.</returns>
        public float GetStartTime()
        {
            return Mathf.Min(
                MinimumTime(capturedPositions),
                MinimumTime(capturedRotations),
                MinimumTime(capturedLifeCycleEvents),
                MinimumTime(capturedCustomSubjectEvents)
            );
        }

        /// <summary>
        /// Converts the Subject Recording to a json formatted string.
        /// </summary>
        /// <returns>Json formatted String.</returns>
        public string ToJSON()
        {
            StringBuilder sb = new StringBuilder("{ ");

            sb.AppendFormat("\"ID\": {0}, ", subjectID);
            sb.AppendFormat("\"Name\": \"{0}\", ", FormattingUtil.JavaScriptStringEncode(subjectName));
            sb.AppendFormat("\"Metadata\": {0}, ", FormattingUtil.ToJSON(metadata));
            sb.AppendFormat("\"Duration\": {0}, ", GetDuration());

            sb.Append(" \"LifeCycleEvents\": [");
            for (int i = 0; i < capturedLifeCycleEvents.Length; i++)
            {
                sb.Append(capturedLifeCycleEvents[i].ToJSON());
                if (i < capturedLifeCycleEvents.Length - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append("], ");

            sb.Append(" \"CustomEvents\": [");
            for (int i = 0; i < capturedCustomSubjectEvents.Length; i++)
            {
                sb.Append(capturedCustomSubjectEvents[i].ToJSON());
                if (i < capturedCustomSubjectEvents.Length - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append("], ");


            sb.Append(" \"Positions\": [");
            for (int i = 0; i < capturedPositions.Length; i++)
            {
                sb.Append(capturedPositions[i].ToJSON());
                if (i < capturedPositions.Length - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append("], ");

            sb.Append(" \"Rotations\": [");
            for (int i = 0; i < capturedRotations.Length; i++)
            {
                sb.Append(capturedRotations[i].ToJSON());
                if (i < capturedRotations.Length - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append("] }");

            return sb.ToString();
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
            {
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));
            }

            for (int i = 0; i < metadataKeys.Count; i++)
            {
                metadata.Add(metadataKeys[i], metadataValues[i]);
            }
        }
    }

}

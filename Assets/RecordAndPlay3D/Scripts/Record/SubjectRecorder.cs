using UnityEngine;
using System.Collections.Generic;
using EliCDavis.RecordAndPlay.Util;

namespace EliCDavis.RecordAndPlay.Record
{

    /// <summary>
    /// Responsible for keeping up with all captured events of a specific subject.
    /// </summary>
    /// <remarks>
    /// The recorder trys to be smart with the captured positional and rotational values it saves. Playback of a recording interpolates between positions to keep things smooth. That means you do not need to record positions 60 frames a second for a smooth recording.
    /// 
    /// The recorder takes into account the direction the subject is moving in and aims to remove and adjust uneeded captured frames. That means if your subject is moving in a predictable fashion then there is less of a need for higher frame rates. If you're subject is moving arround in unpredictable ways extremely quickly (faster than your set framerate), it is recommended to raise your framerate capture to have a more accurate playback. 
    /// 
    /// Sometimes you don't care about very small changes in position or rotation, so you can adjust the minimumDelta to change what is determined to be an entirely new position. A minimumDelta of 0 means that every small change is considered a new position. A higher minimumDelta means that the position or rotation must change more until it is considered a new value.
    /// 
    /// The minimum delta is used to determine if two vectors are equal by comparing their square magnitude (instead of magnitude to run faster) as seen here:
    /// <code>
    /// bool V3Equal(Vector3 a, Vector3 b)
    /// {
    /// &emsp; return Vector3.SqrMagnitude(a - b) <= minimumDelta;
    /// }
    /// </code>
    /// </remarks>
    public class SubjectRecorder
    {
        private int instanceId;

        private GameObject subject;

        private string name;

        private Dictionary<string, string> metadata;

        private LinkedList<VectorCapture> capturedPositions;

        private LinkedList<VectorCapture> capturedRotations;

        private LinkedList<UnityLifeCycleEventCapture> capturedLifeCycleEvents;

        private LinkedList<CustomEventCapture> capturedCustomActorEvents;

        private float minimumDelta;

        /// <summary>
        /// Builds a new recorder.
        /// </summary>
        /// <param name="subject">What we are observing.</param>
        /// <param name="name">Name of the subject to be used as reference. If no name is provided the GameObject's name is substituted.</param>
        /// <param name="metadata">Any initial metadata to be associated with the recording.</param>
        /// <param name="minimumDelta">How much the position or rotation must change before it's considered different.</param>
        public SubjectRecorder(GameObject subject, string name, Dictionary<string, string> metadata, float minimumDelta)
        {
            if (subject == null)
            {
                throw new System.Exception("Can't create a recorder without something ot record");
            }

            this.subject = subject;
            this.name = name != "" ? name : subject.name;
            this.metadata = metadata;
            this.minimumDelta = minimumDelta;

            instanceId = subject.GetInstanceID();
            capturedPositions = new LinkedList<VectorCapture>();
            capturedRotations = new LinkedList<VectorCapture>();
            capturedLifeCycleEvents = new LinkedList<UnityLifeCycleEventCapture>();
            capturedCustomActorEvents = new LinkedList<CustomEventCapture>();
        }

        /// <summary>
        /// If you want to keep up with something special that occured at a certain time in your recording, then you can call this function with the details of the special event.
        /// </summary>
        /// <param name="time">Time the event occured.</param>
        /// <param name="name">Name of the event.</param>
        /// <param name="contents">Details of the event.</param>
        public void CaptureCustomEvent(float time, string name, string contents)
        {
            capturedCustomActorEvents.AddLast(new CustomEventCapture(time, name, contents));
        }

        /// <summary>
        /// Keep up with custom data associated with the subject.
        /// </summary>
        /// <param name="key">Name of the custom data.</param>
        /// <param name="value">Details of the custom data.</param>
        public void SetMetaData(string key, string value)
        {
            if (metadata == null)
            {
                metadata = new Dictionary<string, string>();
            }

            if (metadata.ContainsKey(key))
            {
                metadata[key] = value;
            }
            else
            {
                metadata.Add(key, value);
            }
        }

        /// <summary>
        /// Captures the position and rotation of the current subject.
        /// </summary>
        /// <param name="time">Time of the capture.</param>
        /// <remarks>
        /// This function determines whether or not it can drop previous captures to save space. If the subject is moving in a predictable fashion, then it might drop some positional frames for the sake of memory. This will not reduce the quality of the recording.
        /// </remarks>
        public void Capture(float time)
        {
            if (capturedPositions.Count > 1)
            {
                VectorCapture beforeLastPosition = capturedPositions.Last.Previous.Value;
                VectorCapture lastPosition = capturedPositions.Last.Value;

                Vector3 lastPositionalVelocity = (lastPosition.Vector - beforeLastPosition.Vector) / (lastPosition.Time - beforeLastPosition.Time);
                Vector3 curPositionalVelocity = (subject.transform.position - lastPosition.Vector) / (time - lastPosition.Time);

                if (V3Equal(lastPositionalVelocity, curPositionalVelocity))
                {
                    capturedPositions.RemoveLast();
                }
            }
            capturedPositions.AddLast(new VectorCapture(time, subject.transform.position));


            if (capturedRotations.Count > 1)
            {
                VectorCapture beforeLastRotation = capturedRotations.Last.Previous.Value;
                VectorCapture lastRotation = capturedRotations.Last.Value;

                Vector3 lastRotationalVelocity = (lastRotation.Vector - beforeLastRotation.Vector) / (lastRotation.Time - beforeLastRotation.Time);
                Vector3 curRotationalVelocity = (subject.transform.rotation.eulerAngles - lastRotation.Vector) / (time - lastRotation.Time);

                if (V3Equal(lastRotationalVelocity, curRotationalVelocity))
                {
                    capturedRotations.RemoveLast();
                }
            }
            capturedRotations.AddLast(new VectorCapture(time, subject.transform.rotation.eulerAngles));
        }

        private bool V3Equal(Vector3 a, Vector3 b)
        {
            return Vector3.SqrMagnitude(a - b) <= minimumDelta;
        }

        /// <summary>
        /// Logs a lifecycle event at a given time.
        /// </summary>
        /// <param name="time">Time of the event.</param>
        /// <param name="lifeCycleEvent">The lifecycle event to log.</param>
        public void CaptureUnityEvent(float time, UnityLifeCycleEvent lifeCycleEvent)
        {
            capturedLifeCycleEvents.AddLast(new UnityLifeCycleEventCapture(time, lifeCycleEvent));
        }

        private VectorCapture[] InterpolateFilterAndShift(LinkedList<VectorCapture> original, float startTime, float endTime, IEnumerable<Vector2> pauseSlices)
        {
            var newCaptures = new List<VectorCapture>();
            bool? lastCaptureWasInPause = null;

            var captureNode = original.First;
            Vector2 happenedInPauseRange = Vector2.zero;
            while (captureNode != null)
            {
                var capture = captureNode.Value;
                if (capture.FallsWithin(startTime, endTime))
                {
                    bool happenedInPause = false;

                    float cumulativePauseTime = 0f;
                    foreach (var pause in pauseSlices)
                    {
                        if (capture.FallsWithin(pause.x, pause.y))
                        {
                            happenedInPauseRange = pause;
                            happenedInPause = true;
                        }
                        else if (pause.y > startTime && pause.y <= capture.Time)
                        {
                            cumulativePauseTime += (pause.y - pause.x);
                        }
                    }
                    if (happenedInPause)
                    {
                        if (lastCaptureWasInPause == false)
                        {
                            // We jumped from playing state to paused state, we must interpolate
                            var previous = captureNode.Previous.Value;
                            var progress = (happenedInPauseRange.x - previous.Time) / (capture.Time - previous.Time);
                            newCaptures.Add(new VectorCapture(happenedInPauseRange.x - cumulativePauseTime, Vector3.Lerp(previous.Vector, capture.Vector, progress)));
                        }
                    }
                    else
                    {
                        if (lastCaptureWasInPause == true)
                        {
                            // switched from being in pause to playing, must interpolate
                            var previous = captureNode.Previous.Value;
                            var progress = (happenedInPauseRange.y - previous.Time) / (capture.Time - previous.Time);
                            newCaptures.Add(new VectorCapture(happenedInPauseRange.y - cumulativePauseTime, Vector3.Lerp(previous.Vector, capture.Vector, progress)));
                        }
                        newCaptures.Add(capture.SetTime(capture.Time - cumulativePauseTime) as VectorCapture);
                    }
                    lastCaptureWasInPause = happenedInPause;
                }
                captureNode = captureNode.Next;
            }
            return newCaptures.ToArray();
        }

        private T[] Squash<T>(IEnumerable<T> original, float startTime, float endTime, IEnumerable<Vector2> pauseSlices) where T : Capture
        {
            var filtered = new List<T>();
            foreach (var capture in original)
            {
                if (capture.FallsWithin(startTime, endTime))
                {
                    float? timeInPause = null;
                    float cumulativePauseTime = 0f;
                    foreach (var pause in pauseSlices)
                    {
                        if (capture.FallsWithin(pause.x, pause.y))
                        {
                            timeInPause = pause.x;
                        }
                        else if (pause.y > startTime && pause.y <= capture.Time)
                        {
                            cumulativePauseTime += (pause.y - pause.x);
                        }
                    }
                    filtered.Add(capture.SetTime((timeInPause == null ? capture.Time : (float)timeInPause) - cumulativePauseTime) as T);
                }
            }
            return filtered.ToArray();
        }

        /// <summary>
        /// Takes what events have been witnessed up until now and builds a recording from them.
        /// </summary>
        /// <param name="startTime">The minimum timestamp the events must have.</param>
        /// <param name="endTime">The maximum timestamp the events can have.</param>
        /// <param name="pauseSlices">Any time ranges you want to exclude events from in the final recording.</param>
        /// <returns></returns>
        public SubjectRecording Save(float startTime, float endTime, IEnumerable<Vector2> pauseSlices)
        {
            return new SubjectRecording(
                instanceId,
                name,
                metadata,
                InterpolateFilterAndShift(capturedPositions, startTime, endTime, pauseSlices),
                InterpolateFilterAndShift(capturedRotations, startTime, endTime, pauseSlices),
                Squash(capturedLifeCycleEvents, startTime, endTime, pauseSlices),
                CaptureUtil.FilterAndShift(capturedCustomActorEvents, startTime, endTime, pauseSlices));
        }

    }

}
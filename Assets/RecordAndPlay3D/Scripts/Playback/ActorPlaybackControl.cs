using UnityEngine;

namespace EliCDavis.RecordAndPlay.Playback
{

    /// <summary>
    /// Takes care of animating the actor through their recording.
    /// </summary>
    public class ActorPlaybackControl
    {
        /// <summary>
        /// The "actor" that represents the subject that was recoreded. What
        /// we'll be animating
        /// </summary>
        private GameObject representation;

        private IPlaybackCustomEventHandler customEventHandler;

        /// <summary>
        /// The recording that dictates what our actor will be doing.
        /// </summary>
        private SubjectRecording subjectRecording;

        /// <summary>
        /// Overall recording the subject belongs too
        /// </summary>
        private Recording recording;

        /// <summary>
        /// How many seconds have passed in the playback recording
        /// </summary>
        private float lastTimeThroughPlayback;

        private int lastPositionIndex;

        private int lastRotationIndex;

        private bool hasStartEvent;

        /// <summary>
        /// Build a new ActorPlaybackControl.
        /// </summary>
        /// <param name="representation">The actor meant to represent the subject during playback.</param>
        /// <param name="customEventHandler">Handles custom events that occurs to the subject in the recording.</param>
        /// <param name="recording">The recording containing this subject's recording and potentially others.</param>
        /// <param name="subjectRecording">The recording we will be acting out with the representaion provided.</param>
        public ActorPlaybackControl(GameObject representation, IPlaybackCustomEventHandler customEventHandler, Recording recording, SubjectRecording subjectRecording)
        {
            this.representation = representation;
            this.customEventHandler = customEventHandler;
            this.recording = recording;
            this.subjectRecording = subjectRecording;
            hasStartEvent = HasStartEvent();
            lastTimeThroughPlayback = 0;
            lastPositionIndex = 0;
        }

        /// <summary>
        /// Removes the representation of the actor from the scene.
        /// </summary>
        /// <param name="immediate">Whether or not to use DestroyImmediate or just Destroy.</param>
        public void DestroyRepresentation(bool immediate)
        {
            if (immediate)
            {
                Object.DestroyImmediate(representation);
            }
            else
            {
                Object.Destroy(representation);
            }

        }

        /// <summary>
        /// Set's the current time the playback is in without any of the interpolation.
        /// </summary>
        /// <param name="time">The time to move the playback to.</param>
        public void SetTime(float time)
        {
            lastTimeThroughPlayback = time;
        }


        private void MovePositionTo(float newTime, int direction)
        {
            if (subjectRecording.CapturedPositions == null || subjectRecording.CapturedPositions.Length == 0)
            {
                return;
            }

            var currentOrientationIndex = OrientationIndex(newTime, direction, lastPositionIndex, subjectRecording.CapturedPositions);
            var currentOrientation = subjectRecording.CapturedPositions[currentOrientationIndex];
            var nextOrientation = subjectRecording.CapturedPositions[Mathf.Clamp(currentOrientationIndex + direction, 0, subjectRecording.CapturedPositions.Length - 1)];

            var newPos = Vector3.Lerp(
                currentOrientation.Vector,
                nextOrientation.Vector,
                ProgressThroughOrientation(currentOrientationIndex, newTime, direction, subjectRecording.CapturedPositions)
            );
            newPos.Scale(representation.transform.parent.localScale);

            representation.transform.localPosition = newPos;

            lastPositionIndex = currentOrientationIndex;
        }

        private void MoveRotationTo(float newTime, int direction)
        {
            if (subjectRecording.CapturedRotations == null || subjectRecording.CapturedRotations.Length == 0)
            {
                return;
            }

            var currentOrientationIndex = OrientationIndex(newTime, direction, lastRotationIndex, subjectRecording.CapturedRotations);
            var currentOrientation = subjectRecording.CapturedRotations[currentOrientationIndex];
            var nextOrientation = subjectRecording.CapturedRotations[Mathf.Clamp(currentOrientationIndex + direction, 0, subjectRecording.CapturedRotations.Length - 1)];

            representation.transform.localRotation = Quaternion.Slerp(
                Quaternion.Euler(currentOrientation.Vector),
                Quaternion.Euler(nextOrientation.Vector),
                ProgressThroughOrientation(currentOrientationIndex, newTime, direction, subjectRecording.CapturedRotations)
            );

            lastRotationIndex = currentOrientationIndex;
        }

        private void PlayEventsThatTranspired(float startingTime, float transpired)
        {
            if (customEventHandler == null)
            {
                return;
            }

            float adjustedStartTime = recording.GetStartTime() + startingTime;
            foreach (var customEvent in subjectRecording.CapturedCustomEvents)
            {
                if (customEvent.FallsWithin(adjustedStartTime, adjustedStartTime + transpired))
                {
                    customEventHandler.OnCustomEvent(subjectRecording, customEvent);
                }
            }
        }

        private void MoveTo(float newTime, bool playCustomEvents)
        {
            float deltaTime = newTime - lastTimeThroughPlayback;
            int direction = CalcualteDirection(deltaTime);

            if (direction != 0)
            {
                representation.SetActive(
                    DisableDueToLastLifeCycleEvent(
                        LastLifeCycleEvent(newTime, direction)) == false);

                MovePositionTo(newTime, direction);
                MoveRotationTo(newTime, direction);

                if (playCustomEvents)
                {
                    PlayEventsThatTranspired(lastTimeThroughPlayback, deltaTime);
                }
            }

            lastTimeThroughPlayback = newTime;
        }

        /// <summary>
        /// Interpolates the current actor's orientation with the new one deduced from the time provided. Any custom events that occur from the original time and the new time passed in are <b>executed</b> through the provided custom event handler.
        /// </summary>
        /// <param name="newTime">The time the orientation should represent through the playback.</param>
        public void MoveTo(float newTime)
        {
            MoveTo(newTime, true);
        }

        /// <summary>
        /// Interpolates the current actor's orientation with the new one deduced from the time provided. Any custom events that occur between the original time and the new time passed in are <b>ignored</b>.
        /// </summary>
        /// <param name="newTime">The time the orientation should represent through the playback.</param>
        public void SkipTo(float newTime)
        {
            MoveTo(newTime, false);
        }

        private bool DisableDueToLastLifeCycleEvent(UnityLifeCycleEventCapture e)
        {
            if (e == null && hasStartEvent)
            {
                return true;
            }
            return e != null && (e.LifeCycleEvent == UnityLifeCycleEvent.Disable || e.LifeCycleEvent == UnityLifeCycleEvent.Destroy);
        }

        private bool HasStartEvent()
        {
            for (int i = 0; i < subjectRecording.CapturedLifeCycleEvents.Length; i++)
            {
                if (subjectRecording.CapturedLifeCycleEvents[i].LifeCycleEvent == UnityLifeCycleEvent.Start)
                {
                    return true;
                }
            }
            return false;
        }

        private UnityLifeCycleEventCapture LastLifeCycleEvent(float newTime, int direction)
        {
            if (direction == 0)
            {
                throw new System.Exception("You can't adjust to lifecycle events when time isn't moving");
            }

            UnityLifeCycleEventCapture lastEvent = null;
            for (int i = 0; i < subjectRecording.CapturedLifeCycleEvents.Length; i++)
            {
                if (newTime < subjectRecording.CapturedLifeCycleEvents[i].Time - recording.GetStartTime())
                {
                    return lastEvent;
                }
                lastEvent = subjectRecording.CapturedLifeCycleEvents[i];
            }

            return lastEvent;
        }

        private int OrientationIndex(float time, int direction, int lastOrientation, Capture[] captures)
        {
            if (direction == 1)
            {
                int greaterThan = lastOrientation;
                for (int i = lastOrientation; i < captures.Length; i++)
                {
                    if (time < captures[i].Time - recording.GetStartTime())
                    {
                        return Mathf.Clamp(i - 1, 0, captures.Length - 1);
                    }
                    greaterThan = i;
                }
                return greaterThan;
            }
            else if (direction == -1)
            {
                int greaterThan = lastOrientation;
                for (int i = lastOrientation; i >= 0; i--)
                {
                    if (time > captures[i].Time - recording.GetStartTime())
                    {
                        return Mathf.Clamp(i + 1, 0, captures.Length - 1);
                    }
                    greaterThan = i;
                }
                return greaterThan;
            }
            throw new System.Exception("Direction is 0");
        }

        private int OrientationIndex(float time, int direction, Capture[] captures)
        {
            if (direction == 1)
            {
                int greaterThan = 0;
                for (int i = 0; i < captures.Length; i++)
                {
                    if (time < captures[i].Time - recording.GetStartTime())
                    {
                        return Mathf.Clamp(i - 1, 0, captures.Length - 1);
                    }
                    greaterThan = i;
                }
                return greaterThan;
            }
            else if (direction == -1)
            {
                int greaterThan = captures.Length - 1;
                for (int i = captures.Length - 1; i >= 0; i--)
                {
                    if (time > captures[i].Time - recording.GetStartTime())
                    {
                        return Mathf.Clamp(i + 1, 0, captures.Length - 1);
                    }
                    greaterThan = i;
                }
                return greaterThan;
            }
            throw new System.Exception("Direction is 0");
        }

        private int CalcualteDirection(float deltaTime)
        {
            if (deltaTime > 0)
            {
                return 1;
            }
            else if (deltaTime < 0)
            {
                return -1;
            }
            return 0;
        }

        private float ProgressThroughOrientation(int orientationIndex, float time, int dir, Capture[] captures)
        {
            int nextOrientation = Mathf.Clamp(orientationIndex + dir, 0, captures.Length - 1);
            if (nextOrientation == orientationIndex)
            {
                return 1f;
            }
            return (time - (captures[orientationIndex].Time - recording.GetStartTime())) / FrameDuration(orientationIndex, nextOrientation, captures);
        }

        private float FrameDuration(int frameIndex, int nextFrameIndex, Capture[] captures)
        {
            return captures[nextFrameIndex].Time - captures[frameIndex].Time;
        }

    }

}
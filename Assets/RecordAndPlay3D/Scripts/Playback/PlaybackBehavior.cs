using UnityEngine;

namespace EliCDavis.RecordAndPlay.Playback
{

    /// <summary>
    /// This is used for animating a recording inside the scene.
    /// </summary>
    /// <remarks>
    /// I would recommend using <a page="EliCDavis.RecordAndPlay.Playback.PlaybackBehavior.Build(Recording)">Build()</a> for setting up the playback for a recording.
    /// </remarks>
    public class PlaybackBehavior : MonoBehaviour
    {

        /// <summary> 
        /// The current state we're in that determines whether or not we're animating actors
        /// </summary>
        private PlaybackState currentPlaybackState;

        private PlaybackStateInfo stateInfo;

        /// <summary>
        /// Creates an empty GameObject and attatches an instance of the PlaybackBehavior to it.
        /// </summary>
        /// <param name="recording">The recording to playback.</param>
        /// <param name="actorBuilder">What is used to build the actors.</param>
        /// <param name="onEvent">What to call when an event occurs.</param>
        /// <param name="loop">Whether or not for the recoding playback to start over when it reaches the end.</param>
        /// <returns></returns>
        public static PlaybackBehavior Build(Recording recording, IActorBuilder actorBuilder, IPlaybackCustomEventHandler onEvent, bool loop)
        {
            PlaybackBehavior playbackBehavior = new GameObject("PLAYBACK OBJECT").AddComponent<PlaybackBehavior>();

            playbackBehavior.stateInfo = new PlaybackStateInfo(recording, actorBuilder, onEvent, playbackBehavior.transform, loop);
            playbackBehavior.currentPlaybackState = new StoppedPlaybackState(playbackBehavior.stateInfo);

            return playbackBehavior;
        }

        /// <summary>
        /// Creates an empty GameObject and attatches an instance of the PlaybackBehavior to it.
        /// </summary>
        /// <param name="recording">The recording to playback.</param>
        public static PlaybackBehavior Build(Recording recording)
        {
            return Build(recording, null, null, false);
        }

        /// <summary>
        /// Creates an empty GameObject and attatches an instance of the PlaybackBehavior to it.
        /// </summary>
        /// <param name="recording">The recording to playback.</param>
        /// <param name="loop">Whether or not for the recoding playback to start over when it reaches the end.</param>
        public static PlaybackBehavior Build(Recording recording, bool loop)
        {
            return Build(recording, null, null, loop);
        }

        /// <summary>
        /// Start reenacting a recording.
        /// </summary>
        public void Play()
        {
            currentPlaybackState = currentPlaybackState.Play();
        }

        /// <summary>
        /// Stops the loaded recording. Actor representations are removed through <a page="https://docs.unity3d.com/ScriptReference/Object.Destroy.html">Destroy()</a>.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when there is no playback occuring.</exception>
        public void Stop()
        {
            if (currentPlaybackState == null)
            {
                throw new System.InvalidOperationException("Trying to stop a recording when one isn't loaded yet!");
            }
            Stop(false);
        }

        /// <summary>
        /// Stops the loaded recording.
        /// </summary>
        /// <param name="immediate">Uses <a page="https://docs.unity3d.com/ScriptReference/Object.DestroyImmediate.html">DestroyImmediate()</a> if true, else <a page="https://docs.unity3d.com/ScriptReference/Object.Destroy.html">Destroy()</a>.</param>
        /// <exception cref="System.InvalidOperationException">Thrown when there is no playback occuring.</exception>
        public void Stop(bool immediate)
        {
            if (currentPlaybackState == null)
            {
                throw new System.InvalidOperationException("Trying to stop a recording when one isn't loaded yet!");
            }

            currentPlaybackState = currentPlaybackState.Stop(immediate);
        }

        /// <summary>
        /// If a recording is playing, then it will become paused.
        /// </summary>
        public void Pause()
        {
            if (currentPlaybackState == null)
            {
                throw new System.InvalidOperationException("Trying to pause a recording when one isn't loaded yet!");
            }
            currentPlaybackState = currentPlaybackState.Pause();
        }

        /// <summary>
        /// Whether or not the playback service is currentely active.
        /// </summary>
        /// <returns><c>true</c>, if currentely playing, <c>false</c> otherwise.</returns>
        public bool CurrentlyPlaying()
        {
            return currentPlaybackState.GetType() == typeof(PlayingPlaybackState);
        }

        /// <summary>
        /// Returns whether or not the current recording loaded is paused or not.
        /// </summary>
        /// <returns><c>true</c>, if currentely paused, <c>false</c> otherwise.</returns>
        public bool CurrentlyPaused()
        {
            return currentPlaybackState.GetType() == typeof(PausedPlaybackState);
        }

        /// <summary>
        /// Returns whether or not the current recording loaded is stopped or not.
        /// </summary>
        /// <returns><c>true</c>, if currentely stopped, <c>false</c> otherwise.</returns>
        public bool CurrentlyStopped()
        {
            return currentPlaybackState.GetType() == typeof(StoppedPlaybackState);
        }

        /// <summary>
        /// The current playback speed.
        /// </summary>
        /// <returns>The current playback speed.</returns>
        public float GetPlaybackSpeed()
        {
            return stateInfo.GetPlaybackSpeed();
        }

        /// <summary>
        /// Changes the speed in which the recording will be played back as. A positive speed means the recording processes normally. A negative speend means the recording processes backwards. A speed of 0 means the recording does not budge. A speed of 1 is default.
        /// </summary>
        /// <param name="newSpeed">The speed for the playback to run at.</param>
        public void SetPlaybackSpeed(float newSpeed)
        {
            stateInfo.SetPlaybackSpeed(newSpeed);
        }

        /// <summary>
        /// Sets the time we want the playback to be at in the recording.
        /// Useful for seeking.
        /// </summary>
        /// <param name="time">Time.</param>
        public void SetTimeThroughPlayback(float time)
        {
            currentPlaybackState.SetTimeThroughPlayback(time);
        }

        /// <summary>
        /// Gets the length of recording.
        /// </summary>
        /// <returns>The length of recording.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when there is no playback occuring.</exception>
        public float RecordingDuration()
        {
            if (currentPlaybackState == null)
            {
                throw new System.InvalidOperationException("Can't get durration if there's no recording");
            }
            return currentPlaybackState.GetDuration();
        }

        /// <summary>
        /// The time in seconds of how long the recording has been playing.
        /// </summary>
        /// <returns>The time through recording.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when there is no playback occuring.</exception>
        public float GetTimeThroughPlayback()
        {
            if (currentPlaybackState == null)
            {
                throw new System.InvalidOperationException("Can't get durration if there's no recording");
            }
            return currentPlaybackState.GetTimeThroughPlayback();
        }

        /// <summary>
        /// The recording that is being played back.
        /// </summary>
        /// <returns>The recording being animated.</returns>
        public Recording GetRecording()
        {
            return stateInfo.GetRecording();
        }

        /// <summary>
        /// Progresses the playback. You probably don't want to call this.
        /// </summary>
        public void UpdateState()
        {
            currentPlaybackState = currentPlaybackState.Update();
        }

        void Update()
        {
            if (currentPlaybackState == null)
            {
                throw new System.Exception("Somehow got null playback state");
            }
            UpdateState();
        }

    }

}
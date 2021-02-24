using UnityEngine;

namespace EliCDavis.RecordAndPlay.Playback
{

    internal class PlayingPlaybackState : PlaybackState
    {
        /// <summary>
        /// The time of the last frame.
        /// </summary>
        private float lastRealTime = -1;

        public PlayingPlaybackState(PlaybackStateInfo playbackStateInfo, float startingTime) : base(playbackStateInfo)
        {
            if (playbackStateInfo.GetActors() == null)
            {
                playbackStateInfo.LoadActorsFromRecording();
            }

            playbackStateInfo.SetTime(startingTime);

            foreach (var actor in playbackStateInfo.GetActors())
            {
                actor.SetTime(startingTime);
            }
        }

        public PlayingPlaybackState(PlaybackStateInfo playbackStateInfo) : this(playbackStateInfo, 0) { }

        public override void SetTimeThroughPlayback(float time)
        {
            playbackStateInfo.SetTime(time);
            UpdateActorTransforms();
        }

        public override PlaybackState Pause()
        {
            return new PausedPlaybackState(playbackStateInfo);
        }

        public override PlaybackState Play()
        {
            return new PlayingPlaybackState(playbackStateInfo);
        }

        public override PlaybackState Stop(bool immediate)
        {
            playbackStateInfo.ClearActors(immediate);
            return new StoppedPlaybackState(playbackStateInfo);
        }

        public override PlaybackState Update()
        {
            float timeTranspired = TrueDeltaTime() * playbackStateInfo.GetPlaybackSpeed();
            PlayEventsThatTranspired(playbackStateInfo.GetTimeThroughPlayback(), timeTranspired);

            playbackStateInfo.IncrementTimeThroughPlayback(timeTranspired);

            if (GetTimeThroughPlayback() > GetDuration())
            {
                if (playbackStateInfo.ShouldLoop() == false)
                {
                    return new PausedPlaybackState(playbackStateInfo);
                }
                playbackStateInfo.SetTime(0);
                ResetActorTransformsToPointInTime(playbackStateInfo.GetTimeThroughPlayback());
            }
            else if (GetTimeThroughPlayback() < 0)
            {
                if (playbackStateInfo.ShouldLoop() == false)
                {
                    return new PausedPlaybackState(playbackStateInfo);
                }
                playbackStateInfo.SetTime(GetDuration());
                ResetActorTransformsToPointInTime(playbackStateInfo.GetTimeThroughPlayback());
            }
            else
            {
                UpdateActorTransforms();
            }

            lastRealTime = Time.realtimeSinceStartup;

            return this;
        }

        private void PlayEventsThatTranspired(float startingTime, float transpired)
        {
            if (playbackStateInfo.GetCustomEventHandler() == null)
            {
                return;
            }
            float adjustedStartTime = playbackStateInfo.GetRecording().GetStartTime() + startingTime;
            foreach (var customEvent in playbackStateInfo.GetRecording().CapturedCustomEvents)
            {
                if (customEvent.FallsWithin(adjustedStartTime, adjustedStartTime + transpired))
                {
                    playbackStateInfo.GetCustomEventHandler().OnCustomEvent(null, customEvent);
                }
            }
        }

        /// <summary>
        /// The actual movement of the actors in the playback from one position and
        /// rotation to another.
        /// </summary>
        private void UpdateActorTransforms()
        {
            foreach (var actor in playbackStateInfo.GetActors())
            {
                actor.MoveTo(playbackStateInfo.GetTimeThroughPlayback());
            }
        }

        /// <summary>
        /// Called When their are abrupt changes in the playbacks duration, not
        /// due to the animation process.  
        /// </summary>
        private void ResetActorTransformsToPointInTime(float pointInTime)
        {
            foreach (var actor in playbackStateInfo.GetActors())
            {
                actor.SkipTo(pointInTime);
            }
        }

        /// <summary>
        /// Returns the actual time since the start of Unity, instead of just the scene.
        /// Used so that playback can occur within the editor without having the need
        /// for the scene to be played.
        /// </summary>
        /// <returns>The delta time.</returns>
        private float TrueDeltaTime()
        {
            return lastRealTime == -1 ? 0 : Time.realtimeSinceStartup - lastRealTime;
        }
    }

}
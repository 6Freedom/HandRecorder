namespace EliCDavis.RecordAndPlay.Playback
{

    internal abstract class PlaybackState
    {

        protected PlaybackStateInfo playbackStateInfo;

        public PlaybackState(PlaybackStateInfo playbackStateInfo)
        {
            if (playbackStateInfo == null)
            {
                throw new System.ArgumentException("Can't have null playback state info");
            }
            this.playbackStateInfo = playbackStateInfo;
        }

        public abstract PlaybackState Pause();

        public abstract PlaybackState Play();

        public abstract PlaybackState Stop(bool immediate);

        public abstract PlaybackState Update();

        public abstract void SetTimeThroughPlayback(float time);

        public float GetTimeThroughPlayback()
        {
            return playbackStateInfo.GetTimeThroughPlayback();
        }

        public float GetDuration()
        {
            return playbackStateInfo.GetDuration();
        }

    }

}
namespace EliCDavis.RecordAndPlay.Playback
{

    internal class StoppedPlaybackState : PlaybackState
    {
        public StoppedPlaybackState(PlaybackStateInfo playbackStateInfo) : base(playbackStateInfo)
        {
        }

        public override PlaybackState Pause()
        {
            return this;
        }

        public override PlaybackState Play()
        {
            return new PlayingPlaybackState(playbackStateInfo);
        }

        public override PlaybackState Stop(bool immediate)
        {
            return this;
        }

        public override PlaybackState Update()
        {
            return this;
        }

        public override void SetTimeThroughPlayback(float time)
        {
        }
    }

}
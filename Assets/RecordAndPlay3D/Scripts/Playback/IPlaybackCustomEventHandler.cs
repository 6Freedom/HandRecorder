
namespace EliCDavis.RecordAndPlay.Playback
{

    /// <summary>
    /// Implement this interface if you want your class to be used for handeling custom events that are contained in the recording.
    /// </summary>
    /// <remarks>
    /// If the subject passed in is null, then the event is a global one.
    /// </remarks>
    public interface IPlaybackCustomEventHandler 
    {
        /// <summary>
        /// Called while the playback of a recording is occuring when a custom event occurs.
        /// </summary>
        /// <param name="subject">The subject the event is related too, null if it is a global event.</param>
        /// <param name="customEvent">The custom event.</param>
        void OnCustomEvent(SubjectRecording subject, CustomEventCapture customEvent);
    }
}
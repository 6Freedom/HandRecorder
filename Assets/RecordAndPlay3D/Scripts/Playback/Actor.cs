using UnityEngine;

/// <summary>
/// Everything relevant to building playback for a recording.
/// </summary>
namespace EliCDavis.RecordAndPlay.Playback
{

    /// <summary>
    /// Consists of the GameObject meant to represent the subject and an event handler for the custom events that occured during a recording.
    /// </summary>
    public class Actor
    {
        private GameObject representation;

        private IPlaybackCustomEventHandler customEventHandler;

        /// <summary>
        /// Create a new actor.
        /// </summary>
        /// <param name="representation">The object to represent the subject.</param>
        /// <param name="customEventHandler">The handler for all events that occur to the specific subject.</param>
        public Actor(GameObject representation, IPlaybackCustomEventHandler customEventHandler)
        {
            this.representation = representation;
            this.customEventHandler = customEventHandler;
        }

        /// <summary>
        /// Create a new actor with a null event handler.
        /// </summary>
        /// <param name="representation">The object to represent the subject.</param>
        public Actor(GameObject representation)
        {
            this.representation = representation;
            this.customEventHandler = null;
        }

        /// <summary>
        /// The GameObject in the scene meant to represent the subject in the recording.
        /// </summary>
        public GameObject Representation { get { return representation; } }

        /// <summary>
        /// The custom event handler called during playback.
        /// </summary>
        public IPlaybackCustomEventHandler CustomEventHandler { get { return customEventHandler; } }
    }
}
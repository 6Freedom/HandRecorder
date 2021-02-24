using UnityEngine;

namespace EliCDavis.RecordAndPlay
{

    /// <summary>
    /// An event that represents an event that occurs to a monobehavior instance.
    /// </summary>
    [System.Serializable]
    public class UnityLifeCycleEventCapture : Capture
    {
        [SerializeField]
        UnityLifeCycleEvent lifeCycleEvent;

        /// <summary>
        /// Create a new UnityLifeCycleEventCapture.
        /// </summary>
        /// <param name="time">The time the event occured in the recording.</param>
        /// <param name="lifeCycleEvent">The lifecycle event that occured</param>
        public UnityLifeCycleEventCapture(float time, UnityLifeCycleEvent lifeCycleEvent) : base(time)
        {
            this.lifeCycleEvent = lifeCycleEvent;
        }

        /// <summary>
        /// The lifecycle event captured at some point in time.
        /// </summary>
        public UnityLifeCycleEvent LifeCycleEvent
        {
            get
            {
                return lifeCycleEvent;
            }
        }

        /// <summary>
        /// Creates a new UnityLifeCycleEventCapture with lifecycle event but with a modified time, leaving the original event unchanged.
        /// </summary>
        /// <param name="newTime">The new time the event occured in the recording.</param>
        /// <returns>A entirely new capture that occured with the time passed in.</returns>
        public override Capture SetTime(float newTime)
        {
            return new UnityLifeCycleEventCapture(newTime, lifeCycleEvent);
        }

        /// <summary>
        /// Builds a JSON string that represents the UnityLifeCycleEventCapture object.
        /// </summary>
        /// <returns>A JSON string.</returns>
        public override string ToJSON() { return string.Format("{{ \"Time\": {0}, \"Type\": \"{1}\" }}", Time, lifeCycleEvent.ToString()); }

        /// <summary>
        /// Builds a string that represents a single row in a csv file that contains this object's data.
        /// </summary>
        /// <returns>A row of csv data as a string.</returns>
        public override string ToCSV() { return string.Format("{0}, {1}", Time, lifeCycleEvent.ToString()); }

    }

}
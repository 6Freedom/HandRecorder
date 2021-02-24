using UnityEngine;

namespace EliCDavis.RecordAndPlay
{

    /// <summary>
    /// Encompasses an event that occured at a certian point in time during a recording. Meant to be immutable.
    /// </summary>
    public abstract class Capture
    {

        [SerializeField]
        float time;

        public Capture(float time)
        {
            this.time = time;
        }

        /// <summary>
        /// The time the event was captured in the recording.
        /// </summary>
        public float Time { get { return time; } }

        /// <summary>
        /// Figure out if the captured event occurs within the time slice provided. The range is inclusive [start, end]. If the range is provided backwards such that the start of the timeslice occurs after the end (start > end), then the values are automatically flipped and the function continues.
        /// </summary>
        /// <param name="start">The start of the timeslice.</param>
        /// <param name="end">The end of the timeslice.</param>
        /// <returns>Whether or not the event occurs within the timeslice.</returns>
        public bool FallsWithin(float start, float end)
        {
            if (end < start)
            {
                return FallsWithin(end, start);
            }
            return time >= start && time <= end;
        }

        /// <summary>
        /// Figure out if the captured event occurs within the time slice provided. The range is inclusive [timeSlice.x, timeSlice.y]. If the range is provided backwards such that the start of the timeslice occurs after the end (timeSlice.x > timeSlice.y), then the values are automatically flipped and the function continues.
        /// </summary>
        /// <param name="timeSlice">The timeslice where the x component is the start and the y component is the end.</param>
        /// <returns>Whether or not the event occurs within the timeslice</returns>
        public bool FallsWithin(Vector2 timeSlice)
        {
           return FallsWithin(timeSlice.x, timeSlice.y);
        }
        
        /// <summary>
        /// Builds a new capture object that has a modified capture time. The original capture object is left unchanged.
        /// </summary>
        /// <param name="newTime">The new time the capture occured.</param>
        /// <returns>A new capture object with the capture time set to the value passed in.</returns>
        public abstract Capture SetTime(float newTime);

        /// <summary>
        /// Builds a JSON string that represents the Capture.
        /// </summary>
        /// <returns>A JSON string.</returns>
        public abstract string ToJSON();

        /// <summary>
        /// Builds a string that represents a single row in a csv file that contains this object's data.
        /// </summary>
        /// <returns>A row of csv data as a string.</returns>
        public abstract string ToCSV();

    }

}
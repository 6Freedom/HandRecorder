using UnityEngine;

namespace EliCDavis.RecordAndPlay
{

    /// <summary>
    /// An event that is meant to store a vector value that was present at some point in time. Used to represent things such as position and rotation.
    /// </summary>
    [System.Serializable]
    public class VectorCapture : Capture
    {
        [SerializeField]
        Vector3 vector;

        /// <summary>
        /// Creates a new capture of a vector value in a point in time.
        /// </summary>
        /// <param name="time">A point in time the vector was observed.</param>
        /// <param name="vector">The value of the vector observed.</param>
        public VectorCapture(float time, Vector3 vector) : base(time)
        {
            this.vector = vector;
        }

        /// <summary>
        /// The value of the vector captured at some point in time.
        /// </summary>
        public Vector3 Vector
        {
            get
            {
                return vector;
            }
        }

        /// <summary>
        /// Creates a new VectorCapture with the vector value but with a modified time, leaving the original event unchanged.
        /// </summary>
        /// <param name="newTime">The new time the event occured in the recording.</param>
        /// <returns>A entirely new capture that occured with the time passed in.</returns>
        public override Capture SetTime(float newTime)
        {
            return new VectorCapture(newTime, vector);
        }

        /// <summary>
        /// Builds a JSON string that represents the VectorCapture object.
        /// </summary>
        /// <returns>A JSON string.</returns>
        public override string ToJSON()
        {
            return string.Format(
                "{{ \"Time\": {0}, \"X\": {1}, \"Y\": {2}, \"Z\": {3} }}",
                Time,
                vector.x,
                vector.y,
                vector.z
            );
        }

        /// <summary>
        /// Builds a string that represents a single row in a csv file that contains this object's data.
        /// </summary>
        /// <returns>A row of csv data as a string.</returns>
        public override string ToCSV()
        {
            return string.Format(
                "{0}, {1}, {2}, {3}",
                Time,
                vector.x,
                vector.y,
                vector.z
            );
        }
    }

}
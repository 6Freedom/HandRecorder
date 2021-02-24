using UnityEngine;
using EliCDavis.RecordAndPlay.Util;

namespace EliCDavis.RecordAndPlay
{
    /// <summary>
    /// An event that acts as a key value pair with an associated timestamp.
    /// </summary>
    [System.Serializable]
    public class CustomEventCapture : Capture
    {
        [SerializeField]
        string name;

        [SerializeField]

        string contents;
        
        /// <summary>
        /// Create a new custom event capture.
        /// </summary>
        /// <param name="time">The time the event occured in the recording.</param>
        /// <param name="name">The name of the event.</param>
        /// <param name="contents">The details of the event that occured.</param>
        public CustomEventCapture(float time, string name, string contents) : base(time)
        {
            this.name = name;
            this.contents = contents;
        }

        /// <summary>
        /// The details of the event
        /// </summary>
        public string Contents { get { return contents; } }

        /// <summary>
        /// The name of the event
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Creates a new CustomEventCapture with the same name and details but with a modified time, leaving the original event unchanged.
        /// </summary>
        /// <param name="newTime">The new time the event occured in the recording.</param>
        /// <returns>A entirely new capture that occured with the time passed in.</returns>
        public override Capture SetTime(float newTime)
        {
            return new CustomEventCapture(newTime, name, contents);
        }

        /// <summary>
        /// Builds a JSON string that represents the CustomEventCapture object.
        /// </summary>
        /// <returns>A JSON string.</returns>
        public override string ToJSON()
        {
            return string.Format(
                "{{ \"Time\": {0}, \"Name\": \"{1}\", \"Contents\": \"{2}\" }}",
                Time,
                FormattingUtil.JavaScriptStringEncode(Name),
                FormattingUtil.JavaScriptStringEncode(Contents)
            );
        }

        /// <summary>
        /// Builds a string that represents a single row in a csv file that contains this object's data.
        /// </summary>
        /// <returns>A row of csv data as a string.</returns>
        public override string ToCSV()
        {
            return string.Format(
                "{0}, {1}, {2}",
                Time,
                FormattingUtil.StringToCSVCell(Name),
                FormattingUtil.StringToCSVCell(Contents)
            );
        }
    }
}
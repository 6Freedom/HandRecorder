using UnityEngine;
using System.Collections.Generic;
using EliCDavis.RecordAndPlay.Record;

/// <summary>
/// Different Utility functions to aid the rest of Record And Play. You should not care about this nampespace at all unless you are trying to modify how the library acts as a whole.
/// </summary>
///#IGNORE
namespace EliCDavis.RecordAndPlay.Util
{

    /// <summary>
    /// A utility class for massaging capture data.
    /// </summary>
    public static class CaptureUtil
    {
        /// <summary>
        /// If an event happens during a paused timeslice, we need to remove it entirely. If the event happens after a pause timeslice, then we need to shift it over towards 0 by the sum of all pause durations that came previously.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pauseSlices"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>An array of capture data that passes the criteria provided.</returns>
        public static T[] FilterAndShift<T>(IEnumerable<T> original, float startTime, float endTime, IEnumerable<Vector2> pauseSlices) where T : Capture
        {
            var filtered = new List<T>();
            foreach (var capture in original)
            {
                if (capture.FallsWithin(startTime, endTime))
                {
                    var happenedInPause = false;
                    float cumulativePauseTime = 0f;
                    foreach (var pause in pauseSlices)
                    {
                        if (capture.FallsWithin(pause.x, pause.y))
                        {
                            happenedInPause = true;
                        }
                        else if (pause.y > startTime && pause.y <= capture.Time)
                        {
                            cumulativePauseTime += (pause.y - pause.x);
                        }
                    }
                    if (happenedInPause == false)
                    {
                        filtered.Add(capture.SetTime(capture.Time - cumulativePauseTime) as T);
                    }
                }
            }
            return filtered.ToArray();
        }
    }

}
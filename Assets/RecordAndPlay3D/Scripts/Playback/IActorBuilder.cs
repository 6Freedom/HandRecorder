using System.Collections.Generic;

namespace EliCDavis.RecordAndPlay.Playback
{
    /// <summary>
    /// Implement this interface if you want your class to be used for setting up the actors in the scene for playback.
    /// </summary>
    public interface IActorBuilder 
    {

        /// <summary>
        /// Build an actor given the information presented.
        /// </summary>
        /// <param name="subjectId">The unique ID of the subject in the recording.</param>
        /// <param name="subjectName">The name of the subject.</param>
        /// <param name="metadata">Any extra information on the subject.</param>
        /// <returns>An actor to represent the original subject.</returns>
        Actor Build(int subjectId, string subjectName, Dictionary<string, string> metadata);
        
    }
}
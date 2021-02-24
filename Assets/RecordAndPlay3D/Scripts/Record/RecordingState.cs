
namespace EliCDavis.RecordAndPlay.Record
{
	/// <summary>
	/// The dfferent states a recorder can be in.
	/// </summary>
	public enum RecordingState {

		/// <summary>
		/// Currently recording events occuring in the scene.
		/// </summary>
		Recording,

		/// <summary>
		/// A recording has begun but the recorder is currently ignoring events.
		/// </summary>
		Paused,
		
		/// <summary>
		/// A recording has not begun.
		/// </summary>
		Stopped
	}

}
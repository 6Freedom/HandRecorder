//using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class MRTKRecording : MonoBehaviour
{
    private InputRecordingService recordingService;
    private InputPlaybackService playbackService;

    private bool recording = false;

    //private Task<string> lastAnim;
    
    void Start()
    {
        recordingService = CoreServices.GetInputSystemDataProvider<InputRecordingService>();
        playbackService = CoreServices.GetInputSystemDataProvider<InputPlaybackService>();
    }

    public void SwitchRecording()
    {
        if (!recording)
        {
            recording = !recording;
            recordingService.StartRecording();
        }
        else
        {
            recording = !recording;
            recordingService.StopRecording();
			//lastAnim = recordingService.SaveInputAnimationAsync("lastanimation", Application.persistentDataPath);
			recordingService.SaveInputAnimation("lastanimation", Application.persistentDataPath);
		}
	}

    public void Replay()
    {
	    //lastAnim.Wait();
		playbackService.LoadInputAnimation(Application.persistentDataPath + "/lastanimation");
        playbackService.Play();
    }
}

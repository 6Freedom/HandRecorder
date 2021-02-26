using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class MRTKRecording : MonoBehaviour
{
    // Start is called before the first frame update
    private InputRecordingService recordingService;
    private InputPlaybackService playbackService;

    private bool recording = false;

    private string lastAnimation;
    
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
            lastAnimation = recordingService.SaveInputAnimation("lastanimation", Application.persistentDataPath);
        }
    }

    public void Replay()
    {
        playbackService.LoadInputAnimation(Application.persistentDataPath + "/lastanimation");
        playbackService.Play();
    }
}

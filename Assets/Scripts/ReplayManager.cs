using System.Collections;
using System.Collections.Generic;
using EliCDavis.RecordAndPlay;
using EliCDavis.RecordAndPlay.Playback;
using EliCDavis.RecordAndPlay.Record;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class ReplayManager : MonoBehaviour, IActorBuilder
{
    [SerializeField] private GameObject playspace;
    [SerializeField] private AnimationClip registeredClip;

    [SerializeField] private GameObject leftReplayHandPrefab;
    [SerializeField] private GameObject rightReplayHandPrefab;

    [SerializeField] private GameObject middlePrefab;
    [SerializeField] private GameObject pinkyPrefab;
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private GameObject ringPrefab;
    [SerializeField] private GameObject thumbPrefab;

    private Recorder recorder;
    private Recording lastRecording;
    private PlaybackBehavior playbackBehavior;

    private bool recording = false;
    
    private void Start()
    {
		recorder = ScriptableObject.CreateInstance<Recorder>();
    }
    
    public GameObject[] GetHandsToRecord()
    {
        GameObject[] hands = new GameObject[2];

        if (HandJointUtils.FindHand(Handedness.Left) != null)
        {
            hands[0] = HandJointUtils.FindHand(Handedness.Left).Visualizer.GameObjectProxy;
        }
        
        if (HandJointUtils.FindHand(Handedness.Right) != null)
        {
            hands[1] = HandJointUtils.FindHand(Handedness.Right).Visualizer.GameObjectProxy;
        }
        
        return hands;
    }

    public IEnumerator DestroyReplayObjectAfter(float duration)
    {
		yield return new WaitForSeconds(duration);
		playbackBehavior.Stop();
    }

	public void StopRecording()
    {
        Debug.Log("Stopped recording");
        lastRecording = recorder.Finish();
    }

    public void StartRecording()
    {
        Debug.Log("Started Recording");
		
        // register each active hand
        foreach (var hand in GetHandsToRecord())
        {
            if(hand != null)
            {
				// Register each finger reference
	            var wrist = hand.transform.GetChild(0).GetChild(0).GetChild(0);
				SubjectBehavior.Build(wrist.GetChild(0).gameObject, recorder);
				SubjectBehavior.Build(wrist.GetChild(1).gameObject, recorder);
				SubjectBehavior.Build(wrist.GetChild(2).gameObject, recorder);
				SubjectBehavior.Build(wrist.GetChild(3).gameObject, recorder);
				SubjectBehavior.Build(wrist.GetChild(4).gameObject, recorder);
			}
        }
        
        recorder.Start();
	}

	// called when the record button is pressed
	public void SwitchRecording()
    {
        if (!recording)
        {
            recording = !recording;
            StartRecording();
        }
        else
        {
            recording = !recording;
            StopRecording();
        }
    }

	// called by the replay button
    public void Replay()
    {
        if (lastRecording != null)
        {
            playbackBehavior = PlaybackBehavior.Build(lastRecording, this, null, false);
			StartCoroutine(DestroyReplayObjectAfter(lastRecording.GetDuration()));
			playbackBehavior.Play();
        }
	}

	public Actor Build(int subjectId, string subjectName, Dictionary<string, string> metadata)
    {
		// create the correct finger depending of the subject
		GameObject previewArticulation = null;
		switch (subjectName)
		{
			case "MiddleL_JNT":
				previewArticulation = middlePrefab;
				break;
			case "PinkyL_JNT":
				previewArticulation = pinkyPrefab;
				break;
			case "PointL_JNT":
				previewArticulation = pointPrefab;
				break;
			case "RingL_JNT":
				previewArticulation = ringPrefab;
				break;
			case "ThumbL_JNT1":
				previewArticulation = thumbPrefab;
				break;
		}

        return new Actor(Instantiate(previewArticulation));
    }
}

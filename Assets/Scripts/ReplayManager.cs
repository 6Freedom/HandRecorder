using System.Collections;
using System.Collections.Generic;
using EliCDavis.RecordAndPlay;
using EliCDavis.RecordAndPlay.Playback;
using EliCDavis.RecordAndPlay.Record;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class ReplayManager : MonoBehaviour, IActorBuilder, IMixedRealitySourceStateHandler
{
    [SerializeField] private GameObject playspace;
    [SerializeField] private AnimationClip registeredClip;

    [SerializeField] private GameObject leftReplayHandPrefab;
    [SerializeField] private GameObject rightReplayHandPrefab;

    [SerializeField] private Recording debugRecording;

    private GameObject replayLeftHandInstance;
    private GameObject replayRightHandInstance;
 	
    private Recorder recorder;
    private Recording lastRecording;
    private PlaybackBehavior playbackBehavior;

    private bool recording = false;

    // Enable tracking hands for this object,  can't use OnSourceDetected without this
    // Deprecated version of this is adding component "InputSystemGlobalListener" to the object <- don't do this !
    private void OnEnable()
    {
		CoreServices.InputSystem?.RegisterHandler<IMixedRealitySourceStateHandler>(this);
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
        recorder = ScriptableObject.CreateInstance<Recorder>();
		foreach (var hand in GetHandsToRecord())
        {
            if(hand != null)
            {
                Utility.ForEachChild(hand.transform, child =>
                {	
	                SubjectBehavior.Build(child.gameObject, recorder);
                });
            }
        }
        
        recorder.Start();
	}

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

    public void Replay()
    {
		// do not play a playback if there is already one playing
        if (lastRecording != null && (playbackBehavior == null || !playbackBehavior.CurrentlyPlaying()))
        {
			replayLeftHandInstance = Instantiate(leftReplayHandPrefab);
			replayRightHandInstance = Instantiate(rightReplayHandPrefab);
			playbackBehavior = PlaybackBehavior.Build(lastRecording, this, null, false);
			StartCoroutine(DestroyReplayObjectAfter(lastRecording.GetDuration()));
			playbackBehavior.Play();
        }
	}

	[ContextMenu("debug replay")]
    public void DebugReplay()
    {
	    playbackBehavior.Play();
	}

	public Actor Build(int subjectId, string subjectName, Dictionary<string, string> metadata)
	{
		var tranformFound = replayLeftHandInstance.transform.RecursiveFind(subjectName);
		if (tranformFound == null)
			tranformFound = replayRightHandInstance.transform.RecursiveFind(subjectName);

		if (tranformFound == null)
		{
			Debug.Log($"{subjectName} is null");
		}

		return new Actor(tranformFound.gameObject);
    }

	public void OnSourceDetected(SourceStateEventData eventData)
	{
		Debug.Log("new source");
		if (eventData.Controller != null)
		{
			if (recording)
			{
				Debug.Log("Source added to record");
				SubjectBehavior.Build(eventData.Controller.Visualizer.GameObjectProxy, recorder);
			}
		}

	}

	public void OnSourceLost(SourceStateEventData eventData)
	{
	}

}

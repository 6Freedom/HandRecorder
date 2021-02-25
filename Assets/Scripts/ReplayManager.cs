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
 	
    private Recorder recorder;
    private Recording lastRecording;
    private PlaybackBehavior playbackBehavior;

    private bool recording = false;
    
    private void Start()
    {
        recorder = ScriptableObject.CreateInstance<Recorder>();
        //playbackBehavior = PlaybackBehavior.Build(lastRecording, this, null, true);
    }
    
    // Enable tracking hands for this object,  can't use OnSourceDetected without this
    // Deprecated version of this is adding component "InputSystemGlobalListener" to the object <- don't do this !
    private void OnEnable()
    {
		CoreServices.InputSystem?.RegisterHandler<IMixedRealitySourceStateHandler>(this);
	}

    private void Update()
    {
        //Debug.Log("Currently playing : " + playbackBehavior.CurrentlyPlaying());
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
        //lastRecording.SaveToAssets("Recording Example");
    }

    public void StartRecording()
    {
        Debug.Log("Started Recording");

        foreach (var hand in GetHandsToRecord())
        {
            if(hand != null)
            {
                Utility.ForEachChild(hand.transform, child =>
                {
                    SubjectBehavior.Build(child.gameObject, recorder);
                }, true);
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
        /*if (registeredClip != null)
        {
            instantiatedHands = Instantiate(replayHands);
            
            var destroyEvent = new AnimationEvent();

            destroyEvent.time = registeredClip.length;

            destroyEvent.functionName = "DestroyReplayHands";

            instantiatedHands.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].AddEvent(destroyEvent);
        }*/
        if (lastRecording != null)
        {
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
        /*if (subjectName == "Right_RiggedHandRight(Clone)")
        {
            var instantiatedReplayHand = Instantiate(leftReplayHandPrefab);
            return new Actor(instantiatedReplayHand);
        }
        else if (subjectName == "Left_RiggedHandLeft(Clone)")
        {
            var instantiatedReplayHand = Instantiate(rightReplayHandPrefab);
            return new Actor(instantiatedReplayHand);
        }*/

        GameObject previewArticulation = GameObject.CreatePrimitive(PrimitiveType.Cube);
		previewArticulation.transform.localScale = new Vector3(0.02f,0.02f,0.02f);
        return new Actor(previewArticulation);
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

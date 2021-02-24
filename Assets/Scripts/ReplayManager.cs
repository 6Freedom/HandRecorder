using System.Collections.Generic;
using EliCDavis.RecordAndPlay;
using EliCDavis.RecordAndPlay.Playback;
using EliCDavis.RecordAndPlay.Record;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class ReplayManager : MonoBehaviour, IActorBuilder
{
    [SerializeField] private GameObject playspace;
    [SerializeField] private AnimationClip registeredClip;

    [SerializeField] private GameObject leftReplayHand;
    [SerializeField] private GameObject rightReplayHand;

    [SerializeField] private Recording debugRecoring;
    
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
    //    CoreServices.InputSystem?.RegisterHandler<IMixedRealitySourceStateHandler>(this);
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

    public void StopRecording()
    {
        Debug.Log("Stopped recording");
        
        lastRecording = recorder.Finish();
        lastRecording.SaveToAssets("Recording Example");

    }

    public void StartRecording()
    {
        Debug.Log("Started Recording");

        foreach (var hand in GetHandsToRecord())
        {
            if(hand != null)
            {
                ForEachChild(hand.transform, child =>
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

    private GameObject instantiatedHands;

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
        
        
        playbackBehavior.Play();
        
    }

    [ContextMenu("debgu replay")]
    public void DebugReplay()
    {
        playbackBehavior = PlaybackBehavior.Build(debugRecoring, this, null, true);
    }

    public Actor Build(int subjectId, string subjectName, Dictionary<string, string> metadata)
    {
        /*if (subjectName == "Right_RiggedHandRight(Clone)")
        {
            var instantiatedReplayHand = Instantiate(leftReplayHand);
            return new Actor(instantiatedReplayHand);
        }
        else if (subjectName == "Left_RiggedHandLeft(Clone)")
        {
            var instantiatedReplayHand = Instantiate(rightReplayHand);
            return new Actor(instantiatedReplayHand);
        }*/

        GameObject previewArticulation = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        previewArticulation.transform.localScale = new Vector3(0.02f,0.02f,0.02f);

        return new Actor(previewArticulation);
    }

    public delegate void ForEachChildDel(Transform child);

    public static void ForEachChild(Transform t, ForEachChildDel del, bool recursive = true)
    {
        for (int i = 0; i < t.childCount; i++)
        {
            Transform child = t.GetChild(i);
            if (recursive)
                ForEachChild(child, del);
            del(child);
        }
    }


    /*public void OnSourceDetected(SourceStateEventData eventData)
    {
        Debug.Log("new source");
        if (eventData.Controller != null)
        {
            if (recording)
            {
                Debug.Log("Source added to record");
                recorder.BindComponentsOfType<Transform>(eventData.Controller.Visualizer.GameObjectProxy, true);
            }
        }
        
    }

    public void OnSourceLost(SourceStateEventData eventData)
    {
    }*/
}

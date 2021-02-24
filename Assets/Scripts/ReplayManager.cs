using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEditor.Animations;
using UnityEngine;

public class ReplayManager : MonoBehaviour //, IMixedRealitySourceStateHandler
{
    [SerializeField] private GameObject playspace;
    [SerializeField] private AnimationClip registeredClip;

    [SerializeField] private GameObject replayHands;

    [SerializeField] private AnimatorOverrideController overrideController;
    
    private GameObjectRecorder recorder;


    private bool recording = false;
    
    private void Start()
    {
        recorder = new GameObjectRecorder(playspace);
    }
    
    // Enable tracking hands for this object,  can't use OnSourceDetected without this
    // Deprecated version of this is adding component "InputSystemGlobalListener" to the object <- don't do this !
    private void OnEnable()
    {
    //    CoreServices.InputSystem?.RegisterHandler<IMixedRealitySourceStateHandler>(this);
    }

    private void LateUpdate()
    {
        if (registeredClip == null) return;
        if (recorder == null) return;
        
        Debug.Log("recording");
        
        recorder.TakeSnapshot(Time.deltaTime);
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
        registeredClip.ClearCurves();
        recorder.SaveToClip(registeredClip);
        recorder.ResetRecording();
    }

    public void StartRecording()
    {
        Debug.Log("Started Recording");

        foreach (var hand in GetHandsToRecord())
        {
            if(hand != null)
            {
                recorder.BindComponentsOfType<Transform>(hand, true);
            }
        }
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
        if (registeredClip != null)
        {
            instantiatedHands = Instantiate(replayHands);
            
            var destroyEvent = new AnimationEvent();

            destroyEvent.time = registeredClip.length;

            destroyEvent.functionName = "DestroyReplayHands";

            instantiatedHands.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].AddEvent(destroyEvent);
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

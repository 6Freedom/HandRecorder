using UnityEditor.Animations;
using UnityEngine;

public class CubeRecorder : MonoBehaviour
{
    // Game Object Recorder is attached to the Recorder Object

    [SerializeField] private GameObject cube1;
    [SerializeField] private GameObject cube2;
    [SerializeField] private GameObject cubeRoot;

    [SerializeField] private AnimationClip clip; 
    
    private GameObjectRecorder recorder;
    
    void Start()
    {
        recorder = new GameObjectRecorder(cubeRoot);
        recorder.BindComponentsOfType<Transform>(cube1, true);
        recorder.BindComponentsOfType<Transform>(cube2, true);
    }

    // Update is called once per frame
    void Update()
    {
        recorder.TakeSnapshot(Time.deltaTime);
    }

    private void OnDisable()
    {
        recorder.SaveToClip(clip);
    }
}

using UnityEditor.Animations;
using UnityEngine;

public class Recorded : MonoBehaviour
{
    public AnimationClip clip;

    private GameObjectRecorder recorder;
    
    void Start()
    {
        recorder = new GameObjectRecorder(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void OnDestroy()
    {
        OnDisable();
    }
}

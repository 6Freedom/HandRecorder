using System;

namespace EliCDavis.RecordAndPlay
{
    /// <summary>
    /// Events that coorelates to the <a href="https://docs.unity3d.com/Manual/ExecutionOrder.html">monobehavior lifecycle</a>.
    /// </summary>
    public enum UnityLifeCycleEvent
    {
        /// <summary>
        /// Represents the <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html">Start</a> monobehavior event.
        /// </summary>
        Start = 0,

        /// <summary>
        /// Represents the <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html">OnEnable</a> monobehavior event.
        /// </summary>
        Enable = 1,

        /// <summary>
        /// Represents the <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDisable.html">OnDisable</a> monobehavior event.
        /// </summary>
        Disable = 2,

        /// <summary>
        /// Represents the <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDestroy.html">OnDestroy</a> monobehavior event.
        /// </summary>
        Destroy = 3
    }

    static class UnityLifeCycleEventMethods
    {

        public static String ToString(this UnityLifeCycleEvent s1)
        {
            switch (s1)
            {
                case UnityLifeCycleEvent.Start:
                    return "Start";
                
                case UnityLifeCycleEvent.Enable:
                    return "Enable";

                case UnityLifeCycleEvent.Disable:
                    return "Disable";
                
                case UnityLifeCycleEvent.Destroy:
                    return "Destroy";
            }
            return "";
        }

    }
}
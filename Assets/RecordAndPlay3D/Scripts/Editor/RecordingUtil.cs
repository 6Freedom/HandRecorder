using UnityEngine;
using UnityEditor;
using System.IO;

namespace EliCDavis.RecordAndPlay.Editor
{

    public static class RecordingUtil
    {

        /// <summary>
        /// Saves the recording as an asset to the asset folder of the project.  If no name is provided and the recording's set name is blank, the file will be named "Unamed".
        /// </summary>
        /// <param name="recording">The recording we want to save to the assets folder.</param>
        /// <param name="name">Name of the asset file.</param>
        /// <param name="path">Where in the project for the asset to be saved.</param>
        /// <remarks>Will append a number to the end of the name if another asset already uses the name passed in.</remarks>
        static public void SaveToAssets(Recording recording, string name, string path)
        {
            if (path == "")
            {
                path = "Assets";
            }

            var nameToUse = name;
            if (string.IsNullOrEmpty(nameToUse))
            {
                nameToUse = recording.RecordingName;
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, string.Format("{0}.asset", string.IsNullOrEmpty(nameToUse) ? "Unamed" : nameToUse)));

            AssetDatabase.CreateAsset(recording, assetPathAndName);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Saves the recording as an asset to the root of the asset folder of the project. If no name is provided and the set name for the recording is blank, the file will be named "Unamed".
        /// </summary>
        /// <param name="recording">The recording we want to save to the assets folder.</param>
        /// <param name="name">Name of the asset file.</param>
        /// <remarks>Will append a number to the end of the name if another asset already uses the name passed in.</remarks>
        static public void SaveToAssets(Recording recording, string name)
        {
            SaveToAssets(recording, name, "");
        }

        /// <summary>
        /// Saves the recording as an asset to the root of the asset folder of the project. If no name is provided and the set name for the recording is blank, the file will be named "Unamed".
        /// </summary>
        /// <param name="recording">The recording we want to save to the assets folder.</param>
        /// <remarks>Will append a number to the end of the name if another asset already uses the name passed in.</remarks>
        static public void SaveToAssets(Recording recording)
        {
            SaveToAssets(recording, recording.RecordingName, "");
        }

        /// <summary>
        /// Builds an animation clip per subject in the recording.
        /// 
        /// <b>NOTE:</b> This is an editor-only functionality due to the nature of <a href="https://docs.unity3d.com/ScriptReference/AnimationClip.SetCurve.html">AnimationClips.SetCurve</a>.
        /// </summary>
        /// <param name="recording">The recording we want to build animation clips from.</param>
        /// <param name="smooth">Whether or not there will be a smooth transition from one keyframe to the next. Will cause the actor to be in locations and rotations that where never recorded and will look akward in long standing positions and roations.</param>
        /// <returns>Animation Clips that represent actor movement</returns>
        public static AnimationClip[] GetAnimationClips(Recording recording, bool smooth)
        {
            AnimationClip[] clips = new AnimationClip[recording.SubjectRecordings.Length];
            for (int subjectIndex = 0; subjectIndex < recording.SubjectRecordings.Length; subjectIndex++)
            {
                var subj = recording.SubjectRecordings[subjectIndex];
                clips[subjectIndex] = new AnimationClip
                {
                    name = subj.SubjectName
                };

                var duration = subj.GetDuration();

                var startingPos = subj.GetStartingPosition();
                var endingPos = subj.GetEndingPosition();

                AnimationCurve translateX = AnimationCurve.Linear(0, startingPos.x, duration, endingPos.x);
                AnimationCurve translateY = AnimationCurve.Linear(0, startingPos.y, duration, endingPos.y);
                AnimationCurve translateZ = AnimationCurve.Linear(0, startingPos.z, duration, endingPos.z);

                foreach (var positionCapture in recording.SubjectRecordings[subjectIndex].CapturedPositions)
                {
                    var adjustedEndtime = positionCapture.Time - subj.GetStartTime();
                    translateX.AddKey(adjustedEndtime, positionCapture.Vector.x);
                    translateY.AddKey(adjustedEndtime, positionCapture.Vector.y);
                    translateZ.AddKey(adjustedEndtime, positionCapture.Vector.z);
                }

                if(smooth == false)
                {
                    for (int i = 0; i < recording.SubjectRecordings[subjectIndex].CapturedPositions.Length; i++)
                    {
                        AnimationUtility.SetKeyLeftTangentMode(translateX, i, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyLeftTangentMode(translateY, i, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyLeftTangentMode(translateZ, i, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyRightTangentMode(translateX, i, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyRightTangentMode(translateY, i, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyRightTangentMode(translateZ, i, AnimationUtility.TangentMode.Linear);
                    }
                }

                clips[subjectIndex].SetCurve("", typeof(Transform), "m_LocalPosition.x", translateX);
                clips[subjectIndex].SetCurve("", typeof(Transform), "m_LocalPosition.y", translateY);
                clips[subjectIndex].SetCurve("", typeof(Transform), "m_LocalPosition.z", translateZ);

                var startingRot = subj.GetStartingRotation();
                var endingRot = subj.GetEndingRotation();

                AnimationCurve rotateX = AnimationCurve.Linear(0, startingRot.x, duration, endingRot.x);
                AnimationCurve rotateY = AnimationCurve.Linear(0, startingRot.y, duration, endingRot.y);
                AnimationCurve rotateZ = AnimationCurve.Linear(0, startingRot.z, duration, endingRot.z);
                AnimationCurve rotateW = AnimationCurve.Linear(0, startingRot.w, duration, endingRot.w);

                foreach (var rotationCapture in recording.SubjectRecordings[subjectIndex].CapturedRotations)
                {
                    var adjustedEndtime = rotationCapture.Time - subj.GetStartTime();
                    var q = Quaternion.Euler(rotationCapture.Vector);
                    rotateX.AddKey(adjustedEndtime, q.x);
                    rotateY.AddKey(adjustedEndtime, q.y);
                    rotateZ.AddKey(adjustedEndtime, q.z);
                    rotateW.AddKey(adjustedEndtime, q.w);
                }

                if (smooth == false)
                {
                    for (int i = 0; i < recording.SubjectRecordings[subjectIndex].CapturedRotations.Length; i++)
                    {
                        AnimationUtility.SetKeyLeftTangentMode(rotateX, i, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyLeftTangentMode(rotateY, i, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyLeftTangentMode(rotateZ, i, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyLeftTangentMode(rotateW, i, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyRightTangentMode(rotateX, i, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyRightTangentMode(rotateY, i, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyRightTangentMode(rotateZ, i, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyRightTangentMode(rotateW, i, AnimationUtility.TangentMode.Linear);
                    }
                }

                AnimationEvent[] animationEvents = new AnimationEvent[subj.CapturedCustomEvents.Length];
                for (int i = 0; i < animationEvents.Length; i++)
                {
                    animationEvents[i] = new AnimationEvent
                    {
                        time = subj.CapturedCustomEvents[i].Time - subj.GetStartTime(),
                        functionName = subj.CapturedCustomEvents[i].Name,
                        stringParameter = subj.CapturedCustomEvents[i].Contents,
                    };
                }
                
                AnimationUtility.SetAnimationEvents(clips[subjectIndex], animationEvents);

                clips[subjectIndex].SetCurve("", typeof(Transform), "m_LocalRotation.x", rotateX);
                clips[subjectIndex].SetCurve("", typeof(Transform), "m_LocalRotation.y", rotateY);
                clips[subjectIndex].SetCurve("", typeof(Transform), "m_LocalRotation.z", rotateZ);
                clips[subjectIndex].SetCurve("", typeof(Transform), "m_LocalRotation.w", rotateW);
            }

            return clips;
        }

    }

}
using UnityEngine;
using UnityEditor;

///#IGNORE
namespace EliCDavis.RecordAndPlay.Editor.Extension.RapFile
{

    [InitializeOnLoad]
    public class RapFileGlobal
    {
        private static RapFileWrapper wrapper = null;
        private static bool selectionChanged = false;

        static RapFileGlobal()
        {
            Selection.selectionChanged += SelectionChanged;
            EditorApplication.update += Update;
        }

        private static void SelectionChanged()
        {
            selectionChanged = true;
            // can't do the wrapper stuff here. it does not work 
            // when you Selection.activeObject = wrapper
            // so do it in Update
        }

        private static void Update()
        {
            if (selectionChanged == false)
            {
                return;
            }

            selectionChanged = false;
            if (Selection.activeObject != wrapper)
            {
                string fn = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
                if (fn.ToLower().EndsWith(".rap"))
                {
                    if (wrapper == null)
                    {
                        wrapper = ScriptableObject.CreateInstance<RapFileWrapper>();
                        wrapper.hideFlags = HideFlags.DontSave;
                    }

                    wrapper.fileName = fn;
                    Selection.activeObject = wrapper;

                    UnityEditor.Editor[] ed = Resources.FindObjectsOfTypeAll<RapFileWrapperInspector>();
                    if (ed.Length > 0)
                    {
                        ed[0].Repaint();
                    }
                }
            }
        }
    }

}
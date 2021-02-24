using UnityEngine;
using EliCDavis.RecordAndPlay.IO;
using System.IO;

namespace EliCDavis.RecordAndPlay.Editor.Extension.RapFile
{
    public class RapFileWrapper : ScriptableObject
    {
        [System.NonSerialized] public string fileName; // path is relative to Assets/

        private int numberOfRecordings = -1;

        private int LoadNumberOfRecordings()
        {
            if (fileName == "" || fileName == null)
            {
                return -1;
            }
            using (FileStream fs = File.OpenRead(fileName))
            {
                return Unpackager.Peak(fs);
            }
        }

        void Awake()
        {
            numberOfRecordings = LoadNumberOfRecordings();
        }

        public int NumberOfRecordings()
        {
            if (numberOfRecordings == -1)
            {
                numberOfRecordings = LoadNumberOfRecordings();
            }
            return numberOfRecordings;
        }

    }

}
using EliCDavis.RecordAndPlay.Record;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System;
using UnityEngine;

namespace EliCDavis.RecordAndPlay.IO
{

    /// <summary>
    /// Responsible for converting binary data to recordings for playback.
    /// </summary>
    public static class Unpackager
    {

        /// <summary>
        /// Look at how many recording files are in the stream without reading the entire stream. Should only progress the stream by 5 bytes. That might not always be the case in future versions though. You'll need to reset the stream if you expect to feed the same stream into <b>Unpackage</b>.
        /// </summary>
        /// <param name="stream">The stream containing the recoring data.</param>
        /// <returns>The number of recordings in the stream.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the stream is null or empty.</exception>
        /// <exception cref="System.SystemException">Thrown when the stream contains data in an unexpected format.</exception>
        public static int Peak(Stream stream)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new ArgumentException("Nothing to unpackage (stream is null or empty)");
            }

            int fileVersion = stream.ReadByte();

            if (fileVersion == -1)
            {
                throw new SystemException("End of stream prematurely! Not enough data to build recordings from");
            }

            if (fileVersion != 1)
            {
                throw new SystemException(string.Format("Unsupported File Version: {0}", fileVersion));
            }

            byte[] numberRecordingsByteData = new byte[4];
            stream.Read(numberRecordingsByteData, 0, 4);
            return BitConverter.ToInt32(numberRecordingsByteData, 0);
        }

        /// <summary>
        /// Reads from a stream of data and builds an array of recordings that can be used for playback.
        /// </summary>
        /// <param name="stream">The stream to be read from for building Recording objects.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Thrown when the stream is null or empty.</exception>
        /// <exception cref="System.SystemException">Thrown when the stream contains data in an unexpected format.</exception>
        public static Recording[] Unpackage(Stream stream)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new ArgumentException("Nothing to unpackage (stream is null or empty)");
            }

            int fileVersion = stream.ReadByte();

            if (fileVersion == -1)
            {
                throw new SystemException("End of stream prematurely! Not enough data to build recordings from");
            }

            if (fileVersion != 1)
            {
                throw new SystemException(string.Format("Unsupported File Version: {0}", fileVersion));
            }

            byte[] numberRecordingsByteData = new byte[4];
            stream.Read(numberRecordingsByteData, 0, 4);
            int numberRecordings = BitConverter.ToInt32(numberRecordingsByteData, 0);

            using (BinaryReader reader = new BinaryReader(stream))
            {
                Transport.Recording[] recordings = new Transport.Recording[numberRecordings];
                for (int i = 0; i < numberRecordings; i++)
                {
                    var transportDataCompressed = new MemoryStream(reader.ReadBytes((int)reader.ReadInt64()));
                    using (DeflateStream dstream = new DeflateStream(transportDataCompressed, CompressionMode.Decompress))
                    {
                        recordings[i] = Transport.Recording.Parser.ParseFrom(dstream);
                    }

                }

                return FromTransport(recordings);
            }
        }

        /// <summary>
        /// Converts protobuf recordings into a version that can be used for playback.
        /// </summary>
        /// <param name="transportRecordings">Protobuf recordings to be converted.</param>
        /// <returns>Recordings that can be used for things like playback.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the recordings presented is null or empty.</exception>
        public static Recording[] FromTransport(params Transport.Recording[] transportRecordings)
        {
            if (transportRecordings == null)
            {
                throw new ArgumentException("Nothing to convert (null recordings)");
            }

            Recording[] recordings = new Recording[transportRecordings.Length];

            for (int recordIndex = 0; recordIndex < transportRecordings.Length; recordIndex++)
            {
                if (transportRecordings[recordIndex] == null)
                {
                    throw new ArgumentException(string.Format("Null transport Recording at index {0}.", recordIndex));
                }

                recordings[recordIndex] = Recording.CreateInstance(
                    FromTransport(transportRecordings[recordIndex].Subjects),
                    FromTransport(transportRecordings[recordIndex].CustomEvents),
                    new Dictionary<string, string>(transportRecordings[recordIndex].Metadata)
                );
                recordings[recordIndex].RecordingName = transportRecordings[recordIndex].Name;
            }

            return recordings;
        }

        private static SubjectRecording[] FromTransport(Google.Protobuf.Collections.RepeatedField<Transport.SubjectRecording> transportRecordings)
        {
            if (transportRecordings == null)
            {
                throw new Exception("Nothing to convert (null recordings)");
            }

            SubjectRecording[] recordings = new SubjectRecording[transportRecordings.Count];

            for (int recordIndex = 0; recordIndex < transportRecordings.Count; recordIndex++)
            {
                recordings[recordIndex] = new SubjectRecording(
                    transportRecordings[recordIndex].Id,
                    transportRecordings[recordIndex].Name,
                    new Dictionary<string, string>(transportRecordings[recordIndex].Metadata),
                    FromTransport(transportRecordings[recordIndex].CapturedPositions),
                    FromTransport(transportRecordings[recordIndex].CapturedRotations),
                    FromTransport(transportRecordings[recordIndex].LifecycleEvents),
                    FromTransport(transportRecordings[recordIndex].CustomEvents)
                );
            }

            return recordings;
        }

        private static VectorCapture[] FromTransport(Google.Protobuf.Collections.RepeatedField<Transport.VectorCapture> transportCapture)
        {
            var vectorCapture = new VectorCapture[transportCapture.Count];
            for (int i = 0; i < transportCapture.Count; i++)
            {
                vectorCapture[i] = new VectorCapture(transportCapture[i].Time, new Vector3(transportCapture[i].X, transportCapture[i].Y, transportCapture[i].Z));
            }
            return vectorCapture;
        }

        private static CustomEventCapture[] FromTransport(Google.Protobuf.Collections.RepeatedField<Transport.CustomEventCapture> transportCapture)
        {
            var curstomCapture = new CustomEventCapture[transportCapture.Count];
            for (int i = 0; i < transportCapture.Count; i++)
            {
                curstomCapture[i] = new CustomEventCapture(transportCapture[i].Time, transportCapture[i].Name, transportCapture[i].Contents);
            }
            return curstomCapture;
        }

        private static UnityLifeCycleEventCapture[] FromTransport(Google.Protobuf.Collections.RepeatedField<Transport.LifeCycleEventCapture> transportCapture)
        {
            var curstomCapture = new UnityLifeCycleEventCapture[transportCapture.Count];
            for (int i = 0; i < transportCapture.Count; i++)
            {
                curstomCapture[i] = new UnityLifeCycleEventCapture(transportCapture[i].Time, (UnityLifeCycleEvent)((int)transportCapture[i].Type));
            }
            return curstomCapture;
        }

    }
}
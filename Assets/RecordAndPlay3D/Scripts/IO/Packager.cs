using System.IO;
using System.IO.Compression;
using System;
using Google.Protobuf;

/// <summary>
/// Used for both compressing and decompressing recordings to and from binary. The custom format currently utilizes both protobuf and <a href="https://tools.ietf.org/html/rfc1951">deflate</a> for maintainability and small file sizes.
/// </summary>
namespace EliCDavis.RecordAndPlay.IO
{
    /// <summary>
    /// Responsible for converting recordings into a binary format and compressing them. If you desire further compression be sure to do a proper comparison of file sizes to ensure you're not actually increasing file size in the process. If your domain allows you to utilize the Brotli compression algorithm, I'd reccomend that for even further compression.
    /// </summary>
    public static class Packager
    {
        /// <summary>
        /// Converts recordings into their protobuf counterpart and compresses them using the <a href="https://tools.ietf.org/html/rfc1951">deflate</a> algorithm. The results are written to the stream passed in.
        /// </summary>
        /// <exception cref="System.ArgumentException">Thrown when the recordings presented is null or empty.</exception>
        /// <param name="outStream">The stream to write the results to.</param>
        /// <param name="recordings">The recordings to convert to a compressed binary format.</param>
        public static void Package(Stream outStream, params Recording[] recordings)
        {
            if (outStream == null)
            {
                throw new ArgumentException("Can't write to a null stream!");
            }

            if (recordings == null || recordings.Length == 0)
            {
                throw new ArgumentException("No recordings (null or empty) to write to stream!");
            }

            using (BinaryWriter writer = new BinaryWriter(outStream))
            {
                writer.Write((byte)1);
                writer.Write(recordings.Length);

                var transportRecordings = ToTransport(recordings);
                foreach (var transport in transportRecordings)
                {
                    using (MemoryStream compressionOutput = new MemoryStream())
                    {
                        using (DeflateStream dstream = new DeflateStream(compressionOutput, CompressionLevel.Optimal, true))
                        {
                            transport.WriteTo(dstream);
                        }
                        writer.Write(compressionOutput.Length);
                        compressionOutput.WriteTo(outStream);
                    }
                }
            }

        }

        /// <summary>
        /// Convert recording objects into their protobuf counterpart.
        /// </summary>
        /// <param name="recordings">Recordings to convert.</param>
        /// <returns>The recordings in the protobuf format.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the recordings presented is null or empty.</exception>
        public static Transport.Recording[] ToTransport(params Recording[] recordings)
        {
            if (recordings == null)
            {
                throw new ArgumentException("Nothing to convert (null recordings)");
            }

            if (recordings.Length == 0)
            {
                throw new ArgumentException("Nothing to convert (empty recordings array)");
            }

            Transport.Recording[] transports = new Transport.Recording[recordings.Length];
            for (int recordingIndex = 0; recordingIndex < recordings.Length; recordingIndex++)
            {
                if (recordings[recordingIndex] == null)
                {
                    throw new ArgumentException(string.Format("Null Recording at index {0}", recordingIndex));
                }

                transports[recordingIndex] = new Transport.Recording
                {
                    Name = recordings[recordingIndex].RecordingName
                };

                transports[recordingIndex].CustomEvents.AddRange(ToTransport(recordings[recordingIndex].CapturedCustomEvents));
                transports[recordingIndex].Metadata.Add(recordings[recordingIndex].Metadata);
                transports[recordingIndex].Subjects.AddRange(ToTransport(recordings[recordingIndex].SubjectRecordings));
            }
            return transports;
        }

        private static Transport.SubjectRecording[] ToTransport(params SubjectRecording[] recordings)
        {
            if (recordings == null)
            {
                throw new Exception("Nothing to convert (null recordings)");
            }

            Transport.SubjectRecording[] transports = new Transport.SubjectRecording[recordings.Length];
            for (int recordingIndex = 0; recordingIndex < recordings.Length; recordingIndex++)
            {
                transports[recordingIndex] = new Transport.SubjectRecording
                {
                    Name = recordings[recordingIndex].SubjectName,
                    Id = recordings[recordingIndex].SubjectID,

                };

                transports[recordingIndex].CustomEvents.AddRange(ToTransport(recordings[recordingIndex].CapturedCustomEvents));
                transports[recordingIndex].Metadata.Add(recordings[recordingIndex].Metadata);
                transports[recordingIndex].CapturedPositions.AddRange(ToTransport(recordings[recordingIndex].CapturedPositions));
                transports[recordingIndex].CapturedRotations.AddRange(ToTransport(recordings[recordingIndex].CapturedRotations));

                // Lifecycle
                for (int vIndex = 0; vIndex < recordings[recordingIndex].CapturedLifeCycleEvents.Length; vIndex++)
                {
                    transports[recordingIndex].LifecycleEvents.Add(new Transport.LifeCycleEventCapture()
                    {
                        Time = recordings[recordingIndex].CapturedLifeCycleEvents[vIndex].Time,
                        Type = (Transport.LifeCycleEventCapture.Types.LifeType)((int)recordings[recordingIndex].CapturedLifeCycleEvents[vIndex].LifeCycleEvent)
                    });
                }
            }
            return transports;
        }

        private static Transport.VectorCapture[] ToTransport(params VectorCapture[] customEvents)
        {
            var transportEvents = new Transport.VectorCapture[customEvents.Length];
            for (int eventIndex = 0; eventIndex < customEvents.Length; eventIndex++)
            {
                transportEvents[eventIndex] = new Transport.VectorCapture()
                {
                    Time = customEvents[eventIndex].Time,
                    X = customEvents[eventIndex].Vector.x,
                    Y = customEvents[eventIndex].Vector.y,
                    Z = customEvents[eventIndex].Vector.z,
                };
            }
            return transportEvents;
        }

        private static Transport.CustomEventCapture[] ToTransport(params CustomEventCapture[] customEvents)
        {
            var transportEvents = new Transport.CustomEventCapture[customEvents.Length];
            for (int eventIndex = 0; eventIndex < customEvents.Length; eventIndex++)
            {
                transportEvents[eventIndex] = new Transport.CustomEventCapture()
                {
                    Time = customEvents[eventIndex].Time,
                    Name = customEvents[eventIndex].Name,
                    Contents = customEvents[eventIndex].Contents,
                };
            }
            return transportEvents;
        }

    }

}
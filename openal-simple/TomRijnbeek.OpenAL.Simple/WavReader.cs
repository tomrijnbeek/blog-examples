using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Audio.OpenAL;

namespace TomRijnbeek.OpenAL.Simple
{
    static class WavReader
    {
        public static (List<short[]> buffers, ALFormat alFormat, int sampleRate) ReadWav(Stream file) {
            using (var reader = new BinaryReader(file)) {
                // RIFF header
                var signature = new string(reader.ReadChars(4));
                if (signature != "RIFF") {
                    throw new NotSupportedException("Specified stream is not a wave file.");
                }

                reader.ReadInt32(); // riffChunkSize

                var format = new string(reader.ReadChars(4));
                if (format != "WAVE") {
                    throw new NotSupportedException("Specified stream is not a wave file.");
                }

                // WAVE header
                var formatSignature = new string(reader.ReadChars(4));
                if (formatSignature != "fmt ") {
                    throw new NotSupportedException("Specified wave file is not supported.");
                }

                int formatChunkSize = reader.ReadInt32();
                reader.ReadInt16(); // audioFormat
                int numChannels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32();
                reader.ReadInt32(); // byteRate
                reader.ReadInt16(); // blockAlign
                int bitsPerSample = reader.ReadInt16();

                if (formatChunkSize > 16)
                    reader.ReadBytes(formatChunkSize - 16);

                var dataSignature = new string(reader.ReadChars(4));

                if (dataSignature != "data") {
                    throw new NotSupportedException("Only uncompressed wave files are supported.");
                }

                reader.ReadInt32(); // dataChunkSize

                var alFormat = getSoundFormat(numChannels, bitsPerSample);

                var data = reader.ReadBytes((int)reader.BaseStream.Length);
                var buffers = new List<short[]>();
                int count;
                var i = 0;
                const int bufferSize = 16384;

                while ((count = (Math.Min(data.Length, (i + 1) * bufferSize * 2) - i * bufferSize * 2) / 2) > 0) {
                    var buffer = new short[bufferSize];
                    convertBuffer(data, buffer, count, i * bufferSize * 2);
                    buffers.Add(buffer);
                    i++;
                }

                return (buffers, alFormat, sampleRate);
            }
        }
        
        private static void convertBuffer(byte[] inBuffer, short[] outBuffer, int length, int inOffset = 0) {
            for (var i = 0; i < length; i++) {
                outBuffer[i] = BitConverter.ToInt16(inBuffer, inOffset + 2 * i);
            }
        }

        private static ALFormat getSoundFormat(int channels, int bits) {
            switch (channels) {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }
    }
}

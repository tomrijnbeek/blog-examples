using System;
using System.IO;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace TomRijnbeek.OpenAL.Simple {
    static class Program {
        
        static void Main(string[] args) {
            
            using (var _ = new AudioContext()) {
                var (bytes, alFormat, sampleRate) = WavReader.ReadWav(File.OpenRead("assets/pew.wav"));

                var buffers = AL.GenBuffers(bytes.Count);
                for (var i = 0; i < buffers.Length; i++) {
                    AL.BufferData(buffers[i], alFormat, bytes[i], bytes[i].Length, sampleRate);
                }

                var source = AL.GenSource();
                AL.SourceQueueBuffers(source, buffers.Length, buffers);

                Console.WriteLine("Press button to play sound");
                Console.ReadKey();
                
                AL.SourcePlay(source);
                
                Console.WriteLine("Sound playing. Press button to exit");
                Console.ReadKey();
            }
        }
    }
}

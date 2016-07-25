using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace DCBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Initializer config = new Initializer();
            new Program().Start();
        }

        private DiscordClient _client;
        private IAudioClient _vClient;

        public void Start()
        {
            _client = new DiscordClient();

            _client.UsingAudio(x => // Opens an AudioConfigBuilder so we can configure our AudioService
            {
                x.Mode = AudioMode.Outgoing; // Tells the AudioService that we will only be sending audio
            });

            var _vService = _client.GetService<AudioService>();

            _client.MessageReceived += async (s, e) =>
            {
                var voiceChannel = _client.Servers.FirstOrDefault().VoiceChannels.FirstOrDefault();
                if (!e.Message.IsAuthor)
                {
                    Console.WriteLine("Message recieved from " + e.User + ": " + e.Message.Text);
                    await e.Channel.SendMessage(e.Message.Text);

                    _vClient = await _vService.Join(voiceChannel);
                    //SendAudio("cena.mp3");
                    send("cena.wav");
                    await _vClient.Disconnect();
                }
            };

            _client.ExecuteAndWait(async () =>
            {
                await _client.Connect("MjA2NTc5ODMxNDg2NDE0ODU5.CnWo_w.oJP53Ua0qBGBPMsLofCvfFaheXw");
            });

        }

        private void send(string path)
        {
            int blockSize = 3840; // The size of bytes to read per frame; 1920 for mono

            using (FileStream f = File.Open(path, FileMode.Open))
            {
                byte[] buffer = new byte[blockSize];
                while (f.Read(buffer, 0, buffer.Length) > 0)
                {
                    _vClient.Send(buffer, 0, buffer.Length);
                }
                _vClient.Wait();
            }
        }

        public void SendAudio(string pathOrUrl)
        {
            var process = Process.Start(new ProcessStartInfo
            { // FFmpeg requires us to spawn a process and hook into its stdout, so we will create a Process
                FileName = "ffmpeg",
                Arguments = $"-i {pathOrUrl} " + // Here we provide a list of arguments to feed into FFmpeg. -i means the location of the file/URL it will read from
                            "-f s16le -ar 48000 -ac 2 pipe:1", // Next, we tell it to output 16-bit 48000Hz PCM, over 2 channels, to stdout.
                UseShellExecute = false,
                RedirectStandardOutput = true // Capture the stdout of the process
            });
            Thread.Sleep(1000); // Sleep for a few seconds to FFmpeg can start processing data.

            int blockSize = 3840; // The size of bytes to read per frame; 1920 for mono
            byte[] buffer = new byte[blockSize];
            int byteCount;

            while (true) // Loop forever, so data will always be read
            {
                byteCount = process.StandardOutput.BaseStream // Access the underlying MemoryStream from the stdout of FFmpeg
                        .Read(buffer, 0, blockSize); // Read stdout into the buffer

                if (byteCount == 0) // FFmpeg did not output anything
                    break; // Break out of the while(true) loop, since there was nothing to read.

                _vClient.Send(buffer, 0, byteCount); // Send our data to Discord
            }
            _vClient.Wait(); // Wait for the Voice Client to finish sending data, as ffMPEG may have already finished buffering out a song, and it is unsafe to return now.
        }
    }
}

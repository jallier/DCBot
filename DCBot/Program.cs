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
using Discord.Commands;

namespace DCBot
{
    class Program
    {
        Initializer config;
        static void Main(string[] args)
        {
            new Program().Start();
        }

        private DiscordClient _client;
        private IAudioClient _vClient;
        private Queue<Command> audioQueue = new Queue<Command>();
        private bool audioPlaying = false; //This seems like a dirty hack.

        public void Start()
        {
            config = new Initializer();
            _client = new DiscordClient();

            _client.UsingAudio(x => // Opens an AudioConfigBuilder so we can configure our AudioService
            {
                x.Mode = AudioMode.Outgoing; // Tells the AudioService that we will only be sending audio
            });

            //var _vService = _client.GetService<AudioService>();

            _client.UsingCommands(x =>
            {
                x.PrefixChar = config.CommandChar;
                x.HelpMode = HelpMode.Public;
            });

            createCommands();

            _client.MessageReceived += async (s, e) =>
            {
                if (!e.Message.IsAuthor)
                {
                    Console.WriteLine("Message recieved from " + e.User + ": " + e.Message.Text);
                    await e.Channel.SendMessage(e.Message.Text);
                }
            };

            _client.ExecuteAndWait(async () =>
            {
                await _client.Connect("MjA2NTc5ODMxNDg2NDE0ODU5.CnWo_w.oJP53Ua0qBGBPMsLofCvfFaheXw");
            });

        }

        private void createCommands()
        {
            foreach (Audio command in config.commands)
            {
                CommandBuilder cb = _client.GetService<CommandService>().CreateCommand(command.Command);
                if (command.Alias != null) { cb.Alias(command.Alias); }
                if (command.Description != null) { cb.Description(command.Description); }
                cb.Do( e =>
                {
                    addAudioToQueue(command.Path, e.User.VoiceChannel);
                    if (!audioPlaying) { sendAudioQueue();}
                    
                });
            }
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

        private void addAudioToQueue(string path, Channel voiceChannel)
        {
            audioQueue.Enqueue(new Command(path, voiceChannel));
        }

        private async void sendAudioQueue()
        {
            while(audioQueue.Count > 0)
            {
                audioPlaying = true;
                Command current = audioQueue.Dequeue();
                try { _vClient = await current.VoiceChannel.JoinAudio(); } catch { }
                send(current.Path);
            }
            await _vClient.Disconnect();
            audioPlaying = false;
        }
    }

    class Command
    {
        public string Path { get; }
        public Channel VoiceChannel { get; }
        public Command(string path, Channel voiceChannel)
        {
            Path = path;
            VoiceChannel = voiceChannel;
        }
    }
}

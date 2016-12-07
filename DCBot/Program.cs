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
using System.Reflection;
using log4net;

namespace DCBot
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Start();
        }

        private bool audioPlaying = false; //This seems like a dirty hack.
        private bool IsRunningOnMono { get; } = (Type.GetType("Mono.Runtime") != null);
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType); // Initialize logger with class name of calling class

        DiscordClient _client = new DiscordClient(x => { x.LogLevel = LogSeverity.Info; });
        /// <summary>
        /// Method to start the bot.
        /// </summary>
        public void Start()
        {
            log.Info("Application is starting");
            Initializer config = new Initializer().run();
            Queue<Command> audioQueue = new Queue<Command>();

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

            createCommands(config, _client, audioQueue);

            createFileSystemWatcher(config);

            _client.ExecuteAndWait(async () =>
            {
                log.Info("Connecting to Discord...");
                int attempts = 0;
                while (attempts < 3)
                {
                    try
                    {
                        await _client.Connect(config.TOKEN);
                        break;
                    }
                    catch (Discord.Net.HttpException e)
                    {
                        log.Error(string.Format("{0}: {1}", e.GetType().ToString(), e.Message));
                        attempts++;
                    }
                    catch (System.Net.WebException e)
                    {
                        log.Error(string.Format("{0}: {1}", e.GetType().ToString(), e.Message));
                        attempts++;
                    }
                    catch (Exception e) { log.Error(string.Format("{0}: {1}", e.GetType().ToString(), e.Message)); attempts++; }
                }
                if (attempts == 0)
                {
                    log.Info("Connected");
                }
                else if (attempts == 3) //Failed to connect 3 times; check for errors in config file
                {
                    log.Warn("Failed to connect. Exiting");
                    Environment.Exit(1);
                }
            });
        }

        private static void closeLog(StreamWriter logfile)
        {
            logfile.Close();
        }

        private void logToFile(string message, StreamWriter logfile)
        {

        }

        /// <summary>
        /// Create a FileSystemWatcher to watch for changes in the config file to trigger a reload of the commands and audio
        /// </summary>
        /// <param name="config">Instance of Initializer class</param>
        /// <returns>FileSystemWatcher for the config file</returns>
        private FileSystemWatcher createFileSystemWatcher(Initializer config)
        {
            FileSystemWatcher fsw = new FileSystemWatcher();
            fsw.Path = Directory.GetCurrentDirectory();
            fsw.NotifyFilter = NotifyFilters.LastWrite;
            fsw.Filter = "config.json";
            fsw.Changed += new FileSystemEventHandler(OnChange);
            fsw.EnableRaisingEvents = true;
            return fsw;
        }

        /// <summary>
        /// Called by the FileSystemWatcher when a change in the config file is detected.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void OnChange(object source, FileSystemEventArgs e)
        {
            // Because of how Discord.Net is structured (as far as I understand. I could be wrong) it is impossible to remove commands once added.
            // Therefore, if a change is detected in the config file, restart the whole application.
            // Messy, but it works.

            log.Info("Change in config detected; restarting...");
            var fileName = Assembly.GetExecutingAssembly().Location;
            Process.Start(fileName);
            Environment.Exit(0);
        }

        /// <summary>
        /// Create the commands from the commands in the config file for Discord.Net
        /// </summary>
        private void createCommands(Initializer config, DiscordClient _client, Queue<Command> audioQueue)
        {
            foreach (Audio command in config.commands)
            {
                CommandBuilder cb = _client.GetService<CommandService>().CreateCommand(command.Command);
                if (command.Alias != null) { cb.Alias(command.Alias); }
                if (command.Description != null) { cb.Description(command.Description); }
                cb.Do(e =>
               {
                   if (e.User.VoiceChannel != null)
                   {
                       log.Info(command.Path);
                       addAudioToQueue(command.Path, e.User.VoiceChannel, audioQueue);
                       if (!audioPlaying) { sendAudioQueue(audioQueue); }
                       log.Info(string.Format("Received command: {1} from: {0} in {2} on {3}", e.User, command.Command, e.Channel, e.Server));
                   }

               });
            }
            _client.GetService<CommandService>().CreateCommand("ayy")
                .Description("Test if the bost is receiving messages")
                .Do(e =>
                {
                    log.Info(string.Format("Ayy received from: {0} in {1} on {2}", e.User, e.Channel, e.Server));
                    try
                    {
                        //throw new Discord.Net.TimeoutException();
                        e.Channel.SendMessage("lmao");
                        log.Info("Sent lmao");
                    }
                    catch (Discord.Net.TimeoutException er)
                    {
                        log.Error(string.Format("{0}: {1}", er.GetType().ToString(), er.Message));
                    }
                });
        }

        /// <summary>
        /// Send audio to the current _vClient channel
        /// </summary>
        /// <param name="path">Path of the .wav file to send</param>
        private void send(string path, Command current, IAudioClient _vClient)
        {
            int blockSize = 3840; // The size of bytes to read per frame; 1920 for mono
            try
            {
                using (FileStream f = File.Open(path, FileMode.Open))
                {
                    byte[] buffer = new byte[blockSize];
                    while (f.Read(buffer, 0, buffer.Length) > 0)
                    {
                        try { _vClient.Send(buffer, 0, buffer.Length); }
                        catch (Discord.Net.TimeoutException e)
                        {
                            log.Error("Error sending audio data: " + e.Message);
                        }
                    }

                    _vClient.Wait();
                }
            }
            catch (FileNotFoundException e) { log.Error(string.Format("Could not find file; ensure {0} is correct path", path)); }
            catch (Exception e) { log.Error(e); }
        }

        private void addAudioToQueue(string path, Channel voiceChannel, Queue<Command> audioQueue)
        {
            audioQueue.Enqueue(new Command(path, voiceChannel));
        }

        private async void sendAudioQueue(Queue<Command> audioQueue)
        {
            IAudioClient _vClient = null;
            while (audioQueue.Count > 0)
            {
                Command current = audioQueue.Dequeue();
                try { _vClient = await current.VoiceChannel.JoinAudio(); }
                catch (Exception e)
                {
                    log.Error("Error joining channel: " + e.Message);
                    break;
                }
                audioPlaying = true;
                send(current.Path, current, _vClient);
            }
            try { await _vClient.Disconnect(); }
            catch (Exception e)
            {
                log.Error("Error disconnecting: " + e.Message);
                return;
            }
            audioPlaying = false;
        }

        /// <summary>
        /// Represents a single command from a user to add to the audio queue.
        /// </summary>
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
}

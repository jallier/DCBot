using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Diagnostics;
using log4net;

namespace DCBot
{
    /// <summary>
    /// Reads the config file and converts the audio files stored to .wav
    /// </summary>
    class Initializer
    {
        /// <summary>
        /// List of commands stored in the config file
        /// </summary>
        public List<Audio> commands { get; } = new List<Audio>();
        public string TOKEN { get; private set; }
        public char CommandChar { get; private set; }
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Initializer()
        {
            
        }

        public Initializer run()
        {
            readConfig();
            foreach (var audio in commands)
            {
                if (!checkForWAV(audio.Path))
                {
                    convertToWAV(audio.Path);
                }
                audio.Path = Path.ChangeExtension(audio.Path, ".wav");

            }
            return this;
        }

        /// <summary>
        /// Uses ffmpeg to convert files to .wav for easier reading and playing of audio.
        /// </summary>
        /// <param name="path"></param>
        private void convertToWAV(string path)
        {
            log.Info("Starting ffmpeg process to convert audio file");
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = string.Format("-i {0} -ar 48000 {1}.wav", path, Path.ChangeExtension(path, null)),
                UseShellExecute = false
            });
        }

        private bool checkForWAV(string path)
        {
            path = Path.ChangeExtension(path, null);
            if (!File.Exists(path + ".wav"))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read the config file and populate the list with the commands described in the config file.
        /// </summary>
        private void readConfig()
        {
            string inputJSON = File.ReadAllText("config.json");
            JObject json = JObject.Parse(inputJSON);
            TOKEN = (string)json["token"];
            string client_id = (string)json["client_id"];
            generateAddURL(client_id);
            CommandChar = (char)json["command_char"];
            foreach (var command in json["commands"])
            {
                Audio audio = new Audio((string)command["command"], (string)command["path"]);
                if (!File.Exists(audio.Path)) { continue; }
                if (command["alias"] != null)
                {
                    List<string> output = new List<string>();
                    foreach (var item in command["alias"])
                    {
                        output.Add((string)item);
                    }
                    audio.Alias = output.ToArray();
                }
                if (command["description"] != null) { audio.Description = (string)command["description"]; }
                commands.Add(audio);
            }
        }

        private void generateAddURL(string client_id)
        {
            string base_string = "https://discordapp.com/oauth2/authorize?scope=bot&permissions=0&client_id=";
            log.Info("If you haven't done so, visit this URL to add the bot to your server");
            log.Info(base_string + client_id);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Diagnostics;

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
        public char CommandChar { get; }
        private char commandChar;
        public Initializer()
        {
            run();
            CommandChar = commandChar;
        }

        public void run()
        {
            readConfig();
            bool convertedWAV = false;
            foreach (var audio in commands)
            {
                if (!checkForWAV(audio.Path))
                {
                    convertToWAV(audio.Path);
                    convertedWAV = true;
                }
                else
                {
                    audio.Path = Path.ChangeExtension(audio.Path, ".wav");
                }
            }
            if (convertedWAV)
            {
                Console.WriteLine("Converted files to appropriate format; please rerun DCBot to start. Press enter to continue");
                Console.ReadLine();
                Environment.Exit(0);
                //Kill the program and ask the user to restart it; This fixes a strange bug where using ffmpeg to convert to
                //WAV would cause static to play when the command is called.
            }
        }

        /// <summary>
        /// Uses ffmpeg to convert files to .wav for easier reading and playing of audio.
        /// </summary>
        /// <param name="path"></param>
        private void convertToWAV(string path)
        {
            Console.WriteLine(path);
            string file = Path.GetFileNameWithoutExtension(path);
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = string.Format("-i {0} -ar 48000 {1}.wav", path, file),
                UseShellExecute = true
            });
        }

        private bool checkForWAV(string path)
        {
            string file = Path.GetFileNameWithoutExtension(path);
            if (!File.Exists(file + ".wav"))
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
            commandChar = (char)json["command_char"];
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
    }
}

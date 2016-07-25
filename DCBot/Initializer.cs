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
    class Initializer
    {
        public List<Audio> commands { get; } = new List<Audio>();
        public string CommandChar { get; }
        private string commandChar;
        public Initializer()
        {
            run();
            CommandChar = commandChar;
        }

        public void run()
        {
            readConfig();
            foreach (var audio in commands)
            {
                if (!checkForWAV(audio.Path))
                {
                    convertToWAV(audio.Path);
                }
                else
                {
                    audio.Path = Path.ChangeExtension(audio.Path, ".wav");
                }
            }
        }

        private void convertToWAV(string path)
        {
            Console.WriteLine(path);
            string file = Path.GetFileNameWithoutExtension(path);
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = string.Format("-i {0} -ar 48000 {1}.wav", path, file),
                UseShellExecute = false
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

        private void readConfig()
        {
            string inputJSON = File.ReadAllText("config.json");
            JObject json = JObject.Parse(inputJSON);
            commandChar = (string)json["command_char"];
            foreach (var command in json["commands"])
            {
                commands.Add(new Audio((string)command["path"], (string)command["command"]));
            }
        }
    }
}

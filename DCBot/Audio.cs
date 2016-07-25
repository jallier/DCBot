using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCBot
{
    class Audio
    {
        public string Path { get; set; }
        public string Command { get; }
        public Audio(string path, string command)
        {
            Path = path;
            Command = command;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Command, Path);
        }
    }
}

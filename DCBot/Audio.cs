using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCBot
{
    class Audio
    {
        public string Command { get; }
        public string Path { get; set; }
        public string[] Alias { get; set; }
        public string Description { get; set; }
        public Audio(string command, string path, string[] alias = null, string description = null)
        {
            Path = path;
            Command = command;
            Alias = alias;
            Description = description;
        }

        public override string ToString()
        {
            return string.Format("{0} ([{1}]): {2}: {3}", Command, Alias, Description, Path);
        }
    }
}

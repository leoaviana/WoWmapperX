using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;


namespace WoWmapperX.AvaloniaImpl
{
    public class UpdateRelease
    {   
        public string Name { get; set; }
        public string Body { get; set; }

        public string FileName { get; set; }

        public string Version { get; set; }

        public string DownloadURL { get; set; }

        public UpdateRelease(string name, string body, string filename, string version, string url)
        {
            Name = name;
            Body = body;
            FileName = filename;
            Version = version;
            DownloadURL = url;
        }
    }
}
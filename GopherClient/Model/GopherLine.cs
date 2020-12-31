using System;
namespace GopherClient.Model
{
    public class GopherLine
    {

        public string Type { get; set; }
        public string UserDisplay { get; set; }
        public string Selector { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }

        public GopherLine(string type, string userDisplay, string selector, string host, string port)
        {
            Type = type;
            UserDisplay = userDisplay;
            Selector = selector;
            Host = host;
            Port = port;
        }
    }   
}

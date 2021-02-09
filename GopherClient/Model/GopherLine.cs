using System;
using System.Collections.Generic;

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

        public override bool Equals(object obj)
        {
            return obj is GopherLine line &&
                   Type == line.Type &&
                   UserDisplay == line.UserDisplay &&
                   Selector == line.Selector &&
                   Host == line.Host &&
                   Port == line.Port;
        }

        public override int GetHashCode()
        {
            int hashCode = -1725355058;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UserDisplay);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Selector);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Host);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Port);
            return hashCode;
        }
    }   
}

using GopherClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GopherClient.Service
{
    public static class Parser
    {
        public static List<GopherLine> Parse(string content)
        {
            //TODO: Error checking of content

            List<GopherLine> gopherLines = new List<GopherLine>();
            List<string> lines = content.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();
            foreach (var line in lines)
            {
                if (line.Equals(".") || line.Equals("") || line.Equals("i"))
                {
                    break;
                }
                List<string> item = line.Split('\t').ToList();

                // Catch invalid gopherline
                if (item.Count < 4)
                    continue;
                    //throw new ArgumentOutOfRangeException();

                GopherLine gopherLine = new GopherLine(item[0].Substring(0, 1),
                    item[0].Substring(1),
                    item[1],
                    item[2],
                    item[3]);
                gopherLines.Add(gopherLine);
            }
            return gopherLines;
        }

        public static string[] CheckAddress(string address)
        {
            string pattern = @"(gopher:\/\/)?([a-zA-z\.]+)(?:\:([0-9]{1,4}))?(.*)?";
            string[] result = Regex.Split(address, pattern);
            return result;
        }
    }
}

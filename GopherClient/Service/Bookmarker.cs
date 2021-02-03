using GopherClient.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GopherClient.Service
{
    public static class Bookmarker
    {
        public static List<GopherLine> LoadBookmarks()
        {
            var path = Directory.GetCurrentDirectory();
            try
            {
                var bookmarksFile = Path.Combine(path, "bookmarks.txt");
                List<GopherLine> bookmarks = new List<GopherLine>();

                using (var sr = new StreamReader(bookmarksFile))
                {
                    while (sr.Peek() >= 0)
                    {
                        string[] rawLine = sr.ReadLine().Split(';');
                        var line = new GopherLine(rawLine[0], rawLine[1], rawLine[2], rawLine[3], rawLine[4]);
                        bookmarks.Add(line);
                    }
                }
                return bookmarks;
            }
            catch (Exception)
            {
                throw new ArgumentException();
            }
        }

        public static void SaveBookmark(GopherLine line)
        {
            var path = Directory.GetCurrentDirectory();
            var bookmarksFile = Path.Combine(path, "bookmarks.txt");
            //Create file if not exists
            var fs = File.AppendText(bookmarksFile);
            using (fs)
            {
                var rawLine = $"{line.Type};{line.UserDisplay};{line.Selector};{line.Host};{line.Port}";
                fs.WriteLine(rawLine);
            }

        }

        private static void DeleteBookmarks()
        {
            var path = Directory.GetCurrentDirectory();
            var bookmarksFile = Path.Combine(path, "bookmarks.txt");
            File.Delete(bookmarksFile);
        }

        public static void CreateInitialBookmarks()
        {
            var path = Directory.GetCurrentDirectory();
            var bookmarksFile = Path.Combine(path, "bookmarks.txt");

            //Create file if not exists
            if (!File.Exists(bookmarksFile))
            {
                var fs = File.Create(bookmarksFile);
                using (var sw = new StreamWriter(fs))
                {
                    var line = "1;FloodGap;;gopher.floodgap.com;70";
                    //var line = new GopherLine("1", "FloodGap", "", "gopher.floodgap.com", "70");
                    sw.WriteLine(line);
                }
            }
        }
    }
}

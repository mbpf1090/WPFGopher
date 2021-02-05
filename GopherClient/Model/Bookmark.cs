using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GopherClient.Model
{
    [Table(Name = "Bookmark")]
    public class Bookmark
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }
        [Column, NotNull]
        public string Title { get; set; }
        [Column, NotNull]
        public DateTime CreatedAt { get; set; }
        [Column, NotNull]
        public string Type { get; set; }
        [Column]
        public string UserDisplay { get; set; }
        [Column]
        public string Selector { get; set; }
        [Column, NotNull]
        public string Host { get; set; }
        [Column]
        public int Port { get; set; }

        public Bookmark() { }

        public Bookmark(GopherLine line)
        {
            CreatedAt = DateTime.Now;
            Title = line.UserDisplay;
            Type = line.Type;
            UserDisplay = line.UserDisplay;
            Selector = line.Selector;
            Host = line.Host;
            Port = int.Parse(line.Port);
        }
    }
}

using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GopherClient.Model
{
    public class GopherDB : DataConnection
    {
        public GopherDB() : base("Default") { }
        public ITable<Bookmark> Bookmark => GetTable<Bookmark>();
    }
}

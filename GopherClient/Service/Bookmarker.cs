using GopherClient.Model;
using LinqToDB;
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
        public static Task<List<Bookmark>> LoadBookmarks()
        {
            using (var db = new GopherDB())
            {
                
                return db.GetTable<Bookmark>().ToListAsync();
            }
        }
    }
}

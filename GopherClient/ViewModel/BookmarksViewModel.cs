using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GopherClient.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;

namespace GopherClient.ViewModel
{
    public class BookmarksViewModel : ViewModelBase
    {
        #region Properties
        private ObservableCollection<Bookmark> _bookmarks;
        public ObservableCollection<Bookmark> Bookmarks 
        {
            get => _bookmarks;
            set
            {
                _bookmarks = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand<Bookmark> SaveBookmarkCmd { get; set; }
        public RelayCommand<Bookmark> DeleteBookmarkCmd { get; set; }

        #endregion
        public BookmarksViewModel()
        {
            SaveBookmarkCmd = new RelayCommand<Bookmark>(SaveBookmark);
            DeleteBookmarkCmd = new RelayCommand<Bookmark>(DeleteBookmark);

            UpdateBookmarksList();
        }

        #region Methods
        private void SaveBookmark(Bookmark bookmark)
        {
            using (var db = new GopherDB())
            {
                db.Update(bookmark);
            }
            UpdateBookmarksList();
        }

        private void DeleteBookmark(Bookmark bookmark)
        {
            using (var db = new GopherDB())
            {
                db.Bookmark.Where(b => b.Id == bookmark.Id).Delete();
            }
            UpdateBookmarksList();
        }

        private void UpdateBookmarksList()
        {
            using (var db = new GopherDB())
            {
                Bookmarks = new ObservableCollection<Bookmark>(db.GetTable<Bookmark>().ToList());
            }
        }
        #endregion
    }
}

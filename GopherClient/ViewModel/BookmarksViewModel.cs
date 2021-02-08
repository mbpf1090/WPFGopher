﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GopherClient.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using GopherClient.View;

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

        public RelayCommand<Bookmark> OpenLineCmd { get; set; }
        public RelayCommand<Bookmark> EditBookmarkCmd { get; set; }
        public RelayCommand<Bookmark> DeleteBookmarkCmd { get; set; }

        #endregion
        public BookmarksViewModel()
        {
            OpenLineCmd = new RelayCommand<Bookmark>(OpenLine);
            EditBookmarkCmd = new RelayCommand<Bookmark>(EditBookmark);
            DeleteBookmarkCmd = new RelayCommand<Bookmark>(DeleteBookmark);
            
            if (IsInDesignMode)
            {
                Bookmarks = new ObservableCollection<Bookmark> 
                { 
                    new Bookmark { CreatedAt = DateTime.Now, Host = "taz.de", Id = 0, Port = 70, Selector = "/", Title = "TAZ", Type = "1", UserDisplay = ""},
                    new Bookmark { CreatedAt = DateTime.Now, Host = "taz.de", Id = 0, Port = 70, Selector = "/", Title = "TAZ ist ein viel laengerer Titel den man nicht lsen klann", Type = "1", UserDisplay = ""}
                };
            }
            else
            { 
                UpdateBookmarksList();
            } 

        }

        #region Methods
        private void OpenLine(Bookmark bm)
        {
            GopherLine line = new GopherLine(bm.Type, bm.UserDisplay, bm.Selector, bm.Host, bm.Port.ToString());
            MessengerInstance.Send<GopherLine>(line);
        }

        private void EditBookmark(Bookmark bookmark)
        {
            BookmarkEditView editWindow = new BookmarkEditView
            {
                Owner = App.Current.MainWindow
                
            };

            ((BookmarkEditViewModel)editWindow.DataContext).Bookmark = bookmark;

            editWindow.ShowDialog();
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

        public void UpdateBookmarksList()
        {
            using (var db = new GopherDB())
            {
                Bookmarks = new ObservableCollection<Bookmark>(db.GetTable<Bookmark>().ToList());
            }
        }
        #endregion
    }
}

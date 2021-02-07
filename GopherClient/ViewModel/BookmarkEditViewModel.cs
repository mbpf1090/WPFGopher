using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GopherClient.Model;
using GopherClient.View;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GopherClient.ViewModel
{
    public class BookmarkEditViewModel : ViewModelBase
    {
        private Bookmark _bookmark;
        public Bookmark Bookmark {

            get => _bookmark;
            set
            {
                _bookmark = value;
                RaisePropertyChanged();
            } 
        }
        public RelayCommand<Window> CancelCmd { get; set; }
        public RelayCommand<Window> SaveCmd { get; set; }

        public BookmarkEditViewModel()
        {
            CancelCmd = new RelayCommand<Window>(Cancel);
            SaveCmd = new RelayCommand<Window>(Save);
        }

        private void Save(Window window)
        {
            using (var db = new GopherDB())
            {
                db.Update(Bookmark);
            }
            window.Close();

        }

        private void Cancel(Window window)
        {
            window.Close();
        }
    }
}

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GopherClient.ViewModel
{
    public class SearchViewModel : ViewModelBase
    {
        private string _searchTerm;
        public string SearchTerm 
        {
            get => _searchTerm; 
            set
            {
                _searchTerm = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand<Window> CancelCmd { get; set; }
        public RelayCommand<Window> OkCmd { get; set; }

        public SearchViewModel()
        {
            CancelCmd = new RelayCommand<Window>(Cancel);
            OkCmd = new RelayCommand<Window>(Ok);
        }

        private void Cancel(Window window)
        {
            Debug.WriteLine("Cancel");
            window.Close();
        }

        private void Ok(Window window)
        {
            Debug.WriteLine("Ök");
            if (SearchTerm != null && !SearchTerm.Equals(""))
            {
                MessengerInstance.Send<string>(SearchTerm);
                window.Close();
            }
        }
    }
}
